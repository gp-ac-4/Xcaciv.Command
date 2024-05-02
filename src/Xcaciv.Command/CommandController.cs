using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
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
public class CommandController : ICommandController
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

        this.Commands.Clear();
        foreach (var directory in this.PackageBinaryDirectories.Directories)
        {
            foreach (var command in Crawler.LoadPackageDescriptions(directory, subDirectory).SelectMany(o => o.Value.Commands))
            {
                AddCommand(command.Value);
            }
        }
    }
    /// <summary>
    /// install a single command into the index
    /// </summary>
    /// <param name="command"></param>
    public void AddCommand(CommandDescription command)
    {
        this.Commands.Add(command.BaseCommand.ToUpper(), command);
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
            var childContext = await ioContext.GetChild(args);
            await ExecuteCommand(commandName, childContext);
        }        
    }

    private async Task PipelineTheBitch(string commandLine, ITextIoContext ioContext)
    {
        var tasks = new List<Task>();
        var pipeChannel = null as Channel<string>;

        // split the command line into command by the pipeline character
        foreach(var command in commandLine.Split(PIPELINE_CHAR))
        {
            var commandName = GetCommand(command);
            var args = PrepareArgs(command);
            var childContext = await ioContext.GetChild(args);

            // if not the first command in the pipeline, set the read pipe
            if (pipeChannel != null)
            {
                childContext.setInputPipe(pipeChannel.Reader);
            }

            // set the write pipe
            pipeChannel = Channel.CreateUnbounded<string>();
            childContext.SetOutputPipe(pipeChannel.Writer);

            tasks.Add(ExecuteCommand(commandName, childContext));
        }

        // when the last command is done, set the read pipe
        ioContext.setInputPipe((pipeChannel ?? Channel.CreateBounded<string>(0)).Reader);
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// execute command event
    /// </summary>
    /// <param name="commandKey"></param>
    /// <param name="args"></param>
    /// <param name="ioContext"></param>
    private async Task ExecuteCommand(string commandKey, ITextIoContext ioContext)
    {
        if (!this.Commands.ContainsKey(commandKey))
        {
            await ioContext.SetStatusMessage($"Command [{commandKey}] not found.");
            return;
        }

        try
        {
            var commandDiscription = this.Commands[commandKey];
            using (var context = AssemblyContext.LoadFromPath(commandDiscription.PackageDescription.FullPath))
            {
                var commandInstance = context.GetInstance<ICommand>(commandDiscription.FullTypeName);
                await foreach(var resultMessage in commandInstance.Main(ioContext, ioContext))
                {
                    await ioContext.OutputChunk(resultMessage);
                }
            }
        }
        catch (Exception ex)
        {
            await ioContext.SetStatusMessage(ex.ToString());
        }
    }
    /// <summary>
    /// parse primary command from a command line
    /// </summary>
    /// <param name="commandLine">full command line</param>
    /// <returns></returns>
    public static string GetCommand(string commandLine)
    {
        // get the first word in the command line
        var commandText = (
            (commandLine.Contains(' ')) ?
            commandLine.Substring(0, commandLine.IndexOf(' ')) :
            commandLine
            ).ToUpper();

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
        var args = System.Text.RegularExpressions.Regex.Matches(commandLine, @"[\""].+?[\""]|[\w-]+")
            .Cast<System.Text.RegularExpressions.Match>()
            .Select(o => CommandDescription.InvalidCommandChars.Replace(o.Value, ""))
            .ToArray();

        // the first item in the array is the command
        return args.Skip(1).ToArray();
    }

}
