using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection.Metadata;
using System.Security;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xcaciv.Command.Commands;
using Xcaciv.Command.Core;
using Xcaciv.Command.FileLoader;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Exceptions;
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
    protected Dictionary<string, ICommandDescription> Commands { get; set; } = new Dictionary<string, ICommandDescription>();
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
    /// <exception cref="Interface.Exceptions.InValidConfigurationException"></exception>
    public void LoadCommands(string subDirectory = "bin")
    {
        if (this.PackageBinaryDirectories.Directories.Count == 0) throw new NoPluginsFoundException("No base package directory configured. (Did you set the restricted directory?)");

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
        var packageKey = "Default";

        AddCommand(packageKey, new RegifCommand());

        AddCommand(packageKey, new SayCommand());

        AddCommand(packageKey, new SetCommand(), true);
        
        AddCommand(packageKey, new EnvCommand());
    }
    /// <summary>
    /// add a command from an instance of the command
    /// good for commands from internal or linked dlls
    /// </summary>
    /// <param name="packageKey"></param>
    /// <param name="command"></param>
    /// <param name="modifiesEnvironment">only special commands can modify the environment</param>
    public void AddCommand(string packageKey, ICommandDelegate command, bool modifiesEnvironment = false)
    {
        var commandType = command.GetType();
        AddCommand(packageKey, commandType, modifiesEnvironment);
    }
    /// <summary>
    /// add a command from a loaded type
    /// good for  commands from internal or linked dlls
    /// </summary>
    /// <param name="packageKey"></param>
    /// <param name="commandType"></param>
    /// <param name="modifiesEnvironment"></param>
    public void AddCommand(string packageKey, Type commandType, bool modifiesEnvironment = false)
    {
        if (Attribute.GetCustomAttribute(commandType, typeof(CommandRegisterAttribute)) is CommandRegisterAttribute attributes)
        {
            var packageDesc = new PackageDescription()
            {
                Name = packageKey,
                FullPath = commandType.Assembly.Location
            };

            var commandDesc = CommandParameters.CreatePackageDescription(commandType, packageDesc);

            AddCommand(commandDesc);

            return;
        }

        Trace.WriteLine($"{commandType.FullName} implements ICommandDelegate but does not have BaseCommandAttribute. Unable to automatically register.");
    }

    /// <summary>
    /// install a single command into the index
    /// </summary>
    /// <param name="command"></param>
    public void AddCommand(ICommandDescription command)
    {
        // handle sub commands
        if (command.SubCommands.Count > 0 && this.Commands.TryGetValue(command.BaseCommand, out ICommandDescription? description))
        {
            foreach(var subCommand in command.SubCommands)
            {
                description.SubCommands[subCommand.Key] = subCommand.Value;
            }
        }
        else
        {
            this.Commands[command.BaseCommand] = command;
        }
    }

    /// <summary>
    /// parse a command line, find and execute the command passing in the arguments
    /// </summary>
    /// <param name="commandLine"></param>
    public async Task Run(string commandLine, IIoContext ioContext, IEnvironmentContext env)
    {
        // parse the command line before processing the context
        // check to see if it is a pipeline
        if (commandLine.Contains(PIPELINE_CHAR))
        {
            await PipelineTheBitch(commandLine, ioContext, env);
        }
        else
        {
            var commandName = CommandDescription.GetValidCommandName(commandLine);
            var args = CommandDescription.GetArgumentsFromCommandline(commandLine);
            await ioContext.SetParameters([.. args]);

            await using (var childContext = await ioContext.GetChild(ioContext.Parameters))
            {
                await this.ExecuteCommand(commandName, childContext, env);
            }

        }        
    }

    protected async Task PipelineTheBitch(string commandLine, IIoContext ioContext, IEnvironmentContext env)
    {
        var (tasks, outputChannel) = await CreatePipelineStages(commandLine, ioContext, env);
        
        // wait for the pipeline to finish
        await Task.WhenAll(tasks);

        // handle the final output
        await CollectPipelineOutput(outputChannel, ioContext);
    }

    protected async Task<(List<Task>, Channel<string>?)> CreatePipelineStages(
        string commandLine,
        IIoContext ioContext,
        IEnvironmentContext env)
    {
        var tasks = new List<Task>();
        var pipeChannel = null as Channel<string>;

        // split the command line into commands by the pipeline character
        foreach (var command in commandLine.Split(PIPELINE_CHAR))
        {
            var commandName = CommandDescription.GetValidCommandName(command).ToString();
            var args = CommandDescription.GetArgumentsFromCommandline(command);
            // creating a additional layer of IO to manage the pipes
            await using var childContext = await ioContext.GetChild(args);
            // we are not creating a child environment for simplified synchronization
            // if not the first command in the pipeline, set the read pipe
            if (pipeChannel != null)
            {
                childContext.SetInputPipe(pipeChannel.Reader);
            }
            // set the write pipe
            pipeChannel = Channel.CreateUnbounded<string>();
            childContext.SetOutputPipe(pipeChannel.Writer);

            // add the task to the collection
            tasks.Add(ExecuteCommand(commandName, childContext, env));
        }

        return (tasks, pipeChannel);
    }

    protected async Task CollectPipelineOutput(Channel<string>? outputChannel, IIoContext ioContext)
    {
        var channel = outputChannel ?? Channel.CreateBounded<string>(0);
        await foreach (var output in channel.Reader.ReadAllAsync())
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
    protected async Task ExecuteCommand(string commandKey, IIoContext ioContext, IEnvironmentContext env)
    {
        if (Commands.TryGetValue(commandKey, out ICommandDescription? commandDiscription))
        {
            // Check for help request on sub-commands first
            if (commandDiscription.SubCommands.Count > 0 &&
                ioContext.Parameters?.Length > 0 &&
                ioContext.Parameters[0].Equals($"--{this.HelpCommand}", StringComparison.CurrentCultureIgnoreCase))
            {
                OutputOneLineHelp(commandDiscription, ioContext);
                return;
            }

            // Check for help request on main command
            if (ioContext.Parameters?.Length > 0 && 
                ioContext.Parameters[0].Equals($"--{this.HelpCommand}", StringComparison.CurrentCultureIgnoreCase))
            {
                var cmdInstance = InstantiateCommand(commandDiscription, ioContext, commandKey);
                await ioContext.OutputChunk(cmdInstance.Help(ioContext.Parameters, env));
                return;
            }

            // Normal execution with exception handling
            await ExecuteCommandWithErrorHandling(commandDiscription, ioContext, env, commandKey);
        }
        else
        {
            if (commandKey == this.HelpCommand)
            {
                this.GetHelp(string.Empty, ioContext, env);
            }
            else
            {
                var message = $"Command [{commandKey}] not found.";
                await ioContext.OutputChunk($"{message} Try '{this.HelpCommand}'");
                await ioContext.AddTraceMessage(message);
            }
        }
    }

    protected ICommandDelegate InstantiateCommand(
        ICommandDescription commandDesc,
        IIoContext context,
        string commandKey)
    {
        try
        {
            return GetCommandInstance(commandDesc, context);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to instantiate command {commandKey}: {ex.Message}", ex);
        }
    }

    protected async Task ExecuteCommandWithErrorHandling(
        ICommandDescription commandDesc,
        IIoContext ioContext,
        IEnvironmentContext env,
        string commandKey)
    {
        try
        {
            await ioContext.AddTraceMessage($"ExecuteCommand: {commandKey} Start.");

            var commandInstance = InstantiateCommand(commandDesc, ioContext, commandKey);
            
            await using (var childEnv = await env.GetChild(ioContext.Parameters))
            {
                await foreach (var resultMessage in commandInstance.Main(ioContext, childEnv))
                {
                    await ioContext.OutputChunk(resultMessage);
                }
                if (commandDesc.ModifiesEnvironment && childEnv.HasChanged) 
                {
                    env.UpdateEnvironment(childEnv.GetEnvinronment());
                }
            }
        }
        catch (Exception ex)
        {
            await ioContext.OutputChunk($"Error executing {commandKey} (see trace for more info)");
            await ioContext.SetStatusMessage("**Error: " + ex.Message);
            await ioContext.AddTraceMessage(ex.ToString());
        }
        finally
        {
            await ioContext.AddTraceMessage($"ExecuteCommand: {commandKey} Done.");
        }
    }

    protected static ICommandDelegate GetCommandInstance(ICommandDescription commandDiscription, IIoContext context)
    {
        if (commandDiscription.SubCommands.Count > 0 &&
            context.Parameters != null && context.Parameters.Length > 0)
        {
            if (commandDiscription.SubCommands.TryGetValue(context.Parameters[0].ToUpper(), out ICommandDescription? subCommandDescription) && subCommandDescription != null)
            {
                context.SetParameters(context.Parameters[1..]);
                return GetCommandInstance(subCommandDescription.FullTypeName, commandDiscription.PackageDescription.FullPath);
            }
        }

        return GetCommandInstance(commandDiscription.FullTypeName, commandDiscription.PackageDescription.FullPath);        
    }

    protected static ICommandDelegate GetCommandInstance(string fullTypeName, string packagePath)
    {
        if (String.IsNullOrEmpty(fullTypeName)) throw new InvalidOperationException("Command type name is empty.");
        Type? executeDeligateType = Type.GetType(fullTypeName);
        ICommandDelegate commandInstance;
        if (executeDeligateType == null)
        {
            if (String.IsNullOrEmpty(packagePath)) throw new InvalidOperationException($"Command [{fullTypeName}] is not loaded and no assembly was defined.");

            try
            {
                // Use Default security policy - plugins are loaded from verified package directories
                using var context = new AssemblyContext(
                    packagePath,
                    basePathRestriction: Path.GetDirectoryName(packagePath) ?? Directory.GetCurrentDirectory(),
                    securityPolicy: AssemblySecurityPolicy.Default);
                commandInstance = context.CreateInstance<ICommandDelegate>(fullTypeName);
            }
            catch (SecurityException ex)
            {
                throw new InvalidOperationException($"Security violation loading command [{fullTypeName}] from [{packagePath}]: {ex.Message}", ex);
            }
        }
        else
        {
            commandInstance = AssemblyContext.ActivateInstance<ICommandDelegate>(executeDeligateType);
        }

        return commandInstance;
    }

    /// <summary>
    /// output all the help strings
    /// </summary>
    /// <param name="command"></param>
    /// <param name="context"></param>
    /// <param name="env"></param>
    public void GetHelp(string command, IIoContext context, IEnvironmentContext env)
    {
        if (String.IsNullOrEmpty(command))
        {
            foreach (var description in Commands)
            {
                OutputOneLineHelp(description.Value, context);
            }
        }
        else
        {
            try
            {
                var commandKey = CommandDescription.GetValidCommandName(command);
                if (Commands.TryGetValue(commandKey, out ICommandDescription? value))
                {
                    var description = value;

                    var cmdInsance = GetCommandInstance(description, context);
                    context.OutputChunk(cmdInsance.Help(context.Parameters, env));
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

    protected static void OutputOneLineHelp(ICommandDescription description, IIoContext context)
    {
        if (String.IsNullOrEmpty(description.FullTypeName))
        {
            if (description.SubCommands.Count > 0)
            {
                // get the first sub command to get the type to get the root command
                var subCmd = GetCommandInstance(description.SubCommands.First().Value, context);

                if (subCmd != null && Attribute.GetCustomAttribute(subCmd.GetType(), typeof(CommandRootAttribute)) is CommandRootAttribute rootAttribute)
                {
                    context.OutputChunk($"{rootAttribute.Command,-12} {rootAttribute.Description}");
                }

                foreach (var subCommand in description.SubCommands)
                {
                    OutputOneLineHelpFromInstance(subCommand.Value, context);
                }
            }
            else
            {
                context.AddTraceMessage($"No type name registered for command: {description.BaseCommand}");
            }
        }
        else
        {
            OutputOneLineHelpFromInstance(description, context);
        }
    }

    protected static void OutputOneLineHelpFromInstance(ICommandDescription description, IIoContext context)
    {
        var cmdInsance = GetCommandInstance(description, context);
        context.OutputChunk( cmdInsance.OneLineHelp(context.Parameters) );
    }
}
