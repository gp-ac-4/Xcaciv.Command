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

## Latest Changes

- Pipeline execution is fully async and stream-friendly, so commands can safely yield intermediate chunks to downstream consumers.
- Command loading now supports verified package directories via `AddPackageDirectory` + `LoadCommands`, keeping each plugin confined to its own path-restricted security policy.
- Built-in commands capture parameter metadata through attributes, making help generation and piping behavior consistent across implementations.

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

## Command Examples

### Simple Command (No Parameters)

```csharp
[CommandRegister("Now", "Display current timestamp", Prototype = "NOW")]
internal class NowCommand : AbstractCommand
{
    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return CommandResult<string>.Success(DateTime.UtcNow.ToString("O"), this.OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return CommandResult<string>.Success(pipedChunk, this.OutputFormat); // Pass through piped input
    }
}
```

### Named Parameters with Flag

```csharp
[CommandRegister("Copy", "Copy with optional verbose flag", Prototype = "COPY <source> -dest <destination> [-limit <number>] [-sort <ASC|DSC>] [-v]")]
[CommandParameterOrdered("Source", "Source path")]
[CommandParameterNamed("Dest", "Destination path", IsRequired = true)]
[CommandParameterNamed("Limit", "Limit number of files", DataType = typeof(int)]
[CommandParameterNamed("Sort", "Sort order", IsRequired = false, AllowedValues: ["ASC", "DSC"])]
[CommandFlag("Verbose", "Show verbose output")]
internal class CopyCommand : AbstractCommand
{
    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var source = parameters.TryGetValue("source", out var src) && src.IsValid
            ? src.GetValue<string>()
            : string.Empty;

        var dest = parameters.TryGetValue("dest", out var d) && d.IsValid
            ? d.GetValue<string>()
            : string.Empty;

        var verbose = parameters.TryGetValue("verbose", out var v) && v.IsValid
            ? v.GetValue<bool>()
            : false;

        var result = $"Copied {source} to {dest}" + (verbose ? " [verbose mode]" : "");
        return CommandResult<string>.Success(result, this.OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return CommandResult<string>.Success(string.Empty, this.OutputFormat);
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