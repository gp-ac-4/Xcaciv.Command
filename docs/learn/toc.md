# Table of Contents

## Getting Started

- [Xcaciv.Command Framework](index.md) - Introduction and key concepts
- [Create Your First Command](getting-started-create-command.md) - Build a simple command
- [Configure the CommandController](getting-started-controller.md) - Set up and run commands
- [Build a Plugin Package](getting-started-plugins.md) - Create distributable command plugins
- [Using Pipelines](getting-started-pipelines.md) - Chain multiple commands together

## API Reference

- [Interfaces](api-interfaces.md)
  - ICommandController - Command execution orchestration
  - ICommandDelegate - Command contract
  - IIoContext - Input/output and context
  - IEnvironmentContext - Environment variable management
  - ICommandDescription - Command metadata
  - IResult&lt;T&gt; - Command results
  - IPipelineExecutor - Pipeline coordination
  - IAuditLogger - Execution logging
  - IOutputEncoder - Output formatting

- [Attributes](api-attributes.md)
  - CommandRegisterAttribute - Register a command
  - CommandRootAttribute - Create sub-commands
  - CommandHelpRemarksAttribute - Add help text
  - CommandParameterOrderedAttribute - Positional parameters
  - CommandParameterNamedAttribute - Named parameters
  - CommandFlagAttribute - Boolean flags
  - CommandParameterSuffixAttribute - Trailing arguments

- [Core Types](api-core.md)
  - CommandResult&lt;T&gt; - Result factory
  - AbstractCommand - Command base class
  - CommandController - Central orchestrator
  - EnvironmentContext - Environment management
  - MemoryIoContext - Testing I/O
  - PipelineConfiguration - Pipeline behavior
  - PipelineBackpressureMode - Queue overflow handling
  - CommandDescription - Command metadata
  - PackageDescription - Plugin package info
  - Built-in Commands - Say, Set, Env, Regif, Help

- [Exceptions](api-exceptions.md)
  - XcCommandException - Base exception
  - NoPackageDirectoryFoundException - Missing directory
  - NoPluginFilesFoundException - No DLLs found
  - NoPluginsFoundException - No valid commands
  - InValidConfigurationException - Configuration error

## Architecture and Design

- [Architecture Overview](architecture.md)
  - High-level component diagram
  - Plugin architecture and discovery
  - Pipeline channel communication
  - Environment isolation
  - Parameter processing
  - Error handling strategy
  - Audit logging
  - Thread safety
  - Dependency injection
  - Security model

## Advanced Topics

- [Advanced Command Patterns](advanced-command-patterns.md) (coming soon)
  - Async operations
  - Resource management
  - Streaming data
  - Error recovery
  - State management

- [Plugin Security](advanced-plugin-security.md) (coming soon)
  - Assembly security policies
  - Directory verification
  - Dependency isolation
  - Audit trail enforcement
  - Threat models

- [Performance Optimization](advanced-performance.md) (coming soon)
  - Pipeline tuning
  - Memory efficiency
  - Throughput maximization
  - Latency reduction

- [Custom I/O Implementations](advanced-custom-io.md) (coming soon)
  - Web-based I/O
  - File-based I/O
  - Network I/O
  - GUI integration

---

## Quick Reference

### Common Tasks

**Create a simple command:**
```csharp
[CommandRegister("MYCOMMAND", "My first command")]
public class MyCommand : AbstractCommand
{
    public override string HandleExecution(string[] parameters, IEnvironmentContext env)
    {
        return "Hello World";
    }
}
```

**Run a command:**
```csharp
var controller = new CommandController();
controller.EnableDefaultCommands();
var io = new MemoryIoContext();
var env = new EnvironmentContext();
await controller.Run("SAY Hello", io, env);
```

**Create a pipeline-aware command:**
```csharp
[CommandRegister("UPPERCASE", "Convert to uppercase")]
public class UppercaseCommand : AbstractCommand
{
    public override async IAsyncEnumerable<IResult<string>> Main(
        IIoContext ioContext, IEnvironmentContext env)
    {
        await foreach (var chunk in ioContext.ReadInputPipeChunks())
        {
            yield return CommandResult<string>.Success(chunk.ToUpper());
        }
    }
}
```

**Load plugins:**
```csharp
controller.AddPackageDirectory("/plugins");
controller.LoadCommands(); // searches bin/ subdirectory
```

**Configure pipeline behavior:**
```csharp
var executor = new PipelineExecutor();
executor.Configuration.MaxChannelQueueSize = 100;
executor.Configuration.BackpressureMode = PipelineBackpressureMode.Block;
executor.Configuration.StageTimeoutSeconds = 30;
```

### Key Concepts

| Concept | Description |
|---------|-------------|
| **Command** | Executable unit implementing ICommandDelegate |
| **Pipeline** | Chain of commands connected via `\|` separator |
| **Parameter** | Input to a command defined via attributes |
| **Plugin** | Class library containing commands |
| **Environment** | Isolated context for variables, optional persistence |
| **IoContext** | Interface for input/output and parameter access |
| **Backpressure** | Queue overflow handling strategy |
| **Audit** | Logging of execution and environment changes |

### File Structure for Plugins

```
MyPlugin/
??? MyCommandPlugin.csproj
??? src/
?   ??? Commands/
?   ?   ??? Command1.cs
?   ?   ??? Command2.cs
?   ??? GlobalUsings.cs
??? bin/
?   ??? Release/
?       ??? net8.0/
?           ??? MyPlugin.dll          ? Users copy this directory
?           ??? MyPlugin.deps.json
?           ??? (dependencies)
```

Users place the `net8.0/` directory in their plugins folder, typically:
```
/plugins/MyPlugin/bin/
```

### Help Text Triggers

Users can request help with:
```
MYCOMMAND --HELP
MYCOMMAND -?
MYCOMMAND /?
HELP MYCOMMAND
HELP
```

---

## Links and Resources

- [GitHub Repository](https://github.com/xcaciv/Xcaciv.Command)
- [NuGet Packages](https://www.nuget.org/packages?q=xcaciv)
- [Example Projects](../examples/)
- [Troubleshooting Guide](troubleshooting.md)

---

## Version History

### v1.0
Initial release with:
- Command framework
- Plugin system
- Pipeline support
- Built-in commands
- Audit logging
- Environment isolation
