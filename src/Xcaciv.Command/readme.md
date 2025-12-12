# Xcaciv.Command

Excessively modular, async pipeable, text command framework.

## Quick Start

```csharp
using Xcaciv.Command;
using Xcaciv.Command.FileLoader;

var controller = new CommandController();
controller.EnableDefaultCommands();

var env = new EnvironmentContext();
var ioContext = new MemoryIoContext();

await controller.Run("Say Hello to my little friend", ioContext, env);

// outputs: Hello to my little friend
```

## Loading External Commands

```csharp
var controller = new CommandController(new Crawler(), restrictedDirectory);
controller.AddPackageDirectory("path/to/plugins");
controller.LoadCommands();

await controller.Run("your-command args", ioContext, env);
```

## Creating Commands

Commands are .NET class libraries that contain implementations of the `ICommandDelegate` interface and are decorated with command attributes:

```csharp
[CommandRegister("MYCOMMAND", "Description of my command")]
public class MyCommand : AbstractCommand
{
    [CommandParameterOrdered(0, "input", "Input parameter description")]
    public override async IAsyncEnumerable<string> Main(IIoContext io, IEnvironmentContext env)
    {
        var input = GetParameterValue("input", io.Parameters);
        yield return $"Processed: {input}";
    }
}
```

## Security

This framework uses **Xcaciv.Loader 2.0.1** with instance-based security policies:

- Each plugin is loaded with directory-based path restrictions
- Default security policy prevents access outside plugin directories
- Security violations are logged and handled gracefully
- No static configuration - each AssemblyContext has independent security

## Built-in Commands

- **SAY**: Output text to the context
- **SET**: Set environment variables
- **ENV**: Display environment variables
- **REGIF**: Conditional execution based on regex

## Pipeline Support

Chain commands with `|` for pipeline execution:

```csharp
await controller.Run("command1 arg1 | command2 | command3", ioContext, env);
```

## Dependencies

- Xcaciv.Loader 2.0.1 - Secure assembly loading
- Xcaciv.Command.Interface - Core interfaces
- Xcaciv.Command.Core - Base implementations
- Xcaciv.Command.FileLoader - Plugin discovery

## Project Links

- GitHub: https://github.com/Xcaciv/Xcaciv.Command
- License: BSD-3-Clause