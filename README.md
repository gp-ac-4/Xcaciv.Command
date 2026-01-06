# Xcaciv.Command: Excessive Command Framework

Excessively modular, async pipeable, command framework with strongly-typed parameter support.

```csharp
    var controller = new Xc.Command.CommandController();
    controller.RegisterBuiltInCommands();

    _ = await controller.Run("Say Hello to my little friend", ioContext, env);

    // outputs: Hello to my little friend
```

Commands are .NET class libraries that contain implementations of the `Xc.Command.ICommand` interface and are decorated with attributes that describe the command and its parameters. These attributes are used to filter and validate input as well as output auto-generated help for the user.

**Authoring note:** When building a new command, start from the template and guidance in [COMMAND_TEMPLATE.md](COMMAND_TEMPLATE.md) to match the latest interfaces (`ICommandDelegate`/`AbstractCommand`), `OutputFormat`, and environment propagation rules.

## Features

- **Strongly-Typed Parameters**: Generic `IParameterValue<T>` with compile-time type safety and pre-validation
- **Type-Safe Parameter Access**: `GetValue<T>()` method prevents accessing unconverted strings
- **Rich Type Support**: String, numeric types, bool, Guid, DateTime, JsonElement with nullable variants
- **Enhanced Diagnostics**: Detailed type mismatch and validation error messages with parameter context
- **Async Pipeline Support**: Commands can be chained with `|` for threaded pipeline execution
- **Plugin System**: Dynamic command loading from external assemblies
- **Secure Plugin Loading**: Built on Xcaciv.Loader 2.1.2 with instance-based security policies
- **Auto-Generated Help**: Comprehensive help generation from command attributes
- **Sub-Commands**: Hierarchical command structure support
- **Built-in Commands**: `SAY`, `SET`, `ENV`, and `REGIF` included

## Dependencies

- **Xcaciv.Loader 2.1.2**: Assembly loading with instance-based security configuration
- **System.IO.Abstractions 22.1.0**: File system abstraction for testability

## Security

This framework uses Xcaciv.Loader 2.1.0's instance-based security policies:
- **Default Security Policy**: Each plugin is restricted to its own directory
- **No Wildcard Access**: Replaces v1.x wildcard (`*`) restrictions with proper path-based security
- **Security Exception Handling**: Graceful handling of security violations with detailed logging
- **Per-Instance Configuration**: Each AssemblyContext has independent security settings
- **Type Safety**: Parameters validated and converted before command execution prevents injection attacks

For detailed security model, plugin development guidelines, and vulnerability reporting procedures, see `SECURITY.md`.

### Audit Logging

Command execution and environment changes can be logged via `IAuditLogger`:

```csharp
// Example: Using audit logging
var controller = new CommandController();
controller.RegisterBuiltInCommands();
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
- [X] Type-safe parameter system with generics

## Version History

### 3.1.0 (Current)
- **Type-Safe Parameter System:** Fully generic `IParameterValue<T>` with compile-time type safety
- **Enhanced Security:** Parameters validated before execution, no direct raw string access
- **Breaking:** Commands use `GetValue<T>()` instead of `RawValue`
- **Comprehensive Tests:** 36 new tests for built-in commands (232 total passing)
- **Rich Diagnostics:** Detailed type mismatch and validation error messages
- See [CHANGELOG](CHANGELOG.md#310) for full details

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

- [CHANGELOG](CHANGELOG.md) - Complete version history and release notes
- [Command Template](COMMAND_TEMPLATE.md) - Template and guide for implementing new commands
- [Parameter System Implementation](PARAMETER_SYSTEM_IMPLEMENTATION.md) - Type-safe parameter guide
- [Command Test Coverage](COMMAND_TEST_COVERAGE_COMPLETE.md) - Test coverage report
- [RawValue Removal](RAWVALUE_REMOVAL_COMPLETE.md) - Security improvement details
- [Migration Guide v3.0](docs/migration_guide_v3.0.md) - Upgrade instructions
- [Build Guide](BUILD.md) - Build script usage and configuration
- [Security Policy](SECURITY.md) - Security model and vulnerability reporting
- [Architecture Diagram](docs/architecture_diagram.md) - System component overview

## License

AGPL-3.0 License. See LICENSE file for details.
