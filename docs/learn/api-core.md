# API Reference: Core Types

Reference for core classes and types in Xcaciv.Command framework.

## CommandResult&lt;T&gt;

Factory class for creating command results.

```csharp
public static class CommandResult<T>
{
    public static IResult<T> Success(T output);
    public static IResult<T> Failure(string errorMessage);
}
```

### Methods

#### Success(T output)
Creates a successful result.

**Parameters:**
- `output` (T): The result value

**Returns:** `IResult<T>` - Success result

**Example:**
```csharp
public override async IAsyncEnumerable<IResult<string>> Main(
    IIoContext ioContext,
    IEnvironmentContext env)
{
    yield return CommandResult<string>.Success("Command executed successfully");
}
```

#### Failure(string errorMessage)
Creates a failure result.

**Parameters:**
- `errorMessage` (string): Error description

**Returns:** `IResult<T>` - Failure result

**Example:**
```csharp
if (string.IsNullOrEmpty(filename))
{
    yield return CommandResult<string>.Failure("Filename is required");
}
```

---

## AbstractCommand

Base class for implementing commands.

```csharp
public abstract class AbstractCommand : ICommandDelegate
{
    public static void SetHelpService(IHelpService helpService);

    public virtual ValueTask DisposeAsync();

    public virtual string Help(string[] parameters, IEnvironmentContext env);

    public virtual string OneLineHelp(string[] parameters);

    public async IAsyncEnumerable<IResult<string>> Main(IIoContext io, IEnvironmentContext environment);

    public Dictionary<string, IParameterValue> ProcessParameters(string[] parameters, bool hasPipedInput = false);

    protected CommandParameterOrderedAttribute[] GetOrderedParameters(bool hasPipedInput);

    protected CommandParameterNamedAttribute[] GetNamedParameters(bool hasPipedInput);

    protected CommandFlagAttribute[] GetFlagParameters();

    protected CommandParameterSuffixAttribute[] GetSuffixParameters(bool hasPipedInput);

    public abstract string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env);

    public abstract string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env);

    protected virtual void OnStartPipe(Dictionary<string, IParameterValue> processedParameters, IEnvironmentContext environment);

    protected virtual void OnEndPipe(Dictionary<string, IParameterValue> processedParameters, IEnvironmentContext environment);
}
```

### Methods

#### HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
Core command logic for non-pipelined execution. Uses the typed parameter dictionary produced by `ProcessParameters`.

**Parameters:**
- `parameters` (`Dictionary<string, IParameterValue>`): Parsed parameters
- `env` (`IEnvironmentContext`): Environment context

**Returns:** (string) Command output

**Override:** Required for every command

**Example:**
```csharp
public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
{
    var value = parameters.TryGetValue("input", out var inputParam) && inputParam.IsValid
        ? inputParam.GetValue<string>()
        : string.Empty;

    return value.ToUpperInvariant();
}
```

#### HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
Processes a single chunk from piped input.

**Parameters:**
- `pipedChunk` (string): Output from previous command
- `parameters` (`Dictionary<string, IParameterValue>`): Parsed parameters
- `env` (`IEnvironmentContext`): Environment context

**Returns:** (string) Transformed output

**Override:** Required; return `string.Empty` when not handling piped input

**Example:**
```csharp
public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
{
    return pipedChunk.Trim();
}
```

#### Main(IIoContext io, IEnvironmentContext env)
Framework-provided pipeline orchestrator. It processes parameters, handles help requests, and routes execution to `HandleExecution` or `HandlePipedChunk`. This method is not virtual and should not be overridden.

**Behavior:**
- Calls `ProcessParameters` to bind attributes to values
- If piped input is present, invokes `OnStartPipe`, streams chunks through `HandlePipedChunk`, then calls `OnEndPipe`
- Otherwise, routes to `Help` when requested or `HandleExecution`

#### Help(string[] parameters, IEnvironmentContext env)
Returns help text for the command using the configured `IHelpService`. Override only when a custom help format is required.

#### OneLineHelp(string[] parameters)
Returns a single-line description for listing commands. Uses `IHelpService` when configured; can be overridden for custom formatting.

#### DisposeAsync()
Cleanup method for async disposal.

**Returns:** `ValueTask` - Completion task

**Override:** Optional, for resource cleanup

#### ProcessParameters(string[] parameters, bool hasPipedInput = false)
Binds raw parameters into typed values using command attributes. Excludes parameters marked `UsePipe` when piped input is active.

**Returns:** `Dictionary<string, IParameterValue>`

#### OnStartPipe(...) / OnEndPipe(...)
Lifecycle hooks for piped execution. Override to initialize or finalize state around piped processing; call `base` to preserve default behavior.

---

## CommandController

Central orchestrator implementation.

```csharp
public class CommandController : ICommandController
{
    public CommandController();
    public CommandController(ICrawler crawler);
    public CommandController(ICrawler crawler, string restrictedDirectory);
    public CommandController(IVerifiedSourceDirectories packageBinaryDirectories);
    public CommandController(
        ICommandRegistry? commandRegistry,
        ICommandLoader? commandLoader,
        IPipelineExecutor? pipelineExecutor,
        ICommandExecutor? commandExecutor,
        ICommandFactory? commandFactory,
        IServiceProvider? serviceProvider);

    public string HelpCommand { get; set; }
    public IAuditLogger AuditLogger { get; set; }
    public IOutputEncoder OutputEncoder { get; set; }
    public PipelineConfiguration PipelineConfig { get; set; }

    public void RegisterBuiltInCommands();
    public void AddPackageDirectory(string directory);
    public void LoadCommands(string subDirectory = "bin");
    public void AddCommand(string packageKey, ICommandDelegate command, bool modifiesEnvironment = false);
    public void AddCommand(string packageKey, Type commandType, bool modifiesEnvironment = false);
    public void AddCommand(ICommandDescription command);
    public Task Run(string commandLine, IIoContext ioContext, IEnvironmentContext env);
    public Task Run(string commandLine, IIoContext ioContext, IEnvironmentContext env, CancellationToken cancellationToken);
    public Task GetHelpAsync(string command, IIoContext context, IEnvironmentContext env, CancellationToken cancellationToken = default);
    [Obsolete("Use GetHelpAsync() instead. This method will be removed in v3.0.")]
    public void GetHelp(string command, IIoContext context, IEnvironmentContext env);
}
```

### Key Methods

See [ICommandController](api-interfaces.md#icommandcontroller) for interface documentation.

### Properties

#### CommandRegistry
Internal command registry (protected).

---

## EnvironmentContext

Manages environment variables with isolated contexts.

```csharp
public class EnvironmentContext : IEnvironmentContext
{
    public bool HasChanged { get; }
    public Guid Id { get; }
    public string Name { get; set; }
    public Guid? Parent { get; protected set; }

    public EnvironmentContext();
    public EnvironmentContext(Dictionary<string, string> environment);

    public virtual void SetAuditLogger(IAuditLogger auditLogger);
    public virtual void SetValue(string key, string addValue);
    public virtual string GetValue(string key, string defaultValue = "", bool storeDefault = true);
    public Dictionary<string, string> GetEnvironment();
    public void UpdateEnvironment(Dictionary<string, string> dictionary);
    public Task<IEnvironmentContext> GetChild(string[]? childArguments = null);
    public ValueTask DisposeAsync();
}
```

### Methods

See [IEnvironmentContext](api-interfaces.md#ienvironmentcontext) for interface documentation.

### Example

```csharp
var env = new EnvironmentContext();
env.SetValue("USER", "john");
var childEnv = await env.GetChild();
childEnv.SetValue("TEMP", "value");
```

---

## MemoryIoContext

In-memory I/O implementation for testing and non-console scenarios.

```csharp
public class MemoryIoContext : AbstractTextIo
{
    public ConcurrentBag<MemoryIoContext> Children { get; }
    public ConcurrentBag<string> Output { get; }
    public ConcurrentDictionary<string, string> PromptAnswers { get; }

    public MemoryIoContext(string name = "MemoryIo", string[]? parameters = default, Guid parentId = default);

    public override Task<IIoContext> GetChild(string[]? childArguments = null);
    public override Task HandleOutputChunk(string chunk);
    public override Task<string> PromptForCommand(string prompt);
    public override Task<int> SetProgress(int total, int step);
    public override Task SetStatusMessage(string message);
}
```

### Example

```csharp
var ioContext = new MemoryIoContext(parameters: new[] { "arg1", "arg2" });
await controller.Run("SAY hello", ioContext, env);
var output = string.Join(Environment.NewLine, ioContext.Output);
```

---

## PipelineConfiguration

Configuration for pipeline execution behavior.

```csharp
public class PipelineConfiguration
{
    public int MaxChannelQueueSize { get; set; } = 50;

    public PipelineBackpressureMode BackpressureMode { get; set; } = 
        PipelineBackpressureMode.Block;

    public int StageTimeoutSeconds { get; set; } = 0;
}
```

### Properties

#### MaxChannelQueueSize
Maximum items to queue between pipeline stages.

**Type:** int (default: 50)

**Remarks:** Larger values consume more memory but improve throughput

#### BackpressureMode
How to handle queue overflow.

**Type:** `PipelineBackpressureMode` (default: Block)

**Values:**
- `Block` - Wait when queue is full (default)
- `DropOldest` - Remove oldest item when queue is full
- `DropNewest` - Remove newest item when queue is full

#### StageTimeoutSeconds
Timeout for individual pipeline stages in seconds.

**Type:** int (default: 0 = no timeout)

**Remarks:** Set to positive value to detect stuck stages

### Example

```csharp
var executor = new PipelineExecutor();
executor.Configuration.MaxChannelQueueSize = 1000;
executor.Configuration.BackpressureMode = PipelineBackpressureMode.Block;
executor.Configuration.StageTimeoutSeconds = 30;
```

---

## PipelineBackpressureMode

Enumeration for pipeline backpressure handling.

```csharp
public enum PipelineBackpressureMode
{
    Block = 0,
    DropOldest = 1,
    DropNewest = 2
}
```

### Values

| Value | Behavior |
|-------|----------|
| `Block` | Wait when queue is full (default) |
| `DropOldest` | Remove oldest item when queue is full |
| `DropNewest` | Remove newest item when queue is full |

---

## CommandDescription

Metadata container for a command.

```csharp
public class CommandDescription : ICommandDescription
{
    public string BaseCommand { get; }
    public string FullTypeName { get; }
    public PackageDescription PackageDescription { get; }
    public IReadOnlyDictionary<string, ICommandDescription> SubCommands { get; }
    public bool ModifiesEnvironment { get; }
}
```

### Properties

See [ICommandDescription](api-interfaces.md#icommanddescription) for interface documentation.

---

## PackageDescription

Metadata for a plugin package.

```csharp
public class PackageDescription
{
    public string Name { get; }
    public string FullPath { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
```

### Properties

#### Name
Package name.

**Type:** string

#### FullPath
Full directory path to package.

**Type:** string

#### Metadata
Additional package metadata.

**Type:** `IReadOnlyDictionary<string, string>`

---

## Built-in Command Classes

### SayCommand

Outputs text.

```csharp
[CommandRegister("SAY", "Output text")]
public class SayCommand : AbstractCommand
```

**Usage:**
```
SAY Hello World
SAY "Multiple words"
```

### SetCommand

Sets environment variable.

```csharp
[CommandRegister("SET", "Set environment variable")]
public class SetCommand : AbstractCommand
```

**Usage:**
```
SET MYVAR myvalue
SET --name MYVAR --value myvalue
```

### EnvCommand

Displays environment variables.

```csharp
[CommandRegister("ENV", "Display environment variables")]
public class EnvCommand : AbstractCommand
```

**Usage:**
```
ENV
```

### RegifCommand

Filters by regular expression.

```csharp
[CommandRegister("REGIF", "Filter lines matching regex")]
public class RegifCommand : AbstractCommand
```

**Usage:**
```
SAY line1 | REGIF ^l
```

### HelpCommand

Displays help.

```csharp
[CommandRegister("HELP", "Display command help")]
public class HelpCommand : AbstractCommand
```

**Usage:**
```
HELP
HELP MYCOMMAND
MYCOMMAND --HELP
```

---

## See Also

- [API Interfaces](api-interfaces.md)
- [API Attributes](api-attributes.md)
- [Exception Types](api-exceptions.md)
