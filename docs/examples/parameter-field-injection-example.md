# Parameter Field Injection Example

## Overview

The `CommandFactory` now automatically injects named parameter values into public instance fields of commands. This provides a convenient way to access parameter values without manually extracting them from the parameters dictionary.

## How It Works

When a command is created, the `CommandFactory`:
1. Processes the parameters into typed `IParameterValue` objects
2. Gets all public instance fields from the command class
3. Matches parameter names to field names (case-insensitive)
4. Sets the field value if the parameter is valid and types are compatible

## Example Command

```csharp
using System.Collections.Generic;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;

[CommandRegister("Greet", "Greets a person with optional title")]
[CommandParameterNamed("name", "Name of the person", Required = true)]
[CommandParameterNamed("title", "Optional title (Mr., Mrs., Dr., etc.)", DefaultValue = "")]
public class GreetCommand : AbstractCommand
{
    // Public fields will be automatically set from named parameters
    public string? Name;
    public string? Title;

    public override IResult<string> HandleExecution(
        Dictionary<string, IParameterValue> parameters, 
        IEnvironmentContext env)
    {
        // Fields are already populated by CommandFactory
        var greeting = string.IsNullOrEmpty(Title) 
            ? $"Hello, {Name}!" 
            : $"Hello, {Title} {Name}!";
            
        return CommandResult<string>.Success(greeting);
    }

    public override IResult<string> HandlePipedChunk(
        string pipedChunk, 
        Dictionary<string, IParameterValue> parameters, 
        IEnvironmentContext env)
    {
        return CommandResult<string>.Success(pipedChunk);
    }
}
```

## Usage

```bash
# Basic usage
Greet --name "Smith"
# Output: Hello, Smith!

# With title
Greet --name "Smith" --title "Dr."
# Output: Hello, Dr. Smith!

# Case-insensitive parameter names
Greet --NAME "Smith" --TITLE "Mrs."
# Output: Hello, Mrs. Smith!
```

## Important Notes

1. **Case-Insensitive Matching**: Parameter names are matched to field names case-insensitively
2. **Type Safety**: Values are only set if the parameter type is compatible with the field type
3. **Optional Feature**: Fields are only set if a matching parameter exists and is valid
4. **Error Handling**: Field setting errors are silently ignored to avoid breaking command execution
5. **Still Available in Dictionary**: Parameters are still available in the `parameters` dictionary for manual access

## Benefits

- **Less Boilerplate**: No need to manually extract each parameter
- **Cleaner Code**: Direct field access instead of dictionary lookups
- **Type Safety**: Leverages the existing typed parameter system
- **Backward Compatible**: Existing commands continue to work without changes
