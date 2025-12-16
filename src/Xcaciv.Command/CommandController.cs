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
/// Orchestrates command execution, plugin loading, and pipeline management.
/// Serves as the central controller for discovering, registering, and executing commands
/// (both built-in and from plugin packages).
/// </summary>
/// <remarks>
/// The CommandController is the primary entry point for command execution. It manages:
/// - Command registration from plugin packages (via Crawler)
/// - Command instantiation and execution
/// - Pipeline support for chaining multiple commands
/// - Audit logging for command execution and environment changes
/// 
/// Security: Commands are loaded from verified package directories only (see PackageBinaryDirectories).
/// Plugin assembly loading uses AssemblyContext with security restrictions.
/// </remarks>
public class CommandController : Interface.ICommandController
{
    protected const string PIPELINE_CHAR = "|";
    protected ICrawler Crawler;
    protected IAuditLogger _auditLogger;
    protected IOutputEncoder _outputEncoder;

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
    /// Gets or sets the optional audit logger for command execution and environment changes.
    /// If not provided, defaults to NoOpAuditLogger (no logging).
    /// </summary>
    /// <value>An IAuditLogger implementation, or NoOpAuditLogger if not set.</value>
    /// <remarks>
    /// The audit logger is propagated to child environment contexts, enabling comprehensive
    /// tracking of all command executions and environment variable modifications.
    /// </remarks>
    public IAuditLogger AuditLogger 
    { 
        get => _auditLogger;
        set => _auditLogger = value;
    }

    /// <summary>
    /// Gets or sets the output encoder for encoding command output for specific target systems.
    /// If not provided, defaults to NoOpEncoder (no encoding).
    /// </summary>
    /// <value>An IOutputEncoder implementation, or NoOpEncoder if not set.</value>
    /// <remarks>
    /// The output encoder is applied to all output chunks when they are written via IIoContext.OutputChunk().
    /// Use HtmlEncoder for web UI output, JsonEncoder for JSON API responses, or create custom encoders.
    /// 
    /// Default (NoOpEncoder) provides full backward compatibility—no encoding is applied.
    /// </remarks>
    public IOutputEncoder OutputEncoder
    {
        get => _outputEncoder;
        set => _outputEncoder = value;
    }

    /// <summary>
    /// Gets or sets the pipeline configuration for DoS protection.
    /// Controls bounded channel size, backpressure strategy, and execution timeouts.
    /// </summary>
    /// <value>A PipelineConfiguration with safe defaults (10,000 item queue, DropOldest backpressure).</value>
    /// <remarks>
    /// Default configuration prevents memory exhaustion via unbounded pipeline growth.
    /// Backpressure strategies: DropOldest (default), DropNewest, or Block.
    /// </remarks>
    public PipelineConfiguration PipelineConfig { get; set; } = new PipelineConfiguration();

    /// <summary>
    /// Command Manger
    /// </summary>
    public CommandController() : this(new Crawler()) { }

    /// <summary>
    /// Command Manger
    /// </summary>
    /// <param name="crawler">The ICrawler implementation for plugin discovery.</param>
    /// <remarks>
    /// Allows custom crawler implementations for non-standard plugin loading scenarios.
    /// </remarks>
    public CommandController(ICrawler crawler) 
    {
        this.Crawler = crawler;
        this._auditLogger = new NoOpAuditLogger();
        this._outputEncoder = new NoOpEncoder();
    }

    /// <summary>
    /// Command Manager constructor to specify restricted directory
    /// </summary>
    /// <param name="restrictedDirectory">Base directory for plugin packages. Plugins must be in subdirectories of this path.</param>
    /// <remarks>
    /// The restricted directory enforces security by preventing plugins from being loaded outside this path.
    /// Default plugin structure: restrictedDirectory/PluginName/bin/PluginName.dll
    /// </remarks>
    public CommandController(ICrawler crawler, string restrictedDirectory) : this(crawler)
    {
        this.PackageBinaryDirectories.SetRestrictedDirectory(restrictedDirectory);
    }

    /// <summary>
    /// Command Manager test constructor
    /// </summary>
    /// <param name="packageBinearyDirectories">Custom implementation of IVerfiedSourceDirectories for testing.</param>
    /// <remarks>
    /// This constructor is intended for unit testing scenarios where custom directory verification is needed.
    /// </remarks>
    public CommandController(IVerfiedSourceDirectories packageBinearyDirectories) : this()
    {
        this.PackageBinaryDirectories = packageBinearyDirectories;
    }

    /// <summary>
    /// add a base directory for loading command packages
    /// default directory structure for a plugin will be:
    ///     directory/PluginName/bin/PluginName.dll
    /// </summary>
    /// <param name="directory">Base directory path containing plugin subdirectories.</param>
    /// <remarks>
    /// Expected plugin structure: directory/PluginName/bin/PluginName.dll
    /// Multiple directories can be added via multiple calls to this method.
    /// </remarks>
    public void AddPackageDirectory(string directory)
    {
        this.PackageBinaryDirectories.AddDirectory(directory);
    }

    /// <summary>
    /// (re)populate command collection using a crawler
    /// </summary>
    /// <param name="subDirectory">Subdirectory name where compiled plugins are located (default: "bin").</param>
    /// <exception cref="Interface.Exceptions.NoPluginsFoundException">Thrown if no base package directory is configured.</exception>
    /// <remarks>
    /// This method crawls configured package directories and registers all discovered commands.
    /// Commands must have the CommandRegisterAttribute to be loaded.
    /// Can be called multiple times to reload commands after adding new directories.
    /// </remarks>
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
    /// Registers all built-in commands (Say, Set, Env, Regif).
    /// </summary>
    /// <remarks>
    /// Built-in commands are statically compiled into the framework.
    /// The SetCommand is registered as environment-modifying.
    /// </remarks>
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
    /// Registers a command instance with the controller.
    /// </summary>
    /// <param name="packageKey">Logical grouping key for the command (e.g., plugin name or "Default").</param>
    /// <param name="command">The command instance implementing ICommandDelegate.</param>
    /// <param name="modifiesEnvironment">If true, allows the command to modify environment variables. Default: false.</param>
    /// <remarks>
    /// Only special commands (e.g., SetCommand) should modify the environment.
    /// The command must have the CommandRegisterAttribute for successful registration.
    /// </remarks>
    public void AddCommand(string packageKey, ICommandDelegate command, bool modifiesEnvironment = false)
    {
        var commandType = command.GetType();
        AddCommand(packageKey, commandType, modifiesEnvironment);
    }

    /// <summary>
    /// Registers a command type with the controller.
    /// </summary>
    /// <param name="packageKey">Logical grouping key for the command (e.g., plugin name or "Default").</param>
    /// <param name="commandType">The Type of the command (should implement ICommandDelegate and have CommandRegisterAttribute).</param>
    /// <param name="modifiesEnvironment">If true, allows the command to modify environment variables. Default: false.</param>
    /// <remarks>
    /// Used for internal or linked DLL commands. The command type must have CommandRegisterAttribute
    /// to be successfully registered; without it, a trace warning is emitted.
    /// </remarks>
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
    /// <param name="command">The ICommandDescription containing command metadata and definition.</param>
    /// <remarks>
    /// This is the lowest-level registration method. If the command has sub-commands and a matching
    /// base command already exists, sub-commands are merged into the existing command.
    /// </remarks>
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
    /// <param name="commandLine">The command line to execute (may contain pipes "|" for piped execution).</param>
    /// <param name="ioContext">The IO context for input/output and parameter management.</param>
    /// <param name="env">The environment context for variable management.</param>
    /// <remarks>
    /// Automatically detects pipeline syntax ("|" character) and routes to pipeline execution.
    /// For non-piped commands, calls ExecuteCommand directly.
    /// Audit logger is propagated to the environment context before execution.
    /// 
    /// Example piped command: "Say Hello | Regif"
    /// </remarks>
    public async Task Run(string commandLine, IIoContext ioContext, IEnvironmentContext env)
    {
        // Propagate audit logger to environment context if it's an EnvironmentContext instance
        if (env is EnvironmentContext envContext)
        {
            envContext.SetAuditLogger(_auditLogger);
        }

        // Propagate output encoder to IO context
        ioContext.SetOutputEncoder(_outputEncoder);

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

    /// <summary>
    /// Executes a piped command sequence, managing channel communication between commands.
    /// </summary>
    /// <param name="commandLine">The full pipeline command line (e.g., "command1 arg1 | command2 arg2 | command3").</param>
    /// <param name="ioContext">The IO context for managing pipeline input/output.</param>
    /// <param name="env">The environment context shared across all piped commands.</param>
    /// <remarks>
    /// Creates bounded channels between commands for backpressure and memory safety.
    /// Output from one command feeds as input to the next via channel readers/writers.
    /// </remarks>
    protected async Task PipelineTheBitch(string commandLine, IIoContext ioContext, IEnvironmentContext env)
    {
        var (tasks, outputChannel) = await CreatePipelineStages(commandLine, ioContext, env);
        
        // wait for the pipeline to finish
        await Task.WhenAll(tasks);

        // handle the final output
        await CollectPipelineOutput(outputChannel, ioContext);
    }

    /// <summary>
    /// Creates pipeline stages (tasks and channels) from a pipeline command line.
    /// </summary>
    /// <param name="commandLine">The full pipeline command line.</param>
    /// <param name="ioContext">The IO context for managing pipeline state.</param>
    /// <param name="env">The environment context for all piped commands.</param>
    /// <returns>A tuple of (List of execution tasks, Final output channel).</returns>
    /// <remarks>
    /// Each command in the pipeline becomes a separate task with bounded channels for inter-task communication.
    /// The returned output channel contains the final output from the last command in the pipeline.
    /// </remarks>
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
            // set the write pipe with bounded channel to prevent memory exhaustion
            pipeChannel = Channel.CreateBounded<string>(
                new BoundedChannelOptions(PipelineConfig.MaxChannelQueueSize)
                {
                    FullMode = GetChannelFullMode(PipelineConfig.BackpressureMode)
                });
            childContext.SetOutputPipe(pipeChannel.Writer);

            // add the task to the collection
            tasks.Add(ExecuteCommand(commandName, childContext, env));
        }

        return (tasks, pipeChannel);
    }

    /// <summary>
    /// Convert PipelineBackpressureMode to BoundedChannelFullMode.
    /// </summary>
    /// <param name="mode">The backpressure mode from PipelineConfiguration.</param>
    /// <returns>The corresponding BoundedChannelFullMode enum value.</returns>
    /// <remarks>
    /// Maps DropOldest -> DropOldest, DropNewest -> DropNewest, Block -> Wait.
    /// Defaults to DropOldest for unknown values.
    /// </remarks>
    protected BoundedChannelFullMode GetChannelFullMode(PipelineBackpressureMode mode) => mode switch
    {
        PipelineBackpressureMode.DropOldest => BoundedChannelFullMode.DropOldest,
        PipelineBackpressureMode.DropNewest => BoundedChannelFullMode.DropNewest,
        PipelineBackpressureMode.Block => BoundedChannelFullMode.Wait,
        _ => BoundedChannelFullMode.DropOldest
    };

    /// <summary>
    /// Collects output from a pipeline's final output channel and writes to IO context.
    /// </summary>
    /// <param name="outputChannel">The output channel from the last pipeline command (may be null).</param>
    /// <param name="ioContext">The IO context to receive the final output.</param>
    /// <remarks>
    /// If outputChannel is null, creates an empty bounded channel.
    /// Asynchronously reads all output and writes to ioContext.
    /// </remarks>
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
    /// <param name="commandKey">The command name to execute.</param>
    /// <param name="ioContext">The IO context for input/output.</param>
    /// <param name="env">The environment context for variable access.</param>
    /// <remarks>
    /// Handles three scenarios:
    /// 1. Help request (--HELP flag): outputs command help and returns
    /// 2. Unknown command: outputs error message and suggests HELP
    /// 3. Valid command: delegates to ExecuteCommandWithErrorHandling
    /// </remarks>
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

    /// <summary>
    /// Instantiates a command delegate from a command description.
    /// </summary>
    /// <param name="commandDesc">The command description containing type information.</param>
    /// <param name="context">The IO context (used for sub-command parameter handling).</param>
    /// <param name="commandKey">The command key (for error messages).</param>
    /// <returns>An instantiated ICommandDelegate ready for execution.</returns>
    /// <exception cref="InvalidOperationException">Thrown if command instantiation fails.</exception>
    /// <remarks>
    /// Calls GetCommandInstance internally, wrapping any exceptions in InvalidOperationException
    /// with context information about the failed command.
    /// </remarks>
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

    /// <summary>
    /// Executes a command with comprehensive error handling and audit logging.
    /// </summary>
    /// <param name="commandDesc">The command description to execute.</param>
    /// <param name="ioContext">The IO context for input/output.</param>
    /// <param name="env">The environment context for variable access/modification.</param>
    /// <param name="commandKey">The command name (for logging and error messages).</param>
    /// <remarks>
    /// Wraps command execution in try-catch-finally to:
    /// - Instantiate the command
    /// - Execute Main() with child environment
    /// - Update parent environment if command modified it
    /// - Log execution to audit trail (timestamp, duration, success, error)
    /// - Output user-friendly error messages if execution fails
    /// </remarks>
    protected async Task ExecuteCommandWithErrorHandling(
        ICommandDescription commandDesc,
        IIoContext ioContext,
        IEnvironmentContext env,
        string commandKey)
    {
        var startTime = DateTime.UtcNow;
        var success = false;
        string? errorMessage = null;

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

            success = true;
        }
        catch (Exception ex)
        {
            success = false;
            errorMessage = ex.Message;
            await ioContext.OutputChunk($"Error executing {commandKey} (see trace for more info)");
            await ioContext.SetStatusMessage("**Error: " + ex.Message);
            await ioContext.AddTraceMessage(ex.ToString());
        }
        finally
        {
            await ioContext.AddTraceMessage($"ExecuteCommand: {commandKey} Done.");
            
            // Log command execution to audit trail
            var duration = DateTime.UtcNow - startTime;
            _auditLogger?.LogCommandExecution(
                commandKey,
                ioContext.Parameters ?? Array.Empty<string>(),
                startTime,
                duration,
                success,
                errorMessage);
        }
    }

    /// <summary>
    /// Retrieves a command instance (either already loaded or from a plugin assembly).
    /// </summary>
    /// <param name="commandDiscription">The command description containing type and assembly info.</param>
    /// <param name="context">The IO context (used for handling sub-command parameters).</param>
    /// <returns>An instantiated ICommandDelegate.</returns>
    /// <remarks>
    /// If the command has sub-commands, recursively handles parameter parsing and sub-command lookup.
    /// First parameter is treated as a potential sub-command name.
    /// </remarks>
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

    /// <summary>
    /// Loads and instantiates a command from an assembly (either loaded or from a package DLL).
    /// </summary>
    /// <param name="fullTypeName">The fully qualified type name of the command (e.g., "Xcaciv.Command.Commands.SayCommand").</param>
    /// <param name="packagePath">Path to the package DLL if the type is not already loaded; empty if already loaded.</param>
    /// <returns>An instantiated ICommandDelegate.</returns>
    /// <exception cref="InvalidOperationException">Thrown if type name is empty, assembly cannot be loaded, or security violation occurs.</exception>
    /// <remarks>
    /// First attempts to retrieve the type via Type.GetType(). If not found and packagePath is provided,
    /// uses AssemblyContext to load the assembly with security restrictions (basePathRestriction).
    /// Security: All plugin assemblies are loaded with Default security policy and path restrictions.
    /// </remarks>
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
    /// <param name="command">The command name to get help for. If empty, lists all commands.</param>
    /// <param name="context">The IO context for output.</param>
    /// <param name="env">The environment context (passed to Help() method).</param>
    /// <remarks>
    /// If command is empty: outputs one-line help for all registered commands.
    /// If command is specified: outputs detailed help for that specific command.
    /// Errors are caught and output to the context (e.g., command not found).
    /// </remarks>
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

    /// <summary>
    /// Outputs a one-line summary of a command's purpose.
    /// </summary>
    /// <param name="description">The command description.</param>
    /// <param name="context">The IO context for output.</param>
    /// <remarks>
    /// Used for listing all commands with brief descriptions.
    /// Handles both root commands with sub-commands and leaf commands.
    /// </remarks>
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

    /// <summary>
    /// Instantiates a command and outputs its one-line help.
    /// </summary>
    /// <param name="description">The command description to instantiate and help.</param>
    /// <param name="context">The IO context for output.</param>
    /// <remarks>
    /// Internal helper for OutputOneLineHelp(). Retrieves the command instance
    /// and calls its OneLineHelp() method.
    /// </remarks>
    protected static void OutputOneLineHelpFromInstance(ICommandDescription description, IIoContext context)
    {
        var cmdInsance = GetCommandInstance(description, context);
        context.OutputChunk( cmdInsance.OneLineHelp(context.Parameters) );
    }
}
