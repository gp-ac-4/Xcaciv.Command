# Xcaciv.Command: Excessive Command Framework

Excessively modular, async pipeable, command framework.

```csharp
    var controller = new Xc.Command.CommandController();
    controller.EnableDefaultCommands();

    _ = controller.Run("Say Hello to my little friend");

    // outputs: Hello to my little friend
```

Commands are .NET class libraries that contain implementations of the `Xc.Command.ICommand` interface and are decorated with attributes that describe the command and it's attributes that are used to filter and validate input as welll as output auto generated help for the user.

## Roadmap TODO:

- [X] Threaded piplineing
- [X] Internal commands `SAY` `SET`, `ENV` and `REGIF`
- [X] Sub-command structure
- [X] Auto generated help
- [ ] Stablize at 1.6
