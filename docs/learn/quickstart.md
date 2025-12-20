# Quick Start Guide

Get up and running with Xcaciv.Command in 5 minutes.

## 1. Install the Framework

Add NuGet packages to your project:

```bash
dotnet add package Xcaciv.Command
dotnet add package Xcaciv.Command.Core
dotnet add package Xcaciv.Command.Interface
```

## 2. Create Your First Command

Create a file `GreetCommand.cs`:

```csharp
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

[CommandRegister("GREET", "Greet someone")]
public class GreetCommand : AbstractCommand
{
    [CommandParameterOrdered("name", "Person's name")]
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

## 3. Create a Console Application

Create `Program.cs`:

```csharp
using Xcaciv.Command;
using Xcaciv.Command.Interface;

var controller = new CommandController();
controller.EnableDefaultCommands(); // Enable built-in commands
controller.AddCommand("MyApp", typeof(GreetCommand));

var ioContext = new ConsoleIoContext();
var env = new EnvironmentContext();

string commandLine;
while ((commandLine = await ioContext.PromptForCommand("xcaciv> ")) != null)
{
    try
    {
        await controller.Run(commandLine, ioContext, env);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
```

## 4. Test Your Command

Build and run:

```bash
dotnet build
dotnet run
```

Try these commands:

```
xcaciv> GREET Alice
Hello, Alice!

xcaciv> GREET Bob
Hello, Bob!

xcaciv> SAY World | GREET
Hello, World!

xcaciv> HELP GREET
GREET:
  Greet someone

Usage:
  GREET <name>

Options:
  name - Person's name
```

## 5. Load Plugins

Create a plugin directory structure:

```
/plugins/
  MyPlugin/
    bin/
      MyPlugin.dll
      MyPlugin.deps.json
```

Update your application:

```csharp
var controller = new CommandController();
controller.EnableDefaultCommands();
controller.AddPackageDirectory("/plugins");
controller.LoadCommands();

// ... rest of code
```

## 6. Create a Pipeline

Commands can be chained with `|`:

```csharp
// From console
xcaciv> SAY line1 | SAY line2 | REGIF line1

// From code
await controller.Run("SAY Hello | UPPERCASE", ioContext, env);
```

## Built-in Commands

| Command | Usage | Description |
|---------|-------|-------------|
| SAY | SAY text | Output text |
| SET | SET NAME value | Set environment variable |
| ENV | ENV | Show environment variables |
| REGIF | PATTERN | Filter by regex |
| HELP | HELP [command] | Show help |

## Common Patterns

### Add Parameters to Your Command

```csharp
[CommandRegister("SEND", "Send a message")]
public class SendCommand : AbstractCommand
{
    [CommandParameterOrdered("to", "Recipient")]
    public string To { get; set; }

    [CommandParameterOrdered("message", "Message text")]
    public string Message { get; set; }

    [CommandParameterNamed("subject", "Email subject")]
    public string Subject { get; set; }

    [CommandFlag("urgent", "Mark as urgent")]
    public bool Urgent { get; set; }

    public override string HandleExecution(string[] parameters, IEnvironmentContext env)
    {
        var priority = Urgent ? "URGENT" : "normal";
        return $"[{priority}] To: {To}, Subject: {Subject}, Message: {Message}";
    }
}
```

Usage:
```
xcaciv> SEND alice "Please review" --subject "Code Review" --urgent
[URGENT] To: alice, Subject: Code Review, Message: Please review
```

### Handle Piped Input

```csharp
[CommandRegister("REVERSE", "Reverse text")]
public class ReverseCommand : AbstractCommand
{
    public override async IAsyncEnumerable<IResult<string>> Main(
        IIoContext ioContext, 
        IEnvironmentContext env)
    {
        if (ioContext.HasPipedInput)
        {
            await foreach (var line in ioContext.ReadInputPipeChunks())
            {
                var reversed = new string(line.Reverse().ToArray());
                yield return CommandResult<string>.Success(reversed);
            }
        }
    }
}
```

Usage:
```
xcaciv> SAY hello | REVERSE
olleh
```

### Access Environment Variables

```csharp
public override string HandleExecution(string[] parameters, IEnvironmentContext env)
{
    var appName = env.GetVariable("APP_NAME") ?? "MyApp";
    var userName = env.GetVariable("USER") ?? "unknown";
    
    return $"User {userName} using {appName}";
}
```

### Modify Environment

```csharp
[CommandRegister("SETVAR", "Set variable")]
public class SetVarCommand : AbstractCommand
{
    [CommandParameterOrdered("name", "Variable name")]
    public string Name { get; set; }

    [CommandParameterOrdered("value", "Variable value")]
    public string Value { get; set; }

    public override string HandleExecution(string[] parameters, IEnvironmentContext env)
    {
        env.SetVariable(Name, Value);
        return $"Set {Name}={Value}";
    }
}
```

Register with modification flag:
```csharp
controller.AddCommand("MyApp", typeof(SetVarCommand), modifiesEnvironment: true);
```

## Debug Your Commands

### Use MemoryIoContext for Testing

```csharp
[Fact]
public async Task GreetCommand_ReturnsGreeting()
{
    var controller = new CommandController();
    controller.AddCommand("test", typeof(GreetCommand));

    var io = new MemoryIoContext(new[] { "Alice" });
    var env = new EnvironmentContext();

    await controller.Run("GREET Alice", io, env);

    string output = io.GetOutput();
    Assert.Equal("Hello, Alice!", output);
}
```

### Enable Audit Logging

```csharp
public class ConsoleAuditLogger : IAuditLogger
{
    public async Task LogAsync(string entry)
    {
        Console.WriteLine($"[AUDIT] {entry}");
        await Task.CompletedTask;
    }
}

controller.SetAuditLogger(new ConsoleAuditLogger());
```

## Next Steps

- [Create a Command](getting-started-create-command.md) - Detailed guide
- [Build a Plugin](getting-started-plugins.md) - Package commands for distribution
- [Use Pipelines](getting-started-pipelines.md) - Chain commands together
- [API Reference](api-core.md) - Complete API documentation

## Troubleshooting

### Command Not Found

**Problem:** "Command 'MYCOMMAND' is not registered"

**Solution:** Ensure command class has `CommandRegisterAttribute` and is added to controller

```csharp
[CommandRegister("MYCOMMAND", "description")] // ? Don't forget this
public class MyCommand : AbstractCommand { }

controller.AddCommand("pkg", typeof(MyCommand)); // ? Register it
```

### Plugin Not Loading

**Problem:** "No plugins found" or "No plugin files found"

**Solution:** Check directory structure

```
/plugins/
  MyPlugin/
    bin/              ? LoadCommands() searches here by default
      MyPlugin.dll
      MyPlugin.deps.json
```

### Parameter Not Working

**Problem:** Parameter not being parsed correctly

**Solution:** Ensure attributes are in correct order

```csharp
[CommandRegister("CMD", "desc")]
public class MyCommand : AbstractCommand
{
    [CommandParameterOrdered("first", "desc")]  // 1. Ordered first
    public string First { get; set; }

    [CommandFlag("flag", "desc")]               // 2. Flags second
    public bool Flag { get; set; }

    [CommandParameterNamed("key", "desc")]      // 3. Named third
    public string Key { get; set; }

    [CommandParameterSuffix("rest", "desc")]    // 4. Suffix last
    public string[] Rest { get; set; }
}
```

## Resources

- [API Reference](api-interfaces.md)
- [Architecture](architecture.md)
- [GitHub Repository](https://github.com/xcaciv/Xcaciv.Command)
