# API Reference: Interfaces

Complete reference for all public interfaces in Xcaciv.Command framework.

## ICommandController

Central interface for command execution and plugin management.

```csharp
public interface ICommandController
{
    // Enable built-in commands
    void EnableDefaultCommands();
    
    // Plugin directory management
    void AddPackageDirectory(string directory);
    void LoadCommands(string subDirectory = "bin");
    
    // Command execution
    Task Run(string commandLine, IIoContext output, IEnvironmentContext env);
    
    // Help system
    void GetHelp(string command, IIoContext output, IEnvironmentContext env);
    
    // Manual command registration
    void AddCommand(ICommandDescription command);
    void AddCommand(string packageKey, Type commandType, bool modifiesEnvironment = false);
    void AddCommand(string packageKey, ICommandDelegate command, bool modifiesEnvironment = false);
}
```

### Methods

#### EnableDefaultCommands()
Registers built-in commands:
- `SAY` - Output text
- `SET` - Set environment variable
- `ENV` - Display environment variables  
- `REGIF` - Regular expression filter
- `HELP` - Display help

#### AddPackageDirectory(string directory)
Adds a directory where the framework searches for plugin packages.

**Parameters:**
- `directory` (string): Path to plugin directory

**Example:**
```csharp
controller.AddPackageDirectory("/opt/plugins");
```

#### LoadCommands(string subDirectory = "bin")
Discovers and loads commands from plugin directories.

**Parameters:**
- `subDirectory` (string, optional): Subdirectory within each plugin to search (default: "bin")

**Example:**
```csharp
controller.LoadCommands(); // searches bin/ subdirectories
controller.LoadCommands("lib"); // searches lib/ subdirectories
```

#### Run(string commandLine, IIoContext output, IEnvironmentContext env)
Executes a command line, with support for pipelines.

**Parameters:**
- `commandLine` (string): Command to execute, e.g., "MYCOMMAND arg1 | FILTER pattern"
- `output` (IIoContext): I/O context for input/output
- `env` (IEnvironmentContext): Environment context for variables

**Example:**
```csharp
await controller.Run("SAY Hello | UPPERCASE", ioContext, envContext);
```

#### GetHelp(string command, IIoContext output, IEnvironmentContext env)
Outputs help text.

**Parameters:**
- `command` (string): Command name (empty string for all commands)
- `output` (IIoContext): Where to write help
- `env` (IEnvironmentContext): Environment context

**Example:**
```csharp
controller.GetHelp("MYCOMMAND", ioContext, envContext);
```

---

## ICommandDelegate

Contract for executable commands.

```csharp
public interface ICommandDelegate : IAsyncDisposable
{
    IAsyncEnumerable<IResult<string>> Main(IIoContext ioContext, IEnvironmentContext env);
    string Help(string[] parameters, IEnvironmentContext env);
}
```

### Methods

#### Main(IIoContext ioContext, IEnvironmentContext env)
Primary command execution method.

**Parameters:**
- `ioContext` (IIoContext): I/O context with parameters and pipe access
- `env` (IEnvironmentContext): Environment context (isolated child if command doesn't modify environment)

**Returns:** `IAsyncEnumerable<IResult<string>>` - Yields result objects with output or errors

**Remarks:**
- Use `yield return` to produce output
- Each yield becomes a separate pipeline chunk
- Access parameters via `ioContext.Parameters`
- Read piped input via `ioContext.ReadInputPipeChunks()`

#### Help(string[] parameters, IEnvironmentContext env)
Returns detailed help text for the command.

**Parameters:**
- `parameters` (string[]): Current command parameters
- `env` (IEnvironmentContext): Environment context

**Returns:** (string) Multi-line help text

---

## IIoContext

Manages command input/output and parameter access.

```csharp
public interface IIoContext : ICommandContext<IIoContext>, IAsyncDisposable
{
    bool HasPipedInput { get; }
    string[] Parameters { get; }
    
    void SetInputPipe(ChannelReader<string> reader);
    IAsyncEnumerable<string> ReadInputPipeChunks();
    Task<string> PromptForCommand(string prompt);
    void SetOutputPipe(ChannelWriter<string> writer);
    Task OutputChunk(string message);
    Task OutputStatus(string message);
    Task AddTraceMessage(string message);
    Task<string> PromptInput(string prompt);
    void ReportProgress(int current, int total);
}
```

### Properties

#### HasPipedInput
Indicates if this command receives input from a previous pipeline stage.

**Type:** bool

#### Parameters
Command-line arguments for this command.

**Type:** string[]

### Methods

#### SetInputPipe(ChannelReader<string> reader)
Called by framework to set input pipe for reading from previous stage.

#### ReadInputPipeChunks()
Reads all output chunks from previous pipeline stage.

**Returns:** `IAsyncEnumerable<string>` - Yields strings from previous command

**Example:**
```csharp
await foreach (var chunk in ioContext.ReadInputPipeChunks())
{
    var transformed = chunk.ToUpper();
    yield return CommandResult<string>.Success(transformed);
}
```

#### PromptForCommand(string prompt)
Prompts user for input (console, web, etc.).

**Parameters:**
- `prompt` (string): Prompt text to display

**Returns:** `Task<string>` - User input

#### SetOutputPipe(ChannelWriter<string> writer)
Called by framework to set output pipe for next pipeline stage.

#### OutputChunk(string message)
Outputs a chunk of command result data.

**Parameters:**
- `message` (string): Output text

**Remarks:**
- Prioritizes pipeline output if channel is set
- Otherwise outputs to default output (console, UI buffer, etc.)

#### OutputStatus(string message)
Outputs status information.

**Parameters:**
- `message` (string): Status text

#### AddTraceMessage(string message)
Adds a trace message for debugging.

**Parameters:**
- `message` (string): Trace information

#### PromptInput(string prompt)
Prompts for user input.

**Parameters:**
- `prompt` (string): Prompt text

**Returns:** `Task<string>` - User input

#### ReportProgress(int current, int total)
Reports progress for long-running operations.

**Parameters:**
- `current` (int): Current progress
- `total` (int): Total work

---

## IEnvironmentContext

Manages environment variables with isolated scopes.

```csharp
public interface IEnvironmentContext
{
    string GetVariable(string name);
    void SetVariable(string name, string value);
    bool VariableExists(string name);
    void RemoveVariable(string name);
    IReadOnlyDictionary<string, string> GetAllVariables();
    IEnvironmentContext CreateChildContext();
    Task<IEnvironmentContext> GetChildContextAsync(IEnvironmentContext parent);
}
```

### Methods

#### GetVariable(string name)
Gets an environment variable value.

**Parameters:**
- `name` (string): Variable name

**Returns:** (string) Variable value, or null if not found

#### SetVariable(string name, string value)
Sets an environment variable.

**Parameters:**
- `name` (string): Variable name
- `value` (string): Variable value

**Remarks:** Only persists to parent context if command has `ModifiesEnvironment=true`

#### VariableExists(string name)
Checks if a variable exists.

**Parameters:**
- `name` (string): Variable name

**Returns:** (bool) True if variable exists

#### RemoveVariable(string name)
Removes an environment variable.

**Parameters:**
- `name` (string): Variable name

#### GetAllVariables()
Gets all environment variables.

**Returns:** `IReadOnlyDictionary<string, string>` - All variables

#### CreateChildContext()
Creates an isolated child context.

**Returns:** `IEnvironmentContext` - New isolated context

---

## ICommandDescription

Metadata for a command.

```csharp
public interface ICommandDescription
{
    string BaseCommand { get; }
    string FullTypeName { get; }
    PackageDescription PackageDescription { get; }
    IReadOnlyDictionary<string, ICommandDescription> SubCommands { get; }
    bool ModifiesEnvironment { get; }
}
```

### Properties

#### BaseCommand
The command name users type.

**Type:** string

#### FullTypeName
The fully-qualified type name of the command class.

**Type:** string

#### PackageDescription
Metadata about the plugin package containing this command.

**Type:** PackageDescription

#### SubCommands
Sub-commands if this is a command root.

**Type:** `IReadOnlyDictionary<string, ICommandDescription>`

#### ModifiesEnvironment
Whether this command can modify the environment context.

**Type:** bool

---

## IResult&lt;T&gt;

Result wrapper for command output.

```csharp
public interface IResult<T>
{
    bool IsSuccess { get; }
    T Output { get; }
    string ErrorMessage { get; }
}
```

### Properties

#### IsSuccess
Whether the result represents success.

**Type:** bool

#### Output
The result value (command output).

**Type:** T

#### ErrorMessage
Error message if failed.

**Type:** string

---

## IPipelineExecutor

Manages pipeline execution.

```csharp
public interface IPipelineExecutor
{
    PipelineConfiguration Configuration { get; set; }
    
    Task ExecuteAsync(
        string commandLine,
        IIoContext ioContext,
        IEnvironmentContext environmentContext,
        Func<string, IIoContext, IEnvironmentContext, Task> executeCommand);
    
    Task ExecuteAsync(
        string commandLine,
        IIoContext ioContext,
        IEnvironmentContext environmentContext,
        Func<string, IIoContext, IEnvironmentContext, CancellationToken, Task> executeCommand,
        CancellationToken cancellationToken);
}
```

---

## IAuditLogger

Logs command execution and environment changes.

```csharp
public interface IAuditLogger
{
    Task LogAsync(string entry);
}
```

### Methods

#### LogAsync(string entry)
Logs an audit entry.

**Parameters:**
- `entry` (string): Log entry text

**Example:**
```csharp
public class ConsoleAuditLogger : IAuditLogger
{
    public async Task LogAsync(string entry)
    {
        Console.WriteLine($"[AUDIT] {entry}");
        await Task.CompletedTask;
    }
}
```

---

## IOutputEncoder

Encodes command output for different formats.

```csharp
public interface IOutputEncoder
{
    string Encode(string output);
}
```

### Methods

#### Encode(string output)
Encodes output for a specific format.

**Parameters:**
- `output` (string): Raw output text

**Returns:** (string) Encoded output

**Implementations:**
- `JsonEncoder` - JSON format
- `HtmlEncoder` - HTML format

---

## See Also

- [Attributes Reference](api-attributes.md)
- [Core Types Reference](api-core.md)
- [Exception Types](api-exceptions.md)
