using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Xcaciv.Command.Commands;
using Xcaciv.Command.FileLoader;
using Xcaciv.Command.Interface;

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

    private readonly ICommandRegistry _commandRegistry;
    private readonly ICommandLoader _commandLoader;
    private readonly IPipelineExecutor _pipelineExecutor;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ICommandFactory _commandFactory;

    protected ICommandRegistry CommandRegistry => _commandRegistry;

    private string _helpCommand = "HELP";
    private IAuditLogger _auditLogger;
    private IOutputEncoder _outputEncoder;

    /// <summary>
    /// Command Manager default constructor.
    /// </summary>
    public CommandController()
        : this(null, null, null, null, null, null)
    {
    }

    /// <summary>
    /// Command Manager constructor allowing a custom crawler.
    /// </summary>
    /// <param name="crawler">The ICrawler implementation for plugin discovery.</param>
    public CommandController(ICrawler crawler)
        : this(
            commandRegistry: null,
            commandLoader: new CommandLoader(crawler, new VerfiedSourceDirectories(new FileSystem())),
            pipelineExecutor: null,
            commandExecutor: null,
            commandFactory: null,
            serviceProvider: null)
    {
    }

    /// <summary>
    /// Command Manager constructor to specify restricted directory.
    /// </summary>
    /// <param name="crawler">Crawler used for plugin discovery.</param>
    /// <param name="restrictedDirectory">Base directory for plugin packages.</param>
    public CommandController(ICrawler crawler, string restrictedDirectory)
        : this(crawler)
    {
        _commandLoader.SetRestrictedDirectory(restrictedDirectory);
    }

    /// <summary>
    /// Command Manager constructor for testing custom directory verification logic.
    /// </summary>
    /// <param name="packageBinearyDirectories">Custom implementation of IVerfiedSourceDirectories.</param>
    public CommandController(IVerfiedSourceDirectories packageBinearyDirectories)
        : this(
            commandRegistry: null,
            commandLoader: new CommandLoader(new Crawler(), packageBinearyDirectories),
            pipelineExecutor: null,
            commandExecutor: null,
            commandFactory: null,
            serviceProvider: null)
    {
    }

    /// <summary>
    /// Dependency-injection friendly constructor for supplying orchestrator components.
    /// </summary>
    public CommandController(
        ICommandRegistry? commandRegistry,
        ICommandLoader? commandLoader,
        IPipelineExecutor? pipelineExecutor,
        ICommandExecutor? commandExecutor,
        ICommandFactory? commandFactory,
        IServiceProvider? serviceProvider)
    {
        _commandRegistry = commandRegistry ?? new CommandRegistry();
        _commandFactory = commandFactory ?? new CommandFactory(serviceProvider);
        _commandExecutor = commandExecutor ?? new CommandExecutor(_commandRegistry, _commandFactory);
        _commandLoader = commandLoader ?? new CommandLoader(new Crawler(), new VerfiedSourceDirectories(new FileSystem()));
        _pipelineExecutor = pipelineExecutor ?? new PipelineExecutor();

        _auditLogger = new NoOpAuditLogger();
        _outputEncoder = new NoOpEncoder();

        _commandExecutor.HelpCommand = _helpCommand;
        _commandExecutor.AuditLogger = _auditLogger;
    }

    public string HelpCommand
    {
        get => _helpCommand;
        set
        {
            _helpCommand = value;
            _commandExecutor.HelpCommand = value;
        }
    }

    /// <summary>
    /// Gets or sets the optional audit logger for command execution and environment changes.
    /// If not provided, defaults to NoOpAuditLogger (no logging).
    /// </summary>
    public IAuditLogger AuditLogger
    {
        get => _auditLogger;
        set
        {
            _auditLogger = value ?? new NoOpAuditLogger();
            _commandExecutor.AuditLogger = _auditLogger;
        }
    }

    /// <summary>
    /// Gets or sets the output encoder for encoding command output for specific target systems.
    /// </summary>
    public IOutputEncoder OutputEncoder
    {
        get => _outputEncoder;
        set => _outputEncoder = value ?? new NoOpEncoder();
    }

    /// <summary>
    /// Gets or sets the pipeline configuration for DoS protection.
    /// </summary>
    public PipelineConfiguration PipelineConfig
    {
        get => _pipelineExecutor.Configuration;
        set => _pipelineExecutor.Configuration = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// add a base directory for loading command packages
    /// </summary>
    /// <param name="directory">Base directory path containing plugin subdirectories.</param>
    public void AddPackageDirectory(string directory)
    {
        _commandLoader.AddPackageDirectory(directory);
    }

    /// <summary>
    /// (re)populate command collection using a crawler
    /// </summary>
    /// <param name="subDirectory">Subdirectory name where compiled plugins are located (default: "bin").</param>
    public void LoadCommands(string subDirectory = "bin")
    {
        _commandLoader.LoadCommands(subDirectory, _commandRegistry.AddCommand);
    }

    /// <summary>
    /// Registers all built-in commands (Say, Set, Env, Regif).
    /// </summary>
    public void EnableDefaultCommands()
    {
        var packageKey = "Default";

        AddCommand(packageKey, new RegifCommand());
        AddCommand(packageKey, new SayCommand());
        AddCommand(packageKey, new SetCommand(), true);
        AddCommand(packageKey, new EnvCommand());
    }

    /// <summary>
    /// Registers a command instance with the controller.
    /// </summary>
    public void AddCommand(string packageKey, ICommandDelegate command, bool modifiesEnvironment = false)
    {
        _commandRegistry.AddCommand(packageKey, command, modifiesEnvironment);
    }

    /// <summary>
    /// Registers a command type with the controller.
    /// </summary>
    public void AddCommand(string packageKey, Type commandType, bool modifiesEnvironment = false)
    {
        _commandRegistry.AddCommand(packageKey, commandType, modifiesEnvironment);
    }

    /// <summary>
    /// install a single command into the index
    /// </summary>
    public void AddCommand(ICommandDescription command)
    {
        _commandRegistry.AddCommand(command);
    }

    /// <summary>
    /// parse a command line, find and execute the command passing in the arguments
    /// </summary>
    public async Task Run(string commandLine, IIoContext ioContext, IEnvironmentContext env)
    {
        if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));
        if (ioContext == null) throw new ArgumentNullException(nameof(ioContext));
        if (env == null) throw new ArgumentNullException(nameof(env));

        if (env is EnvironmentContext envContext)
        {
            envContext.SetAuditLogger(_auditLogger);
        }

        ioContext.SetOutputEncoder(_outputEncoder);

        if (commandLine.Contains(PIPELINE_CHAR))
        {
            await _pipelineExecutor.ExecuteAsync(commandLine, ioContext, env, ExecuteCommandInternal).ConfigureAwait(false);
        }
        else
        {
            var commandName = CommandDescription.GetValidCommandName(commandLine);
            var args = CommandDescription.GetArgumentsFromCommandline(commandLine);
            await ioContext.SetParameters([.. args]).ConfigureAwait(false);

            await using var childContext = await ioContext.GetChild(ioContext.Parameters).ConfigureAwait(false);
            await ExecuteCommandInternal(commandName, childContext, env).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// output all the help strings
    /// </summary>
    public void GetHelp(string command, IIoContext context, IEnvironmentContext env)
    {
        _commandExecutor.GetHelpAsync(command, context, env).GetAwaiter().GetResult();
    }

    private Task ExecuteCommandInternal(string commandKey, IIoContext ioContext, IEnvironmentContext env)
    {
        return _commandExecutor.ExecuteAsync(commandKey, ioContext, env);
    }
}
