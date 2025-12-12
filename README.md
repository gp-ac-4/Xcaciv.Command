# Xcaciv.Command: Excessive Command Framework

Excessively modular, async pipeable, command framework.

```csharp
    var controller = new Xc.Command.CommandController();
    controller.EnableDefaultCommands();

    _ = controller.Run("Say Hello to my little friend");

    // outputs: Hello to my little friend
```

Commands are .NET class libraries that contain implementations of the `Xc.Command.ICommand` interface and are decorated with attributes that describe the command and its parameters. These attributes are used to filter and validate input as well as output auto-generated help for the user.

## Features

- **Async Pipeline Support**: Commands can be chained with `|` for threaded pipeline execution
- **Plugin System**: Dynamic command loading from external assemblies
- **Secure Plugin Loading**: Built on Xcaciv.Loader 2.0.1 with instance-based security policies
- **Auto-Generated Help**: Comprehensive help generation from command attributes
- **Sub-Commands**: Hierarchical command structure support
- **Built-in Commands**: `SAY`, `SET`, `ENV`, and `REGIF` included

## Dependencies

- **Xcaciv.Loader 2.0.1**: Assembly loading with instance-based security configuration
- **System.IO.Abstractions 21.0.2**: File system abstraction for testability

## Security

This framework uses Xcaciv.Loader 2.0.1's instance-based security policies:
- **Default Security Policy**: Each plugin is restricted to its own directory
- **No Wildcard Access**: Replaces v1.x wildcard (`*`) restrictions with proper path-based security
- **Security Exception Handling**: Graceful handling of security violations with detailed logging
- **Per-Instance Configuration**: Each AssemblyContext has independent security settings

## Roadmap

- [X] Threaded pipelining
- [X] Internal commands `SAY` `SET`, `ENV` and `REGIF`
- [X] Sub-command structure
- [X] Auto generated help
- [X] Migrate to Xcaciv.Loader 2.0.1 with instance-based security
- [ ] Stabilize at 1.6

## Version History

### 1.5.18 (Current)
- **Breaking Change**: Migrated to Xcaciv.Loader 2.0.1
- Implemented instance-based security policies for plugin loading
- Replaced wildcard path restrictions with directory-based security
- Added comprehensive security exception handling
- Fixed null reference warnings in .NET 8

## License

BSD-3-Clause
