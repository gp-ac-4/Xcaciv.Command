# Xcaciv.Command

Excessively modular, async pipeable, text command framework.

```csharp
    var commandManager = new Xc.Command.Manager();
    commandManager.LoadCommands(new Crawler());
    _ = commandManager.Run("Say Hello to my little friend");

    // outputs: Hello to my little friend
```

Commands are .NET class libraries that contain implementations of the `Xc.Command.ICommand` interface.

## Roadmap

- [X] Threaded piplineing
- [ ] Internal commands `SAY` and `REGIF`
- [ ] External commands `HEAD` and `SITECERT` (start of web util package)
- [ ] Binary package installer
- [ ] Cupcake shell platform
- [ ] Source package installer
- [ ] dotnet tool installer