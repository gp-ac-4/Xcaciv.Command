# Xcaciv.Command: Excessive Command Framework

Excessively modular, async pipeable, command framework with strongly-typed parameter support.

```csharp
    var controller = new Xc.Command.CommandController();
    controller.RegisterBuiltInCommands();

    _ = await controller.Run("Say Hello to my little friend", ioContext, env);

    // outputs: Hello to my little friend
```

Commands are .NET class libraries that contain implementations of the `Xc.Command.ICommand` interface and are decorated with attributes that describe the command and its parameters. These attributes are used to filter and validate input as well as output auto-generated help for the user.

## Features

- **Strongly-Typed Parameters**: Generic parameter values with compile-time type safety and early validation
- **Async Pipeline Support**: Commands can be chained with `|` for threaded pipeline execution
- **Plugin System**: Dynamic command loading from external assemblies
- **Secure Plugin Loading**: Built on Xcaciv.Loader 2.1.2 with instance-based security policies
- **Auto-Generated Help**: Comprehensive help generation from command attributes
- **Sub-Commands**: Hierarchical command structure support
- **Built-in Commands**: `SAY`, `SET`, `ENV`, and `REGIF` included
- **Rich Type Support**: String, numeric types, bool, Guid, DateTime, JsonElement with nullable variants

## Dependencies

- **Xcaciv.Loader 2.1.2**: Assembly loading with instance-based security configuration
- **System.IO.Abstractions 22.1.0**: File system abstraction for testability

## Security

This framework uses Xcaciv.Loader 2.1.0's instance-based security policies:
- **Default Security Policy**: Each plugin is restricted to its own directory
- **No Wildcard Access**: Replaces v1.x wildcard (`*`) restrictions with proper path-based security
- **Security Exception Handling**: Graceful handling of security violations with detailed logging
- **Per-Instance Configuration**: Each AssemblyContext has independent security settings

For detailed security model, plugin development guidelines, and vulnerability reporting procedures, see `SECURITY.md`.

### Audit Logging

Command execution and environment changes can be logged via `IAuditLogger`:

```csharp
// Example: Using audit logging
var controller = new CommandController();
controller.EnableDefaultCommands();
var env = new EnvironmentContext();

// Provide an audit logger implementation
var auditLogger = new YourAuditLoggerImplementation();
controller.AuditLogger = auditLogger;

// Command execution and environment changes are now logged
await controller.Run("say hello", ioContext, env);
```

Logs include:
- Command execution with parameters, duration, and success status
- Environment variable changes with old/new values
- Timestamps in UTC for consistency

See `SECURITY.md` for secure audit logging patterns.

## Roadmap

- [X] Threaded pipelining
- [X] Internal commands `SAY` `SET`, `ENV` and `REGIF`
- [X] Sub-command structure
- [X] Auto generated help
- [X] Migrate to Xcaciv.Loader 2.1.0 with instance-based security

## Version History

### 3.1.0 (Current)
- **Generic Parameter System:** Fully generic `ParameterValue<T>` with compile-time type safety
- **Breaking:** `IParameterValue` adds `DataType`, `UntypedValue`, `GetValue<T>()`, `TryGetValue<T>()`
- **Breaking:** Removed `As<T>()` and `Value` property - use `GetValue<T>()`
- **Factory Creation:** Runtime `ParameterValue.Create()` for dynamic type instantiation
- **Enhanced Diagnostics:** Rich type mismatch and validation error messages
- **InvalidParameterValue Sentinel:** Maintained for conversion failure handling
- See [Parameter System Summary](docs/parameter-system-summary.md) for details

### 3.0.0
- **BREAKING:** Removed deprecated `EnableDefaultCommands()` - use `RegisterBuiltInCommands()`
- **BREAKING:** Removed deprecated `GetHelp()` - use `await GetHelpAsync()`
- Cleaned up API surface for better maintainability
- See [Migration Guide v3.0](docs/migration_guide_v3.0.md) for upgrade instructions

### 2.1.3
- Bumped all package versions to 2.1.3
- Updated to Xcaciv.Loader 2.1.2
- Documentation improvements
- Maintains .NET 10 targeting

## Documentation

- [Parameter System Summary](docs/parameter-system-summary.md) - Generic parameter implementation guide
- [Project History](docs/project-history.md) - Development history and architectural decisions
- [Migration Guide v3.0](docs/migration_guide_v3.0.md) - Upgrade instructions
- [Build Guide](BUILD.md) - Build script usage and configuration
- [Security Policy](SECURITY.md) - Security model and vulnerability reporting
- [Architecture Diagram](docs/architecture_diagram.md) - System component overview

## License

AGPL-3.0 License. See LICENSE file for details.
