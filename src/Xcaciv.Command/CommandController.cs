using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xcaciv.Command.FileLoader;
using Xcaciv.Command.Interface;
using Xcaciv.Loader;

namespace Xcaciv.Command;

/// <summary>
/// Command Manager
/// </summary>
public class CommandController : Interface.ICommandController
{
    protected const string PIPELINE_CHAR = "|";
    protected ICrawler Crawler;

    /// <summary>
    /// registered command
    /// </summary>
    protected Dictionary<string, CommandDescription> Commands { get; set; } = new Dictionary<string, CommandDescription>();
    /// <summary>
    /// restricted directories containing command packages
    /// if no restrictedDirectory is specified the current running directory will be used
    /// </summary>
    protected IVerfiedSourceDirectories PackageBinaryDirectories { get; set; } = new VerfiedSourceDirectories(new FileSystem());

    public string HelpCommand { get; set; } = "HELP";
    /// <summary>
    /// Command Manger
    /// </summary>
    public CommandController() : this(new Crawler()) { }
    /// <summary>
    /// Command Manger
    /// </summary>
    public CommandController(ICrawler crawler) 
    {
        this.Crawler = crawler;
    }
    /// <summary>
    /// Command Manager constructor to specify restricted directory
    /// </summary>
    /// <param name="restrictedDirectory"></param>
    public CommandController(ICrawler crawler,string restrictedDirectory) : this(crawler)
    {
        this.PackageBinaryDirectories.SetRestrictedDirectory(restrictedDirectory);
    }
    /// <summary>
    /// Command Manager test constructor
    /// </summary>
    /// <param name="packageBinearyDirectories"></param>
    public CommandController(IVerfiedSourceDirectories packageBinearyDirectories) : this()
    {
        this.PackageBinaryDirectories = packageBinearyDirectories;
    }
    /// <summary>
    /// add a base directory for loading command packages
    /// default directory structure for a plugin will be:
    ///     directory/PluginName/bin/PluginName.dll
    /// </summary>
    /// <param name="directory"></param>
    public void AddPackageDirectory(string directory)
    {
        this.PackageBinaryDirectories.AddDirectory(directory);
    }
    /// <summary>
    /// (re)populate command collection using a crawler
    /// </summary>
    /// <param name="subDirectory"></param>
    /// <exception cref="Exceptions.InValidConfigurationException"></exception>
    public void LoadCommands(string subDirectory = "bin")
    {
        if (this.PackageBinaryDirectories.Directories.Count == 0) throw new Exceptions.NoPluginsFoundException("No base package directory configured. (Did you set the restricted directory?)");

        foreach (var directory in this.PackageBinaryDirectories.Directories)
        {
            foreach (var command in Crawler.LoadPackageDescriptions(directory, subDirectory).SelectMany(o => o.Value.Commands))
            {
                AddCommand(command.Value);
            }
        }
    }
    /// <summary>
    /// load the built in commands
    /// </summary>
    public void EnableDefaultCommands()
    {
        // This is the package action
        var key = "Default";
        var packagDescription = new PackageDescription()
        {
            Name = key,
            FullPath = "",
        };

        AddCommand(new CommandDescription()
        {
            BaseCommand = "REGIF",
            FullTypeName = "Xcaciv.Command.Commands.RegifCommand",
            PackageDescription = new PackageDescription()
            {
                Name = key,
                FullPath = ""
            }
        });

        AddCommand(new CommandDescription()
        {
            BaseCommand = "SAY",
            FullTypeName = "Xcaciv.Command.Commands.SayCommand",
            PackageDescription = new PackageDescription()
            {
                Name = key,
                FullPath = ""
            }
        });
    }
    /// <summary>
    /// install a single command into the index
    /// </summary>
    /// <param name="command"></param>
    public void AddCommand(CommandDescription command)
    {
        var key = command.BaseCommand.ToUpper();
        this.Commands[key] = command;
    }

    /// <summary>
    /// parse a command line, find and execute the command passing in the arguments
    /// </summary>
    /// <param name="commandLine"></param>
    public async Task Run(string commandLine, ITextIoContext ioContext)
    {
        // parse the command line before processing the context
        // check to see if it is a pipeline
        if (commandLine.Contains(PIPELINE_CHAR))
        {
            await PipelineTheBitch(commandLine, ioContext);
        }
        else
        {
            var commandName = GetCommand(commandLine);
            var args = PrepareArgs(commandLine);
            await using (var childContext = await ioContext.GetChild(args))
                await ExecuteCommand(commandName, childContext);
            
        }        
    }

    protected async Task PipelineTheBitch(string commandLine, ITextIoContext ioContext)
    {
        var tasks = new List<Task>();
        var pipeChannel = null as Channel<string>;

        // split the command line into commands by the pipeline character
        foreach(var command in commandLine.Split(PIPELINE_CHAR))
        {
            var commandName = GetCommand(command);
            var args = PrepareArgs(command);
            await using var childContext = await ioContext.GetChild(args);
            // if not the first command in the pipeline, set the read pipe
            if (pipeChannel != null)
            {
                childContext.SetInputPipe(pipeChannel.Reader);
            }

            // set the write pipe
            pipeChannel = Channel.CreateUnbounded<string>();
            childContext.SetOutputPipe(pipeChannel.Writer);

            tasks.Add(ExecuteCommand(commandName, childContext));
        }

        await Task.WhenAll(tasks);

        await foreach (var output in (pipeChannel ?? Channel.CreateBounded<string>(0)).Reader.ReadAllAsync())
        {
            await ioContext.OutputChunk(output);
        }
    }

    /// <summary>
    /// execute command event
    /// </summary>
    /// <param name="commandKey"></param>
    /// <param name="args"></param>
    /// <param name="ioContext"></param>
    protected async Task ExecuteCommand(string commandKey, ITextIoContext ioContext)
    {
        if (!this.Commands.ContainsKey(commandKey))
        {
            if (commandKey == this.HelpCommand)
            {
                this.GetHelp(string.Empty, ioContext);
            }
            else
            {
                await ioContext.SetStatusMessage($"Command [{commandKey}] not found. Try typing '{this.HelpCommand}'");
            }
            return;
        }

        try
        {
            var commandDiscription = this.Commands[commandKey];
            
            ICommandDelegate commandInstance;
            commandInstance = GetCommandInstance(commandDiscription);

            await ExecuteCommand(ioContext, commandInstance);
        }
        catch (Exception ex)
        {
            await ioContext.SetStatusMessage(ex.ToString());
        }
        finally
        {
            await ioContext.AddTraceMessage($"ExecuteCommand: {commandKey} Done.");
        }
    }

    protected static ICommandDelegate GetCommandInstance(CommandDescription commandDiscription)
    {
        var executeDeligateType = Type.GetType(commandDiscription.FullTypeName);
        ICommandDelegate commandInstance;
        if (executeDeligateType == null)
        {
            using (var context = new AssemblyContext(commandDiscription.PackageDescription.FullPath, basePathRestriction:"*"))
            {
                commandInstance = context.CreateInstance<ICommandDelegate>(commandDiscription.FullTypeName);
            }
        }
        else
        {
            commandInstance = AssemblyContext.ActivateInstance<ICommandDelegate>(executeDeligateType);
        }

        return commandInstance;
    }

    protected static async Task ExecuteCommand(ITextIoContext ioContext, ICommandDelegate commandInstance)
    {
        await foreach (var resultMessage in commandInstance.Main(ioContext, ioContext))
        {
            await ioContext.OutputChunk(resultMessage);
        }
    }

    /// <summary>
    /// parse primary command from a command line
    /// </summary>
    /// <param name="commandLine">full command line</param>
    /// <returns></returns>
    public static string GetCommand(string commandLine)
    {
        commandLine= commandLine.Trim();
        var commandText = ((commandLine.Contains(' ')) ?
                commandLine.Substring(0, commandLine.Trim().IndexOf(' '))
                 : commandLine).ToUpper();

        // remove invalid characters
        return CommandDescription.InvalidCommandChars.Replace(commandText.Trim(), "");
    }
    /// <summary>
    /// parses arguments from a command line
    /// </summary>
    /// <param name="commandLine">full command line</param>
    /// <returns></returns>
    public static string[] PrepareArgs(string commandLine)
    {
        var args = System.Text.RegularExpressions.Regex.Matches(commandLine, @"[\""].*?[\""]|[\w-]+")
            .Cast<System.Text.RegularExpressions.Match>()
            .Select(o => CommandDescription.InvalidParameterChars.Replace(o.Value, "").Trim('"'))
            .ToArray();

        // the first item in the array is the command
        return args.Skip(1).ToArray();
    }
    /// <summary>
    /// output all the help strings
    /// </summary>
    /// <param name="context"></param>
    public void GetHelp(string command, IOutputContext context)
    {
        if (String.IsNullOrEmpty(command))
            foreach(var description in Commands)
            {
                var cmdInsance = GetCommandInstance(description.Value);
                cmdInsance.Help(context);
            }
        else
        {
            try
            {
                var commandKey = GetCommand(command);
                if (Commands.TryGetValue(commandKey, out CommandDescription? value))
                {
                    var description = value;

                    var cmdInsance = GetCommandInstance(description);
                    cmdInsance.Help(context);
                }
                else
                {
                    context.OutputChunk($"Command [{commandKey}] not found.").Wait();
                }
            }
            catch (Exception ex)
            {
                context.OutputChunk(ex.Message).Wait();
            }
        }
    }

}
