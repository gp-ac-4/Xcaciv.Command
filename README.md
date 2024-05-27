# Xcaciv.Command

Excessively modular, async pipeable, text command framework.

```csharp
    var controller = new Xc.Command.CommandController();
    controller.EnableDefaultCommands();

    _ = controller.Run("Say Hello to my little friend");

    // outputs: Hello to my little friend
```

Commands are .NET class libraries that contain implementations of the `Xc.Command.ICommand` interface.

## Roadmap

- [X] Threaded piplineing
- [X] Internal commands `SAY` and `REGIF`
