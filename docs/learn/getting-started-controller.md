# Configure the CommandController

Learn how to set up and configure the `CommandController` to execute commands.

## What is CommandController?

`CommandController` is the central orchestrator that:
- Manages command registration and discovery
- Routes command execution
- Supports plugin loading
- Handles pipelines and I/O
- Manages audit logging

## Basic Setup

### Create and Configure

```csharp
using Xcaciv.Command;
using Xcaciv.Command.Interface;

// Create controller
var controller = new CommandController();

// Enable built-in commands (Say, Set, Env, Regif, Help)
controller.EnableDefaultCommands();

// Create I/O and environment contexts
var ioContext = new MemoryIoContext();
var environmentContext = new EnvironmentContext();

// Run a command
await controller.Run("SAY Hello World", ioContext, environmentContext);

// Get output
var output = ioContext.GetOutput();
Console.WriteLine(output);
```

## Output: 
```
Hello World
```

## Constructor Overloads

### Default Constructor

```csharp
var controller = new CommandController();
```

Creates a controller with default components.

### With Custom Crawler

```csharp
var crawler = new Crawler();
var controller = new CommandController(crawler);
```

Use when you need to customize plugin discovery.

### With Restricted Directory

```csharp
var controller = new CommandController(crawler, "/opt/plugins");
```

Restricts plugin loading to a specific directory for security.

### With Custom Verified Directories

```csharp
var directories = new VerifiedSourceDirectories(fileSystem);
var controller = new CommandController(directories);
```

Use when implementing custom directory verification logic.

### Full Dependency Injection

```csharp
var registry = new CommandRegistry();
var loader = new CommandLoader(crawler, directories);
var executor = new PipelineExecutor();
var commandExecutor = new CommandExecutor();
var factory = new CommandFactory();

var controller = new CommandController(
    registry,
    loader,
    executor,
    commandExecutor,
    factory,
    serviceProvider: null);
```

All dependencies are injectable for testing and customization.

## Adding Commands

### Built-in Commands

```csharp
controller.EnableDefaultCommands();
```

Registers these commands:
- `SAY` - Output text
- `SET` - Set environment variable
- `ENV` - Display environment variables
- `REGIF` - Regular expression filtering
- `HELP` - Display help

### From a Type

```csharp
[CommandRegister("MYCMD", "My custom command")]
public class MyCommand : AbstractCommand
{
    public override string HandleExecution(string[] parameters, IEnvironmentContext env)
    {
        return "Output";
    }
}

// Add to controller
controller.AddCommand("MyPackage", typeof(MyCommand), modifiesEnvironment: false);
```

### From an Instance

```csharp
var command = new MyCommand();
controller.AddCommand("MyPackage", command, modifiesEnvironment: false);
```

### From Package Directory

```csharp
// Add directory to search
controller.AddPackageDirectory("/opt/plugins");

// Load all commands found (searches bin/ subdirectory by default)
controller.LoadCommands();

// Or load from a specific subdirectory
controller.LoadCommands("lib");
```

## Running Commands

### Basic Execution

```csharp
var ioContext = new MemoryIoContext();
var env = new EnvironmentContext();

await controller.Run("MYCOMMAND arg1 arg2", ioContext, env);
```

### With Named Parameters

```csharp
await controller.Run("MYCOMMAND --verbose --output file.txt", ioContext, env);
```

### With Pipelining

```csharp
// Chain multiple commands with |
await controller.Run("SAY line1 | REGIF ^l", ioContext, env);
```

### Handling Errors

```csharp
try
{
    await controller.Run("UNKNOWN_COMMAND", ioContext, env);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Command failed: {ex.Message}");
}
```

## Help System

### Get All Commands

```csharp
controller.GetHelp("", ioContext, env);
var commands = ioContext.GetOutput();
```

### Get Specific Command Help

```csharp
controller.GetHelp("MYCOMMAND", ioContext, env);
var help = ioContext.GetOutput();
Console.WriteLine(help);
```

### User-Facing Help

Users can request help at command time:

```csharp
// These all show help for MYCOMMAND
await controller.Run("MYCOMMAND --HELP", ioContext, env);
await controller.Run("MYCOMMAND -?", ioContext, env);
await controller.Run("MYCOMMAND /?", ioContext, env);
```

## Audit Logging

Configure audit logging to track command execution:

```csharp
// Implement IAuditLogger
public class ConsoleAuditLogger : IAuditLogger
{
    public async Task LogAsync(string entry)
    {
        Console.WriteLine($"[AUDIT] {entry}");
        await Task.CompletedTask;
    }
}

// Set on controller
controller.SetAuditLogger(new ConsoleAuditLogger());
```

Logs include:
- Command execution start/completion
- Parameters processed
- Environment changes (for ModifiesEnvironment commands)
- Execution time
- Error conditions

## Output Encoding

Configure output encoding for different formats:

```csharp
// JSON encoding
controller.SetOutputEncoder(new JsonEncoder());

// HTML encoding
controller.SetOutputEncoder(new HtmlEncoder());
```

## Pipeline Configuration

Configure pipeline behavior:

```csharp
var executor = controller.GetPipelineExecutor();

executor.Configuration.MaxChannelQueueSize = 100;
executor.Configuration.BackpressureMode = PipelineBackpressureMode.Block;
executor.Configuration.StageTimeoutSeconds = 30;
```

**Configuration Options:**

| Property | Description | Default |
|----------|-------------|---------|
| `MaxChannelQueueSize` | Maximum items queued between pipeline stages | 50 |
| `BackpressureMode` | How to handle queue overflow (Block, DropOldest, DropNewest) | Block |
| `StageTimeoutSeconds` | Timeout for individual pipeline stages (0 = no timeout) | 0 |

## Advanced Topics

### Custom I/O Context

Implement `IIoContext` for custom input/output handling:

```csharp
public class WebIoContext : IIoContext
{
    // Implement interface methods for web-based I/O
}

await controller.Run("MYCOMMAND", new WebIoContext(), env);
```

### Custom Environment Context

Extend `EnvironmentContext` for specialized environment variable handling:

```csharp
var env = new EnvironmentContext();
env.SetVariable("MYVAR", "myvalue");
string value = env.GetVariable("MYVAR");
```

### Multiple Package Directories

Load commands from multiple locations:

```csharp
controller.AddPackageDirectory("/opt/plugins/system");
controller.LoadCommands();

controller.AddPackageDirectory("/opt/plugins/user");
controller.LoadCommands();
```

Commands from all directories are available in the same session.

## Next Steps

- [Build a Plugin Package](getting-started-plugins.md)
- [Use Pipelines](getting-started-pipelines.md)
- [CommandController API](api-command-controller.md)
- [ICommandController Interface](api-interfaces.md)
