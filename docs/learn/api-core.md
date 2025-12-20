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
    public abstract string HandleExecution(string[] parameters, IEnvironmentContext env);
    
    public virtual string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env);
    
    public virtual IAsyncEnumerable<IResult<string>> Main(IIoContext ioContext, IEnvironmentContext env);
    
    public virtual string Help(string[] parameters, IEnvironmentContext env);
    
    public virtual ValueTask DisposeAsync();
    
    // Helpers
    protected virtual string BuildHelpString(string[] parameters, IEnvironmentContext environment);
    
    protected CommandParameterOrderedAttribute[] GetOrderedParameters(bool hasPipedInput);
    
    protected CommandParameterNamedAttribute[] GetNamedParameters(bool hasPipedInput);
    
    protected CommandFlagAttribute[] GetFlagParameters();
    
    protected CommandParameterSuffixAttribute[] GetSuffixParameters(bool hasPipedInput);
}
```

### Methods

#### HandleExecution(string[] parameters, IEnvironmentContext env)
Core command logic for non-pipelined execution.

**Parameters:**
- `parameters` (string[]): Command arguments
- `env` (IEnvironmentContext): Environment context

**Returns:** (string) Command output

**Override:** Required in most commands

**Example:**
```csharp
public override string HandleExecution(string[] parameters, IEnvironmentContext env)
{
    return $"Processed {parameters.Length} parameters";
}
```

#### HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env)
Processes a single chunk from piped input.

**Parameters:**
- `pipedChunk` (string): Output from previous command
- `parameters` (string[]): Command arguments
- `env` (IEnvironmentContext): Environment context

**Returns:** (string) Transformed output

**Override:** Optional, for simple single-line transformations

**Example:**
```csharp
public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env)
{
    return pipedChunk.ToUpper();
}
```

#### Main(IIoContext ioContext, IEnvironmentContext env)
Primary command execution supporting pipelines.

**Parameters:**
- `ioContext` (IIoContext): I/O context with parameters and pipes
- `env` (IEnvironmentContext): Environment context

**Returns:** `IAsyncEnumerable<IResult<string>>` - Async enumerable of results

**Override:** Use for full control over execution and async operations

**Example:**
```csharp
public override async IAsyncEnumerable<IResult<string>> Main(
    IIoContext ioContext,
    IEnvironmentContext env)
{
    if (ioContext.HasPipedInput)
    {
        await foreach (var chunk in ioContext.ReadInputPipeChunks())
        {
            yield return CommandResult<string>.Success(chunk.ToUpper());
        }
    }
    else
    {
        if (ioContext.Parameters.Length > 0)
        {
            yield return CommandResult<string>.Success(ioContext.Parameters[0].ToUpper());
        }
    }
}
```

#### Help(string[] parameters, IEnvironmentContext env)
Returns help text for the command.

**Parameters:**
- `parameters` (string[]): Current parameters
- `env` (IEnvironmentContext): Environment context

**Returns:** (string) Help text

**Override:** Optional, default uses BuildHelpString()

#### DisposeAsync()
Cleanup method for async disposal.

**Returns:** `ValueTask` - Completion task

**Override:** Optional, for resource cleanup

#### BuildHelpString(string[] parameters, IEnvironmentContext environment)
Generates help text from attributes.

**Parameters:**
- `parameters` (string[]): Command parameters
- `environment` (IEnvironmentContext): Environment context

**Returns:** (string) Formatted help text

**Remarks:** Helper method for automatic help generation

#### GetOrderedParameters(bool hasPipedInput)
Gets ordered parameter attributes.

**Parameters:**
- `hasPipedInput` (bool): Whether piped input is available

**Returns:** `CommandParameterOrderedAttribute[]` - Parameter attributes

#### GetNamedParameters(bool hasPipedInput)
Gets named parameter attributes.

**Parameters:**
- `hasPipedInput` (bool): Whether piped input is available

**Returns:** `CommandParameterNamedAttribute[]` - Parameter attributes

#### GetFlagParameters()
Gets flag parameter attributes.

**Returns:** `CommandFlagAttribute[]` - Parameter attributes

#### GetSuffixParameters(bool hasPipedInput)
Gets suffix parameter attributes.

**Parameters:**
- `hasPipedInput` (bool): Whether piped input is available

**Returns:** `CommandParameterSuffixAttribute[]` - Parameter attributes

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
        ICommandRegistry commandRegistry,
        ICommandLoader commandLoader,
        IPipelineExecutor pipelineExecutor,
        ICommandExecutor commandExecutor,
        ICommandFactory commandFactory,
        IServiceProvider serviceProvider);
    
    // Implementation of ICommandController
    public void EnableDefaultCommands();
    public void AddPackageDirectory(string directory);
    public void LoadCommands(string subDirectory = "bin");
    public Task Run(string commandLine, IIoContext output, IEnvironmentContext env);
    public void GetHelp(string command, IIoContext output, IEnvironmentContext env);
    public void AddCommand(ICommandDescription command);
    public void AddCommand(string packageKey, Type commandType, bool modifiesEnvironment = false);
    public void AddCommand(string packageKey, ICommandDelegate command, bool modifiesEnvironment = false);
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
    public EnvironmentContext();
    
    public EnvironmentContext(IDictionary<string, string> variables);
    
    public string GetVariable(string name);
    
    public void SetVariable(string name, string value);
    
    public bool VariableExists(string name);
    
    public void RemoveVariable(string name);
    
    public IReadOnlyDictionary<string, string> GetAllVariables();
    
    public IEnvironmentContext CreateChildContext();
}
```

### Methods

See [IEnvironmentContext](api-interfaces.md#ienvironmentcontext) for interface documentation.

### Example

```csharp
var env = new EnvironmentContext();
env.SetVariable("USER", "john");
env.SetVariable("APP_NAME", "MyApp");

string user = env.GetVariable("USER");
bool exists = env.VariableExists("USER");

var childEnv = env.CreateChildContext();
childEnv.SetVariable("TEMP", "value");
// Doesn't affect parent unless command has ModifiesEnvironment=true
```

---

## MemoryIoContext

In-memory I/O implementation for testing and non-console scenarios.

```csharp
public class MemoryIoContext : AbstractTextIo
{
    public MemoryIoContext();
    
    public MemoryIoContext(string[] parameters);
    
    public string GetOutput();
    
    public void Clear();
}
```

### Methods

#### GetOutput()
Gets all accumulated output.

**Returns:** (string) All output written via OutputChunk()

#### Clear()
Clears accumulated output.

### Example

```csharp
var ioContext = new MemoryIoContext(new[] { "arg1", "arg2" });

await controller.Run("MYCOMMAND arg1 arg2", ioContext, env);

string output = ioContext.GetOutput();
Console.WriteLine(output);
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
- `Block` - Wait when queue full (prevents data loss)
- `DropOldest` - Remove oldest item when queue full
- `DropNewest` - Remove newest item when queue full

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
