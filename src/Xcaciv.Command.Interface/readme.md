# Xcaciv.Command.Interface

Contracts, attributes, and result types for building commands in the Xcaciv command framework. Use this package to describe commands, parameters, and pipeline behavior without pulling in concrete implementations.

## Target Frameworks

- Uses the solution-wide `$(XcacivTargetFrameworks)` (defaults to .NET 10).

## Install

```powershell
Install-Package Xcaciv.Command.Interface -Version 3.2.2
```

## Core Surface

- Interfaces: `ICommandDelegate`, `ICommandController`, `IEnvironmentContext`, `IIoContext`, `IResult<T>`
- Attributes: `CommandRegisterAttribute`, `CommandRootAttribute`, `CommandParameterOrderedAttribute`, `CommandParameterNamedAttribute`, `CommandFlagAttribute`, `CommandParameterSuffixAttribute`, `CommandHelpRemarksAttribute`
- Results & formats: `CommandResult`, `ResultFormat`
- Configuration: `PipelineConfiguration`, `PipelineBackpressureMode`
- Exceptions: `XcCommandException`, `NoPluginsFoundException`, `NoPackageDirectoryFoundException`, `InValidConfigurationException`

## Attribute Usage Examples

### Register with Ordered + Named + Flag Parameters

```csharp
using System.Collections.Generic;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;

[CommandRegister("Copy", "Copy with optional verbose flag", Prototype = "COPY <source> -dest <destination> [-v]")]
[CommandParameterOrdered("Source", "Source path")]
[CommandParameterNamed("Dest", "Destination path", IsRequired = true)]
[CommandFlag("Verbose", "Show verbose output")]
internal class CopyCommand : AbstractCommand
{
    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var source = parameters.TryGetValue("source", out var src) && src.IsValid ? src.GetValue<string>() : string.Empty;
        var dest = parameters.TryGetValue("dest", out var dst) && dst.IsValid ? dst.GetValue<string>() : string.Empty;
        var verbose = parameters.TryGetValue("verbose", out var v) && v.IsValid && v.GetValue<bool>();

        if (string.IsNullOrWhiteSpace(dest))
        {
            throw new ArgumentException("Destination path is required for COPY commands.", nameof(dest));
        }

        var result = $"Copied {source} to {dest}" + (verbose ? " [verbose mode]" : string.Empty);
        return CommandResult<string>.Success(result, OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return CommandResult<string>.Success(string.Empty, OutputFormat);
    }
}
```

### Suffix Parameter (Capture Remaining Arguments)

```csharp
[CommandRegister("Echo", "Echo text", Prototype = "ECHO <text...>")]
[CommandParameterSuffix("text", "Text to echo")]
internal class EchoCommand : AbstractCommand
{
    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var text = parameters.TryGetValue("text", out var t) && t.IsValid ? t.GetValue<string>() : string.Empty;
        return CommandResult<string>.Success(text, OutputFormat);
    }
}
```

## Defining Parameters

- Ordered parameters must precede named parameters.
- Use `DataType` on parameter attributes to narrow accepted types (e.g., `typeof(int)`, `typeof(Guid)`).
- Flags represent booleans; presence sets `true`.
- Suffix parameters capture the remaining raw text as one value.
- Access values via `IParameterValue.GetValue<T>()`; check `IsValid` before use.

## Exception Guidance

- Validate early and throw specific exceptions (e.g., `ArgumentException`, `InvalidOperationException`) with context.
- Avoid catching general exceptions in command implementations; allow the controller layer to manage unexpected faults.
- When wrapping an exception, add context and rethrow to preserve the stack trace:

```csharp
catch (IOException ioException)
{
    throw new InvalidOperationException("COPY failed while accessing the file system.", ioException);
}
```

## More Resources

- `COMMAND_TEMPLATE.md` — full patterns for command implementation, piping hooks, and disposal guidance.
- `src/Xcaciv.Command.Interface/Attributes/` — authoritative attribute definitions.
- `Xcaciv.Command.Core` - `AbstractCommand` and `CommandResult` used alongside these interfaces.
