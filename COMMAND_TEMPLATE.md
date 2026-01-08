# Command Implementation Template

This document provides a template and guidelines for implementing new commands in the Xcaciv.Command framework.

## Quick Reference: Parameter Attributes

| Attribute | Usage | Notes |
|-----------|-------|-------|
| `CommandRegisterAttribute` | **Required** on class | Registers the command with a name and description |
| `CommandParameterOrderedAttribute` | Position-based parameters | Must precede named parameters |
| `CommandParameterNamedAttribute` | Named parameters (with `-name` flag) | Used with `-name value` syntax |
| `CommandFlagAttribute` | Boolean toggle flags | Presence = true, absence = false |
| `CommandParameterSuffixAttribute` | Capture remaining arguments | Collects all remaining args as single value |
| `CommandHelpRemarksAttribute` | Additional help information | Multiple remarks can be added |
| `CommandRootAttribute` | Multi-level sub-commands | For commands with sub-commands |

## Interface Alignment

- `AbstractCommand` implements `ICommandDelegate` (which includes `Main`, `Help`, and `OneLineHelp`) and `IAsyncDisposable`; most commands only override `HandleExecution` and `HandlePipedChunk`.
- Use `OutputFormat` to declare the serialization shape of your output (defaults to `ResultFormat.General`).
- Override `DisposeAsync` when your command owns disposable resources. Add `using System.Threading.Tasks;` and `using Xcaciv.Command.Interface;` when you override it.

```csharp
public MyCommand()
{
    OutputFormat = ResultFormat.General; // Or ResultFormat.JSON/CSV/TDL/YAML
}

public override ValueTask DisposeAsync()
{
    // Release disposable resources here.
    return base.DisposeAsync();
}
```

## Important: HandlePipedChunk Signature (v3.2.3+)

As of version 3.2.3, `HandlePipedChunk` accepts `IResult<string>` instead of `string`:

```csharp
// New signature (v3.2.3+)
public override IResult<string> HandlePipedChunk(
    IResult<string> pipedChunk, 
    Dictionary<string, IParameterValue> parameters, 
    IEnvironmentContext env)
{
    // Access the output string
    var input = pipedChunk.Output ?? string.Empty;
    
    // Optionally check if upstream command succeeded
    if (!pipedChunk.IsSuccess)
    {
        // Handle or propagate error
        return pipedChunk;
    }
    
    // Process the input
    var result = ProcessInput(input);
    return CommandResult<string>.Success(result, this.OutputFormat);
}
```

This allows commands to:
- Check `pipedChunk.IsSuccess` for upstream failures
- Access `pipedChunk.ErrorMessage` and `pipedChunk.Exception`
- Retrieve `pipedChunk.ResultFormat` and `pipedChunk.CorrelationId`

## Basic Command Template

```csharp
using System;
using System.Collections.Generic;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;

namespace Xcaciv.Command.Commands
{
    [CommandRegister("MyCommand", "Description of what the command does", Prototype = "MYCOMMAND <param1> <param2>")]
    [CommandParameterOrdered("param1", "Description of first parameter")]
    [CommandParameterOrdered("param2", "Description of second parameter", DataType = typeof(int))]
    [CommandParameterNamed("param3", "Description of named parameter", IsRequired = false, AllowedValues = ["value1", "value2"])]
    internal class MyCommand : AbstractCommand
    {
        public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            // Extract typed parameters safely
            var param1 = parameters.TryGetValue("param1", out var p1) && p1.IsValid 
                ? p1.GetValue<string>() 
                : string.Empty;
            
            var param2 = parameters.TryGetValue("param2", out var p2) && p2.IsValid 
                ? p2.GetValue<string>() 
                : string.Empty;

            var param3 = parameters.TryGetValue("param3", out var p3) && p3.IsValid 
                ? p3.GetValue<string>() 
                : "default";

            // Execute command logic
            var result = $"Processed: {param1}, {param2}, {param3}";
            
            return CommandResult<string>.Success(result, this.OutputFormat);
        }

        public override IResult<string> HandlePipedChunk(IResult<string> pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            // Handle piped input - extract the output string
            var input = pipedChunk.Output ?? string.Empty;
            return CommandResult<string>.Success(input.ToUpper(), this.OutputFormat);
        }
    }
}
```

## Pattern Examples

### 1. Simple Command (No Parameters)

```csharp
[CommandRegister("Now", "Display current timestamp", Prototype = "NOW")]
internal class NowCommand : AbstractCommand
{
    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return CommandResult<string>.Success(DateTime.UtcNow.ToString("O"), this.OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(IResult<string> pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        // Pass through piped input unchanged
        return CommandResult<string>.Success(pipedChunk.Output ?? string.Empty, this.OutputFormat);
    }
}
```

### 2. Command with Ordered Parameters

```csharp
[CommandRegister("Add", "Add two numbers", Prototype = "ADD <number1> <number2>")]
[CommandParameterOrdered("Number1", "First number")]
[CommandParameterOrdered("Number2", "Second number")]
internal class AddCommand : AbstractCommand
{
    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var num1 = parameters.TryGetValue("number1", out var p1) && p1.IsValid 
            ? p1.GetValue<int>() 
            : 0;
        var num2 = parameters.TryGetValue("number2", out var p2) && p2.IsValid 
            ? p2.GetValue<int>() 
            : 0;

        return CommandResult<string>.Success((num1 + num2).ToString(), this.OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(IResult<string> pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        // Not designed for piping - return empty
        return CommandResult<string>.Success(string.Empty, this.OutputFormat);
    }
}
```

### 3. Command with Named Parameters

```csharp
[CommandRegister("Copy", "Copy with optional verbose flag", Prototype = "COPY <source> -dest <destination> [-v]")]
[CommandParameterOrdered("Source", "Source path")]
[CommandParameterNamed("Dest", "Destination path", IsRequired = true)]
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

        // Execute copy logic
        var result = $"Copied {source} to {dest}" + (verbose ? " [verbose mode]" : "");
        return CommandResult<string>.Success(result, this.OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(IResult<string> pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return CommandResult<string>.Success(string.Empty, this.OutputFormat);
    }
}
```

### 4. Command with Suffix Parameter (Capture All Remaining Args)

```csharp
[CommandRegister("Echo", "Echo text with interpolation", Prototype = "ECHO <text...>")]
[CommandParameterSuffix("text", "Text to echo")]
internal class EchoCommand : AbstractCommand
{
    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var text = parameters.TryGetValue("text", out var t) && t.IsValid 
            ? t.GetValue<string>() 
            : string.Empty;
        
        return CommandResult<string>.Success(text, this.OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(IResult<string> pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        // Pass through piped input
        return CommandResult<string>.Success(pipedChunk.Output ?? string.Empty, this.OutputFormat);
    }
}
```

### 5. Command with Piped Input (SET pattern)

```csharp
[CommandRegister("Buffer", "Buffer piped input", Prototype = "BUFFER <name>")]
[CommandParameterOrdered("Name", "Variable name")]
[CommandParameterOrdered("Value", "Initial value", UsePipe = true)]
[CommandHelpRemarks("This command accumulates piped input into a variable.")]
internal class BufferCommand : AbstractCommand
{
    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var name = parameters.TryGetValue("name", out var n) && n.IsValid 
            ? n.GetValue<string>() 
            : string.Empty;
        var value = parameters.TryGetValue("value", out var v) && v.IsValid 
            ? v.GetValue<string>() 
            : string.Empty;

        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
        {
            env.SetValue(name, value);
        }
        return CommandResult<string>.Success(string.Empty, this.OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(IResult<string> pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var name = parameters.TryGetValue("name", out var n) && n.IsValid 
            ? n.GetValue<string>() 
            : string.Empty;
        
        if (!string.IsNullOrEmpty(name))
        {
            var input = pipedChunk.Output ?? string.Empty;
            var current = env.GetValue(name);
            env.SetValue(name, current + input);
        }
        return CommandResult<string>.Success(string.Empty, this.OutputFormat);
    }

    protected override void OnStartPipe(Dictionary<string, IParameterValue> processedParameters, IEnvironmentContext environment)
    {
        var name = processedParameters.TryGetValue("name", out var n) && n.IsValid 
            ? n.GetValue<string>() 
            : string.Empty;
        
        if (!string.IsNullOrEmpty(name))
        {
            environment.SetValue(name, string.Empty);
        }
        base.OnStartPipe(processedParameters, environment);
    }
}
```

### 6. Command with Stateful Processing (REGIF pattern)

```csharp
[CommandRegister("Filter", "Filter piped input by condition", Prototype = "FILTER <condition>")]
[CommandParameterOrdered("Condition", "Filter condition")]
internal class FilterCommand : AbstractCommand
{
    private bool cachedCondition = false;

    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        // When not piping, just initialize
        if (parameters.TryGetValue("condition", out var c) && c.IsValid)
        {
            cachedCondition = c.GetValue<bool>();
        }
        return CommandResult<string>.Success(string.Empty, this.OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(IResult<string> pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var input = pipedChunk.Output ?? string.Empty;
        
        if (cachedCondition)
        {
            return CommandResult<string>.Success(input, this.OutputFormat);
        }
        return CommandResult<string>.Success(string.Empty, this.OutputFormat);
    }
}
```

### 7. Command with Error Propagation

```csharp
[CommandRegister("Validate", "Validate and process input", Prototype = "VALIDATE <rule>")]
[CommandParameterOrdered("Rule", "Validation rule")]
[CommandHelpRemarks("Checks if piped input matches the validation rule.")]
[CommandHelpRemarks("Propagates errors from upstream commands.")]
internal class ValidateCommand : AbstractCommand
{
    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return CommandResult<string>.Success(string.Empty, this.OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(IResult<string> pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        // Check if upstream command failed
        if (!pipedChunk.IsSuccess)
        {
            // Propagate the error to downstream commands
            return pipedChunk;
        }

        var input = pipedChunk.Output ?? string.Empty;
        var rule = parameters.TryGetValue("rule", out var r) && r.IsValid 
            ? r.GetValue<string>() 
            : string.Empty;

        // Validate the input
        if (IsValid(input, rule))
        {
            return CommandResult<string>.Success(input, this.OutputFormat);
        }
        else
        {
            return CommandResult<string>.Failure($"Validation failed: input does not match rule '{rule}'");
        }
    }

    private bool IsValid(string input, string rule)
    {
        // Implement validation logic
        return true;
    }
}
```

### 8. Command with Help Remarks and Aliases

```csharp
[CommandRegister("Count", "Count lines or characters", Prototype = "COUNT [-type lines|chars]")]
[CommandParameterNamed("Type", "Count type", DefaultValue = "lines", AllowedValues = ["lines", "chars"])]
[CommandHelpRemarks("Counts the number of lines or characters in piped input.")]
[CommandHelpRemarks("Default behavior counts lines. Use -type chars to count characters.")]
internal class CountCommand : AbstractCommand
{
    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return CommandResult<string>.Success("0", this.OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(IResult<string> pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var input = pipedChunk.Output ?? string.Empty;
        var type = parameters.TryGetValue("type", out var t) && t.IsValid 
            ? t.GetValue<string>() 
            : "lines";

        var count = type == "chars" 
            ? input.Length 
            : input.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None).Length;

        return CommandResult<string>.Success(count.ToString(), this.OutputFormat);
    }
}
```

## Best Practices

1. **Null Safety**: Always use `pipedChunk.Output ?? string.Empty` when accessing piped input
2. **Error Handling**: Check `pipedChunk.IsSuccess` if you need to handle upstream failures
3. **Error Propagation**: Return `pipedChunk` directly to propagate errors to downstream commands
4. **Parameter Validation**: Use `IsValid` property before calling `GetValue<T>()`
5. **Resource Cleanup**: Override `DisposeAsync` for commands that allocate resources
6. **Output Format**: Set `OutputFormat` in constructor if command produces structured data
7. **Help Documentation**: Use `CommandHelpRemarks` for additional usage information

## Migration from v3.2.2 to v3.2.3

If you have existing commands using the old signature:

```csharp
// Old (v3.2.2 and earlier)
public override IResult<string> HandlePipedChunk(
    string pipedChunk, 
    Dictionary<string, IParameterValue> parameters, 
    IEnvironmentContext env)
{
    return CommandResult<string>.Success(pipedChunk.ToUpper());
}

// New (v3.2.3+)
public override IResult<string> HandlePipedChunk(
    IResult<string> pipedChunk, 
    Dictionary<string, IParameterValue> parameters, 
    IEnvironmentContext env)
{
    var input = pipedChunk.Output ?? string.Empty;
    return CommandResult<string>.Success(input.ToUpper());
}
