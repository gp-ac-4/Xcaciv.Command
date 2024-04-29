# Xcaciv.Command

Excessively modular command framework capable of executing a line of text as a command with options without leaving command assembly in memory.

```csharp
    var commandManager = new Xc.Command.Manager();
    commandManager.LoadCommands(new Crawler());
    _ = commandManager.Run("Say Hello to my little friend");

    // outputs: Hello to my little friend
```

Commands are .NET class libraries that contain implementations of the `Xc.Command.ICommand` interface.