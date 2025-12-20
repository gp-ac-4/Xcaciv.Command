# Xcaciv.Command Framework

Xcaciv.Command is an extensible .NET command framework that enables building command-line applications with plugin support, pipelining, and dynamic command discovery.

## What is Xcaciv.Command?

Xcaciv.Command provides a comprehensive framework for creating command-driven applications with the following capabilities:

- **Command Framework**: Execute commands with attribute-based parameter definitions
- **Plugin System**: Dynamically load command plugins from assemblies at runtime
- **Pipeline Support**: Chain multiple commands together using `|` separator
- **Environment Context**: Manage isolated environment variables with optional persistence
- **Audit Logging**: Track command execution and environment changes
- **Help System**: Auto-generate help from command attributes
- **Flexible I/O**: Abstract interface for console, web, or custom I/O implementations

## Key Concepts

### Commands
Commands implement `ICommandDelegate` and are decorated with `CommandRegisterAttribute`. They process inputs and produce outputs that can be piped to other commands.

### Parameters
Commands accept parameters defined via attributes:
- **Ordered Parameters**: Positional arguments (`CommandParameterOrderedAttribute`)
- **Named Parameters**: Key-value arguments (`CommandParameterNamedAttribute`)
- **Flags**: Boolean switches (`CommandFlagAttribute`)
- **Suffix Parameters**: Trailing arguments (`CommandParameterSuffixAttribute`)

### Pipeline
Multiple commands can be chained using the `|` character. Output from one command becomes input to the next.

### Plugin System
Commands can be compiled into assemblies and discovered at runtime from plugin directories using the `Crawler` class.

## Get Started

- [Create a Command](getting-started-create-command.md)
- [Configure the Controller](getting-started-controller.md)
- [Build a Plugin Package](getting-started-plugins.md)
- [Use Pipelines](getting-started-pipelines.md)

## API Reference

- [Command Interfaces](api-interfaces.md)
- [Attributes](api-attributes.md)
- [Commands and Core Types](api-core.md)
- [Exception Types](api-exceptions.md)

## Built-in Commands

The framework ships with several built-in commands:
- `Say` - Output text
- `Set` - Set environment variables
- `Env` - Display environment variables
- `Regif` - Regular expression filtering
- `Help` - Display command help

## Key Design Principles

**Security**: Commands run in isolated contexts. Only commands marked with `ModifiesEnvironment=true` can persist environment changes.

**Maintainability**: Attribute-driven design decouples parameter definitions from implementation, making commands easy to understand and test.

**Extensibility**: Plugin architecture allows adding commands without modifying the core framework.

**Reliability**: Comprehensive error handling and audit logging provide visibility into command execution.
