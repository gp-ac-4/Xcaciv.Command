# Create Your First Command

Learn how to create a command using the Xcaciv.Command framework.

## Prerequisites

- .NET 8 or later
- Xcaciv.Command.Core package reference
- Understanding of `ICommandDelegate` interface

## Basic Command Structure

All commands inherit from `AbstractCommand` and are decorated with `CommandRegisterAttribute`.

```csharp
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

[CommandRegister("GREET", "Greet the user with a personalized message")]
public class GreetCommand : AbstractCommand
{
    [CommandParameterOrdered("name", "The name of the person to greet")]
    public string Name { get; set; }

    public override string HandleExecution(string[] parameters, IEnvironmentContext env)
    {
        return $"Hello, {Name}!";
    }

    public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env)
    {
        return $"Hello, {pipedChunk}!";
    }
}
```

## Command Registration Attribute

The `CommandRegisterAttribute` decorates your command class and provides metadata:

```csharp
[CommandRegister("GREET", "Greet the user with a personalized message")]
```

**Parameters:**
- `command` (string): The command name users type
- `description` (string): Short description for help
- `prototype` (string, optional): Custom usage prototype (default: "todo" = auto-generate)

## Adding Parameters

### Ordered Parameters

Positional arguments that must appear in order:

```csharp
[CommandParameterOrdered("name", "User's name", usePipe: false)]
public string Name { get; set; }

[CommandParameterOrdered("greeting", "Greeting text")]
public string Greeting { get; set; }
```

Usage: `GREET John "Hello there"`

### Named Parameters

Key-value arguments that can appear anywhere:

```csharp
[CommandParameterNamed("greeting", "Custom greeting text")]
public string Greeting { get; set; }

[CommandParameterNamed("title", "Title to use")]
public string Title { get; set; }
```

Usage: `GREET John --greeting "Hello" --title "Mr"`

### Flags

Boolean switches:

```csharp
[CommandFlag("verbose", "Show detailed output")]
public bool Verbose { get; set; }

[CommandFlag("quiet", "Suppress output")]
public bool Quiet { get; set; }
```

Usage: `GREET John --verbose --quiet`

### Suffix Parameters

Trailing arguments captured as an array:

```csharp
[CommandParameterSuffix("messages", "Additional messages")]
public string[] Messages { get; set; }
```

Usage: `GREET John message1 message2 message3`

## Implementing Command Logic

Override `HandleExecution` for normal command operation:

```csharp
public override string HandleExecution(string[] parameters, IEnvironmentContext env)
{
    // Access command-line arguments
    var args = parameters;
    
    // Read environment variables
    var userName = env.GetVariable("USER");
    
    // Return output
    return $"Processing {args.Length} parameters";
}
```

### Returning Multiple Output Chunks

Use `Main()` to yield multiple outputs for pipeline support:

```csharp
public override async IAsyncEnumerable<IResult<string>> Main(
    IIoContext ioContext, 
    IEnvironmentContext env)
{
    var lines = new[] { "First line", "Second line", "Third line" };
    
    foreach (var line in lines)
    {
        yield return CommandResult<string>.Success(line);
    }
}
```

## Pipeline Support

Commands that support piped input override `HandlePipedChunk`:

```csharp
public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env)
{
    // Transform each line from the previous command
    return pipedChunk.ToUpper();
}

public override async IAsyncEnumerable<IResult<string>> Main(
    IIoContext ioContext, 
    IEnvironmentContext env)
{
    // Read from piped input
    await foreach (var chunk in ioContext.ReadInputPipeChunks())
    {
        yield return CommandResult<string>.Success(HandlePipedChunk(chunk, env.GetVariable("ARGS").Split(), env));
    }
}
```

Usage in pipeline: `SAY "Hello" | GREET`

## Help Generation

The framework automatically generates help from your attributes. Override for custom help:

```csharp
public override string Help(string[] parameters, IEnvironmentContext env)
{
    return BuildHelpString(parameters, env);
}
```

Or add remarks:

```csharp
[CommandRegister("GREET", "Greet a user")]
[CommandHelpRemarks("Examples:\n  GREET John\n  GREET --title Mr John")]
public class GreetCommand : AbstractCommand
{
    // ...
}
```

## Modifying Environment

By default, commands run in isolated environment contexts. To modify the parent environment, set `ModifiesEnvironment=true`:

```csharp
[CommandRegister("SETVAR", "Set environment variable")]
public class SetVarCommand : AbstractCommand
{
    // Register with modifiesEnvironment=true when adding to controller
}
```

Then in the controller:

```csharp
controller.AddCommand("MyPackage", typeof(SetVarCommand), modifiesEnvironment: true);
```

## Next Steps

- [Configure the CommandController](getting-started-controller.md)
- [Build a Plugin Package](getting-started-plugins.md)
- [Parameter Attributes Reference](api-attributes.md)
- [ICommandDelegate Interface](api-interfaces.md)
