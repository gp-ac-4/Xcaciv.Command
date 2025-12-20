# Xcaciv.Command Documentation

Comprehensive Microsoft Learn-style documentation for the Xcaciv.Command framework.

## Documentation Structure

This documentation is organized into the following sections:

### Getting Started
Quick guides to get you up and running with Xcaciv.Command:

- **[Quick Start Guide](quickstart.md)** - 5-minute introduction with working examples
- **[Create Your First Command](getting-started-create-command.md)** - Step-by-step guide to building commands
- **[Configure the CommandController](getting-started-controller.md)** - Set up and manage the command execution engine
- **[Build a Plugin Package](getting-started-plugins.md)** - Create distributable command plugins
- **[Using Pipelines](getting-started-pipelines.md)** - Chain multiple commands together

### API Reference
Complete API documentation for all public types:

- **[Interfaces](api-interfaces.md)** - Core interfaces (ICommandController, ICommandDelegate, IIoContext, etc.)
- **[Attributes](api-attributes.md)** - Parameter and command definition attributes
- **[Core Types](api-core.md)** - Core classes (CommandController, AbstractCommand, EnvironmentContext, etc.)
- **[Exceptions](api-exceptions.md)** - Exception types and error handling

### Architecture & Design
In-depth information about framework design:

- **[Architecture Overview](architecture.md)** - Component diagrams, design patterns, and technical decisions

### Start Here

1. **New to Xcaciv.Command?** ? Start with [Quick Start Guide](quickstart.md)
2. **Building your first command?** ? Read [Create Your First Command](getting-started-create-command.md)
3. **Setting up the framework?** ? Follow [Configure the CommandController](getting-started-controller.md)
4. **Creating plugins?** ? See [Build a Plugin Package](getting-started-plugins.md)
5. **Need complete API reference?** ? Check [API Reference](toc.md#api-reference)

## Key Concepts

### Commands
Self-contained executable units that implement `ICommandDelegate`. Commands are decorated with `CommandRegisterAttribute` and can accept parameters, process input, and produce output.

### Parameters
Commands define parameters using attributes:
- **Ordered** - Positional arguments
- **Named** - Key-value arguments
- **Flags** - Boolean switches
- **Suffix** - Trailing arguments

### Pipelines
Chain multiple commands together using the `|` character. Output from one command becomes input to the next.

### Plugins
Commands are packaged in .NET class libraries (.dll) and discovered at runtime from plugin directories.

### Environment Context
Isolated scope for environment variables. Commands can optionally modify the parent environment if marked with `ModifiesEnvironment=true`.

### I/O Context
Abstraction for input/output that supports pipelines, parameters, and various I/O implementations (console, web, file, etc.).

## Common Tasks

### Create a Simple Command

```csharp
[CommandRegister("GREET", "Greet someone")]
public class GreetCommand : AbstractCommand
{
    [CommandParameterOrdered("name", "Person's name")]
    public string Name { get; set; }

    public override string HandleExecution(string[] parameters, IEnvironmentContext env)
    {
        return $"Hello, {Name}!";
    }
}
```

See [Create Your First Command](getting-started-create-command.md) for details.

### Run a Command

```csharp
var controller = new CommandController();
controller.EnableDefaultCommands();
var io = new MemoryIoContext();
var env = new EnvironmentContext();
await controller.Run("SAY Hello World", io, env);
```

See [Configure the CommandController](getting-started-controller.md) for details.

### Create a Pipeline-Aware Command

```csharp
[CommandRegister("UPPERCASE", "Convert to uppercase")]
public class UppercaseCommand : AbstractCommand
{
    public override async IAsyncEnumerable<IResult<string>> Main(
        IIoContext ioContext, 
        IEnvironmentContext env)
    {
        await foreach (var chunk in ioContext.ReadInputPipeChunks())
        {
            yield return CommandResult<string>.Success(chunk.ToUpper());
        }
    }
}
```

See [Using Pipelines](getting-started-pipelines.md) for details.

### Load Plugins

```csharp
controller.AddPackageDirectory("/plugins");
controller.LoadCommands(); // searches bin/ subdirectory by default
```

See [Build a Plugin Package](getting-started-plugins.md) for details.

## Built-in Commands

The framework includes these commands by default:

| Command | Usage | Purpose |
|---------|-------|---------|
| SAY | SAY text | Output text |
| SET | SET NAME value | Set environment variable |
| ENV | ENV | Display environment variables |
| REGIF | PATTERN | Filter by regex |
| HELP | HELP [command] | Display help |

## Documentation Standards

This documentation follows Microsoft Learn standards:

- **Code Examples**: Practical, runnable examples
- **Links**: Cross-references to related content
- **Remarks**: Important notes and warnings
- **Tables**: Reference material in table format
- **Headings**: Clear hierarchy (H1 for main topic, H2 for sections, etc.)
- **Consistent Style**: British English, consistent terminology

## Finding Information

### By Task
- "How do I..." ? Check [Quick Start Guide](quickstart.md)
- "I want to..." ? Check relevant Getting Started guide
- "How does..." ? Check [Architecture](architecture.md)

### By Type
- Looking for a class ? Check [Core Types](api-core.md)
- Looking for an interface ? Check [Interfaces](api-interfaces.md)
- Looking for an attribute ? Check [Attributes](api-attributes.md)
- Looking for an exception ? Check [Exceptions](api-exceptions.md)

### By Scenario
- Building a console app ? [Quick Start](quickstart.md) + [Create Command](getting-started-create-command.md)
- Creating plugins ? [Plugin Guide](getting-started-plugins.md)
- Using pipelines ? [Pipeline Guide](getting-started-pipelines.md)
- Understanding design ? [Architecture](architecture.md)

## API Quick Reference

### Commonly Used Types

| Type | Purpose |
|------|---------|
| `CommandController` | Central orchestrator |
| `AbstractCommand` | Base class for commands |
| `MemoryIoContext` | Testing I/O implementation |
| `EnvironmentContext` | Environment variable management |
| `CommandResult<T>` | Result factory |
| `ICommandDelegate` | Command contract |
| `IIoContext` | I/O interface |
| `IEnvironmentContext` | Environment interface |

### Commonly Used Attributes

| Attribute | Purpose |
|-----------|---------|
| `CommandRegisterAttribute` | Register a command |
| `CommandParameterOrderedAttribute` | Positional parameter |
| `CommandParameterNamedAttribute` | Named parameter |
| `CommandFlagAttribute` | Boolean flag |
| `CommandParameterSuffixAttribute` | Trailing arguments |

## Examples

### Example 1: Simple Text Transformer

```csharp
[CommandRegister("UPPERCASE", "Convert text to uppercase")]
public class UppercaseCommand : AbstractCommand
{
    [CommandParameterOrdered("text", "Text to convert")]
    public string Text { get; set; }

    public override string HandleExecution(string[] parameters, IEnvironmentContext env)
    {
        return Text.ToUpper();
    }
}
```

### Example 2: Pipeline-Aware Processor

```csharp
[CommandRegister("LINECOUNT", "Count lines")]
public class LineCountCommand : AbstractCommand
{
    public override async IAsyncEnumerable<IResult<string>> Main(
        IIoContext ioContext, 
        IEnvironmentContext env)
    {
        int count = 0;
        await foreach (var line in ioContext.ReadInputPipeChunks())
        {
            count++;
        }
        yield return CommandResult<string>.Success($"Lines: {count}");
    }
}
```

### Example 3: Using Environment Variables

```csharp
[CommandRegister("GREET", "Personalized greeting")]
public class GreetCommand : AbstractCommand
{
    public override string HandleExecution(string[] parameters, IEnvironmentContext env)
    {
        var name = env.GetVariable("USER") ?? "Friend";
        return $"Hello, {name}!";
    }
}
```

## Support

For issues, questions, or contributions:

- **GitHub**: [xcaciv/Xcaciv.Command](https://github.com/xcaciv/Xcaciv.Command)
- **NuGet**: [Xcaciv.Command packages](https://www.nuget.org/packages?q=xcaciv)

## License

See the main repository for license information.

---

**Last Updated:** 2024
**Documentation Version:** 1.0
**Framework Version:** 1.0+
