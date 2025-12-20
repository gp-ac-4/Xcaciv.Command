# Xcaciv.Command.Extensions.Commandline

`Xcaciv.Command.Extensions.Commandline` provides a thin adapter for running `System.CommandLine` commands inside the `Xcaciv.Command` pipeline. Use `CommandLineCommand<T>` to wrap your `System.CommandLine.Command` instances and expose them as `ICommandDelegate` implementations.

## Usage

```csharp
var systemCommand = new Command("greet", "Writes a greeting");
systemCommand.SetHandler((InvocationContext context) =>
{
    context.Console.Out.Write("hello");
    return Task.CompletedTask;
});

var adapter = new CommandLineCommand<Command>();
adapter.SetCommand(systemCommand);
```

The adapter returns `IResult<string>` entries that carry captured command output for use by the `Xcaciv.Command` controller and pipeline.
