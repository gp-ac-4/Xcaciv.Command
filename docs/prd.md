# PRD: Xcaciv.Command Framework

## 1. Product overview

### 1.1 Document title and version

- PRD: Xcaciv.Command Framework
- Version: 3.2.1
- Date: January 6, 2026

### 1.2 Product summary

Xcaciv.Command is an extensible .NET command processing framework that provides a text input command API designed to be exposed via multiple GUI interfaces. The framework enables developers to build text command-driven interfaces with minimal overhead by offering a robust infrastructure for command discovery, execution, and pipelining.

The project solves the fundamental challenge of creating text-based command interfaces by abstracting away the complexity of command parsing, parameter validation, plugin discovery, asynchronous pipeline execution, and security management. Developers can focus on implementing command logic while the framework handles orchestration, security boundaries, help generation, and multi-threaded pipeline coordination.

Built on .NET 10 by default (with optional .NET 8 compatibility builds), the framework is production-ready and includes features like dynamic plugin loading from external assemblies, async pipeline support with bounded channels, auto-generated help documentation, hierarchical sub-commands, environment isolation, audit logging, and instance-based security policies.

#### What’s new in 3.2.1

- Default target framework upgraded to **.NET 10.0** with optional multi-targeting to **.NET 8.0** via the `UseNet08` switch for compatibility builds.
- Dependency updates: Xcaciv.Loader **2.1.2**, System.IO.Abstractions **22.1.0**, Microsoft.Extensions.* **10.0.1**, System.CommandLine **2.0.1**.
- Pipeline configuration types are type-forwarded from `Xcaciv.Command` into `Xcaciv.Command.Interface` to keep existing consumers binary compatible.
- Build script now enforces .NET 10 SDK presence and auto-skips tests when multi-targeting to avoid unsupported test TFMs.

## 2. Goals

### 2.1 Business goals

- Provide a production-ready framework that reduces time-to-market for text command-driven applications
- Enable plugin ecosystem growth through secure, isolated plugin loading from external assemblies
- Support multiple UI implementations (console, web, desktop) through abstracted IO contexts
- Maintain backward compatibility while iteratively improving security and architecture
- Establish the framework as the standard solution for .NET command processing needs

### 2.2 User goals

- Implement custom commands with minimal boilerplate using attribute-based decoration
- Chain commands together using intuitive pipe syntax for complex data transformations
- Dynamically discover and load commands from plugin assemblies without application restarts
- Access auto-generated help documentation without writing help text manually
- Build secure command applications with sandboxed plugin execution and audit logging
- Integrate the framework into existing .NET applications via dependency injection
- Trace and debug command execution through structured audit events

### 2.3 Non-goals

- This is not a shell replacement or terminal emulator
- This does not provide a built-in GUI or user interface (framework only)
- This does not include command implementations for specific domains (business logic is user-provided)
- This does not manage application state beyond environment variables and command context
- This does not provide network communication or distributed command execution

## 3. User personas

### 3.1 Key user types

- Plugin developers creating reusable command packages
- Application developers integrating command processing into larger systems
- DevOps engineers building automation and tooling applications
- Framework maintainers ensuring security and reliability

### 3.2 Basic persona details

- **Plugin Developer (Sarah)**: A .NET developer building specialized commands for data transformation. Sarah needs clear documentation on command registration, parameter handling, and pipeline support. She values minimal dependencies and straightforward testing patterns.

- **Application Architect (James)**: A senior engineer integrating command processing into an enterprise application. James requires dependency injection support, configurable security policies, structured audit logging, and control over plugin loading sources. He needs to ensure commands execute within defined security boundaries.

- **CLI Tool Builder (Maria)**: A software engineer creating a command-line tool with rich functionality. Maria wants to leverage built-in commands, create custom commands, and chain operations using pipelines. She values auto-generated help and clear error messages.

- **Security Engineer (David)**: A security-focused developer responsible for ensuring command execution does not violate security policies. David needs path-based sandboxing, audit event correlation, parameter masking for sensitive data, and configurable security restrictions on plugin loading.

## 4. Functional requirements

### **Command registration and discovery** (Priority: Critical)

- The framework must discover command implementations by scanning assemblies for types implementing `ICommandDelegate` decorated with `CommandRegisterAttribute`
- Commands must be registered in a thread-safe `CommandRegistry` that supports concurrent read operations
- Built-in commands (SAY, SET, ENV, REGIF) must be registered via `RegisterBuiltInCommands()`
- External commands must be loaded from verified package directories using `AddPackageDirectory()` and `LoadCommands()`
- Command metadata (name, description, parameters) must be extracted from attributes at discovery time

### **Command execution** (Priority: Critical)

- The framework must route command lines to registered command instances via `CommandController.Run()`
- Commands must execute within isolated `IEnvironmentContext` child contexts to prevent unintended state mutation
- Commands marked `ModifiesEnvironment=true` must propagate environment changes to parent context
- Async command execution must support `CancellationToken` propagation for graceful cancellation
- Command execution must be audited via `IAuditLogger` with correlation IDs, timestamps, duration, and success status

### **Pipeline execution** (Priority: Critical)

- The framework must parse pipe-delimited command sequences using `PipelineParser` with support for quoted arguments and escape sequences
- Pipeline stages must communicate via bounded `ChannelReader`/`ChannelWriter` with backpressure handling
- Each pipeline stage must execute on independent threads with output flowing to the next stage asynchronously
- Pipeline stages must support per-stage timeouts (`PipelineConfiguration.StageTimeoutSeconds`)
- Output limits must be enforced (max bytes, max items) to prevent resource exhaustion

### **Parameter processing** (Priority: Critical)

- Commands must support ordered parameters (`CommandParameterOrderedAttribute`) that appear positionally
- Commands must support named parameters (`CommandParameterNamedAttribute`) with `--key value` syntax
- Commands must support boolean flags (`CommandFlagAttribute`) with `--flag` syntax
- Commands must support suffix parameters (`CommandParameterSuffixAttribute`) capturing trailing arguments
- Parameter definitions must indicate pipeline compatibility via `usePipe` property
- Parameter values must be converted into strongly-typed `IParameterValue<T>` instances before command execution, with validation errors surfaced early

### **Help generation** (Priority: High)

- The framework must detect help requests (`--HELP`, `-?`, `/?`) via `HelpService.IsHelpRequest()`
- Help service must generate single-line command summaries via `BuildOneLineHelp()`
- Help service must generate detailed help documentation via `BuildDetailedHelp()` including usage syntax, parameter descriptions, and remarks
- Help generation must work for both built-in and plugin commands without additional code

### **Plugin security** (Priority: Critical)

- Each plugin assembly must load in isolated `AssemblyContext` with path-based restrictions
- Security policy must default to `Strict` mode with `EnforceBasePathRestriction=true`
- Plugins must be restricted to their own directory (`basePathRestriction`)
- Security violations must raise `SecurityException` and log errors without crashing the framework
- Plugin loading must support configurable `AssemblySecurityConfiguration` for legacy compatibility

### **Audit logging** (Priority: High)

- The framework must support structured audit logging via `IAuditLogger` interface
- Audit events must include correlation IDs for tracing command execution across pipeline stages
- Command execution events must log command name, parameters (with masking), duration, and success status
- Environment changes must log variable name, old value, new value, changed-by context, and timestamp
- Audit loggers must support parameter masking via `AuditMaskingConfiguration` to redact sensitive values

### **Dependency injection support** (Priority: Medium)

- The framework must integrate with `Microsoft.Extensions.DependencyInjection` via optional adapter package
- Services must be registered using `AddXcacivCommand()` extension method
- Commands must support constructor injection when created via `IServiceProvider`
- Controller creation must support factory pattern via `CommandControllerFactory`

### **Sub-command hierarchies** (Priority: Medium)

- Commands decorated with `CommandRootAttribute` must support nested sub-commands
- Sub-command metadata must be stored in `ICommandDescription.SubCommands` collection
- Help generation must indicate root commands with `-` prefix in command listings

### **Environment variable handling** (Priority: Medium)

- Commands must access environment variables via `IEnvironmentContext.GetValue()`
- Environment changes must occur within isolated child contexts by default
- SAY command must support `%var%` syntax for environment variable substitution
- SET command must modify environment variables when `ModifiesEnvironment=true`

### **Output encoding** (Priority: Low)

- The framework must support pluggable output encoding via `IOutputEncoder` interface
- Default no-op encoder must pass output unchanged
- Custom encoders must transform command output before writing to `IIoContext`

## 5. User experience

### 5.1 Entry points & first-time user flow

- Developer installs `Xcaciv.Command.Core` and `Xcaciv.Command.Interface` NuGet packages
- Developer creates `CommandController` instance and calls `RegisterBuiltInCommands()`
- Developer creates `IIoContext` and `IEnvironmentContext` implementations (or uses `MemoryIoContext`)
- Developer calls `await controller.Run("SAY Hello World", ioContext, env)` to execute first command
- Framework outputs "Hello World" via `IIoContext.OutputChunk()`

### 5.2 Core experience

- **Creating a custom command**: Developer creates class inheriting `AbstractCommand`, decorates with `[CommandRegister("MYCOMMAND", "Description")]`, adds parameter attributes to properties, and overrides `Main()` method to implement logic. Command is packaged as class library DLL.

- **Loading plugins**: Developer calls `controller.AddPackageDirectory("/path/to/plugins")` followed by `controller.LoadCommands()`. Framework discovers all commands in `plugin_name/bin/` subdirectories and registers them in the command registry. (windows and linux paths supported)

- **Executing pipelines**: Developer runs `await controller.Run("SAY some words here | REGIF here", ioContext, env)`. Framework parses pipeline, creates bounded channels between stages, executes SAY command outputting to channel, REGIF command reads from channel and filters by regex, final output appears in `IIoContext`.

- **Accessing help**: Developer runs `await controller.GetHelpAsync("", ioContext, env)` to list all commands or `await controller.Run("MYCOMMAND --HELP", ioContext, env)` for command-specific help. Framework generates formatted help text from command attributes.

### 5.3 Advanced features & edge cases

- Configuring security policies for legacy plugins that require reflection emit or broader path access
- Implementing custom `IAuditLogger` with JSON output and log shipping to centralized monitoring
- Using dependency injection to provide services to commands via constructor injection
- Handling pipeline stage timeouts and output limits to prevent runaway processes
- Masking sensitive parameters in audit logs using regex patterns
- Correlating command execution across distributed systems using audit event correlation IDs

### 5.4 UI/UX highlights

- Zero-configuration help generation from attributes eliminates documentation burden
- Intuitive pipe syntax mirrors Unix/PowerShell conventions for immediate familiarity
- Clear exception messages for security violations, parameter validation failures, and timeout errors
- Trace messages via `IIoContext.AddTraceMessage()` provide debugging insight without cluttering output
- Bounded channels prevent memory exhaustion from producer/consumer speed mismatches

## 6. Narrative

A developer building a data processing CLI tool needs to parse CSV files, transform records, and output JSON. Using Xcaciv.Command, they create three custom commands: READCSV, TRANSFORM, and WRITEJSON. Each command inherits `AbstractCommand`, uses parameter attributes to define arguments, and implements pipeline-aware output via `yield return`.

The framework handles plugin discovery by scanning the `/plugins` directory, extracts command metadata from attributes, and generates comprehensive help documentation automatically. The developer chains commands using `READCSV input.csv | TRANSFORM --uppercase | WRITEJSON output.json`, and the framework coordinates execution using bounded channels for backpressure management.

When security requirements demand audit trails, the developer implements `IAuditLogger` to write structured JSON logs to a monitoring service. Parameter masking ensures sensitive data like API keys never appear in logs. The framework's security policy restricts each plugin to its own directory, preventing malicious plugins from accessing the filesystem beyond their sandbox.

## 7. Success metrics

### 7.1 User-centric metrics

- Time to implement first custom command: < 15 minutes
- Lines of code required for basic command: < 30 lines
- Number of built-in commands available immediately: 4 (SAY, SET, ENV, REGIF)
- Help documentation coverage: 100% (auto-generated from attributes)
- Developer satisfaction score for API clarity: Target > 4.5/5

### 7.2 Business metrics

- Number of plugin packages published by community: Target growth trajectory
- Framework adoption rate in new .NET projects requiring command processing
- Reduction in security incidents related to command execution vulnerabilities
- Backward compatibility maintenance across major versions: 100% via deprecation strategy

### 7.3 Technical metrics

- Command execution throughput: > 1000 commands/second for simple commands
- Pipeline stage latency: < 10ms overhead per stage for channel coordination
- Plugin discovery time: < 100ms for 50 plugin assemblies
- Memory overhead per command instance: < 1KB
- Security exception handling: 100% of security violations logged without framework crash
- Thread safety: Zero race conditions in concurrent command registration/execution
- Test coverage: > 90% for core framework components (currently 150 tests passing)

## 8. Technical considerations

### 8.1 Integration points

- Depends on `Xcaciv.Loader 2.0.1` for assembly loading with instance-based security policies
- Depends on `System.IO.Abstractions 21.0.2` for testable file system operations
- Optional integration with `Microsoft.Extensions.DependencyInjection` via separate adapter package
- Exposes `ICommandController` for application-level integration
- Exposes `IIoContext` interface for custom UI implementations (console, web, WPF, etc.)
- Exposes `IAuditLogger` interface for custom audit log sinks
- Default target framework is .NET 10.0; use the `UseNet08` build flag when you need to produce .NET 8.0 compatible binaries alongside .NET 10.0

### 8.2 Data storage & privacy

- Commands execute within isolated `IEnvironmentContext` child contexts preventing unintended state leakage
- Environment variables are copied to child contexts; changes require explicit `ModifiesEnvironment=true` to be copied back to global context
- Audit logs contain structured data with correlation IDs for traceability
- Audit logs may contain command parameters; `AuditMaskingConfiguration` redacts sensitive values using regex patterns
- Plugin assemblies are loaded in isolated `AssemblyContext` instances preventing cross-plugin contamination
- No persistent storage managed by framework; commands handle their own I/O

### 8.3 Scalability & performance

- Thread-safe command registry uses `ConcurrentDictionary` for lock-free reads
- Pipeline stages execute on independent threads with bounded channels for backpressure (default: 50 items)
- Per-stage timeout configuration prevents hung operations (`StageTimeoutSeconds`)
- Output limits prevent resource exhaustion (`MaxStageOutputBytes`, `MaxStageOutputItems`)
- Async/await patterns throughout eliminate thread blocking
- Command instances are created per execution and disposed afterward minimizing memory footprint

### 8.4 Potential challenges

- Plugin security model relies on `Xcaciv.Loader` path restrictions; sophisticated attacks may attempt directory traversal
- Pipeline backpressure requires careful tuning; aggressive producers can still cause memory pressure if channel bounds are too large
- Reflection-heavy plugin discovery can be slow for large plugin counts; consider caching strategies
- Attribute-based parameter binding has limited expressiveness for complex validation rules
- Async pipeline execution makes debugging stack traces more challenging
- Breaking changes in v3.0 (removal of deprecated methods) will require migration effort

## 9. Milestones & sequencing

### 9.1 Project estimate

- **Size**: Large-scale framework (currently 8 phases complete across 150+ tests)
- **Time estimate**: Core framework completed; ongoing maintenance and feature additions estimated at 1-2 developer months per major version cycle

### 9.2 Team size & composition

- **Team size**: 1-2 developers for core framework maintenance
- **Roles**: Framework architect, security engineer, community support lead
- **Skills required**: .NET 8 expertise, async programming patterns, security engineering, open source community management

### 9.3 Suggested phases

- **Phase 1: Foundation (Completed)**: Command registration, execution, basic pipeline support, built-in commands
  - Key deliverables: `CommandController`, `ICommandDelegate`, `CommandRegistry`, `PipelineExecutor`, 4 built-in commands

- **Phase 2: Plugin system (Completed)**: Dynamic plugin loading, security boundaries, directory verification
  - Key deliverables: `CommandLoader`, `Crawler`, `IVerifiedSourceDirectories`, assembly isolation via `Xcaciv.Loader`

- **Phase 3: Help & documentation (Completed)**: Auto-generated help, attribute-based documentation, help service
  - Key deliverables: `HelpService`, `BuildHelpString`, `--HELP` flag detection

- **Phase 4: Audit & tracing (Completed)**: Structured audit logging, correlation IDs, parameter masking
  - Key deliverables: `IAuditLogger`, `AuditEvent`, `AuditMaskingConfiguration`, `StructuredAuditLogger`

- **Phase 5: Configuration & DI (Completed)**: Factory pattern, dependency injection support, configuration options
  - Key deliverables: `CommandControllerFactory`, `Xcaciv.Command.DependencyInjection` package, `CommandControllerOptions`

- **Phase 6: Pipeline hardening (Completed)**: Formal parser, timeout support, resource limits, backpressure tuning
  - Key deliverables: `PipelineParser`, `PipelineConfiguration`, per-stage timeouts, output limits

- **Phase 7: Security hardening (Completed)**: Strict security defaults, configurable policies, security validation
  - Key deliverables: `AssemblySecurityConfiguration`, strict mode default, security policy validation

- **Phase 8: Thread safety & async (Completed)**: Concurrent collections, cancellation token propagation, async factory methods
  - Key deliverables: `ConcurrentDictionary` in registry, `CancellationToken` overloads, `CreateCommandAsync()`

- **Phase 9: Stabilization & v2.0 release (Completed - Dec 2025)**: Backward compatibility via deprecation, migration guide, 150 tests passing
  - Key deliverables: v2.0.0 release, migration documentation, deprecated method markers

- **Phase 10: Community & ecosystem (Ongoing)**: Plugin marketplace, community contributions, sample command packs
  - Key deliverables: Plugin template projects, sample command packages (file I/O, network, data transformation), contribution guidelines

- **Phase 11: Advanced features (Future - 3-4 months)**: Command aliases, command history, tab completion support, REPL mode
  - Key deliverables: Command alias registry, history buffer, completion provider interface

- **Phase 12: Performance optimization (Future - 2-3 months)**: Plugin caching, lazy command loading, memory profiling, benchmark suite
  - Key deliverables: Command metadata caching, assembly load optimization, performance benchmarks

## 10. User stories

### 10.1. Register and execute a built-in command

- **ID**: GH-001
- **Description**: As an application developer, I want to register built-in commands and execute them so that I can quickly add command processing to my application without writing custom commands.
- **Acceptance criteria**:
  - Developer can create `CommandController` instance
  - Developer can call `RegisterBuiltInCommands()` to register SAY, SET, ENV, REGIF commands
  - Developer can call `await controller.Run("SAY Hello", ioContext, env)` and receive "Hello" as output
  - Output appears in `IIoContext` via `OutputChunk()` method
  - Command execution completes without exceptions

### 10.2. Create a custom command with parameters

- **ID**: GH-002
- **Description**: As a plugin developer, I want to create a custom command with typed parameters so that I can implement domain-specific logic with minimal boilerplate.
- **Acceptance criteria**:
  - Developer can create class inheriting `AbstractCommand`
  - Developer can decorate class with `[CommandRegister("NAME", "Description")]`
  - Developer can add `[CommandParameterOrdered("param", "Description")]` to properties
  - Developer can add `[CommandFlag("flag", "Description")]` to boolean properties
  - Developer can override `Main()` method to implement command logic
  - Command builds successfully as class library DLL
  - Parameter values are correctly bound from command line arguments

### 10.3. Load plugin commands from external assembly

- **ID**: GH-003
- **Description**: As an application developer, I want to load command plugins from external directories so that I can extend my application with third-party commands without recompiling.
- **Acceptance criteria**:
  - Developer can call `controller.AddPackageDirectory("/plugins")`
  - Developer can call `controller.LoadCommands()`
  - Framework discovers all DLLs in `/plugins/*/bin/` subdirectories
  - Framework reflects on types to find `ICommandDelegate` implementations
  - Framework registers discovered commands in `CommandRegistry`
  - Loaded commands appear in help output via `GetHelpAsync()`
  - Loaded commands execute successfully via `Run()`

### 10.4. Chain commands with pipeline operator

- **ID**: GH-004
- **Description**: As a CLI tool builder, I want to chain multiple commands using the pipe operator so that I can transform data through multiple processing stages.
- **Acceptance criteria**:
  - Developer can execute `await controller.Run("SAY data | REGIF pattern", ioContext, env)`
  - Framework parses command line into pipeline stages using `PipelineParser`
  - Framework creates bounded channels between stages
  - First command outputs to channel via `ChannelWriter`
  - Second command reads from channel via `ChannelReader`
  - Final output appears in `IIoContext`
  - Pipeline executes asynchronously on independent threads
  - Backpressure prevents memory exhaustion when producer outpaces consumer

### 10.5. Access auto-generated help documentation

- **ID**: GH-005
- **Description**: As an end user, I want to access help documentation for commands so that I can understand command syntax and parameters without reading source code.
- **Acceptance criteria**:
  - User can execute `await controller.GetHelpAsync("", ioContext, env)` to list all commands
  - User can execute `await controller.Run("COMMAND --HELP", ioContext, env)` for command-specific help
  - Help output includes command name, description, usage syntax, parameter descriptions, and remarks
  - Help generation works for built-in commands
  - Help generation works for plugin commands
  - No custom help text code required in command implementations (auto-generated from attributes)

### 10.6. Audit command execution with structured logging

- **ID**: GH-006
- **Description**: As a security engineer, I want to audit command execution with structured logs so that I can trace actions, investigate incidents, and ensure compliance.
- **Acceptance criteria**:
  - Developer can implement `IAuditLogger` interface
  - Developer can assign logger via `controller.AuditLogger = logger`
  - Framework calls `LogAuditEvent()` for each command execution
  - Audit events include correlation ID, command name, parameters, timestamp, duration, success status
  - Audit events include package origin metadata (plugin name)
  - Environment changes trigger `LogEnvironmentChange()` calls
  - Correlation IDs persist across pipeline stages
  - Audit logger can be configured with `AuditMaskingConfiguration` to redact sensitive parameters

### 10.7. Mask sensitive parameters in audit logs

- **ID**: GH-007
- **Description**: As a security engineer, I want to mask sensitive parameters in audit logs so that credentials and secrets do not leak into log systems.
- **Acceptance criteria**:
  - Developer can configure `AuditMaskingConfiguration` with regex patterns
  - Developer can define patterns like `password`, `token`, `secret`, `apikey`
  - Audit logger redacts parameter values matching patterns before logging
  - Redacted values appear as `***REDACTED***` in logs
  - Non-sensitive parameters remain unmasked
  - Masking configuration is optional (defaults to no masking)

### 10.8. Enforce plugin security boundaries

- **ID**: GH-008
- **Description**: As a security engineer, I want to enforce security boundaries on plugin assemblies so that malicious plugins cannot access the filesystem beyond their sandbox.
- **Acceptance criteria**:
  - Framework loads plugins in isolated `AssemblyContext` with `basePathRestriction`
  - Security policy defaults to `Strict` mode with path enforcement enabled
  - Plugins attempting directory traversal raise `SecurityException`
  - Security violations are logged via trace messages
  - Framework continues operation after plugin security violation (does not crash)
  - Developer can configure `AssemblySecurityConfiguration` for legacy plugin compatibility
  - Developer can set `AllowReflectionEmit=true` for plugins requiring dynamic code generation

### 10.9. Cancel long-running command execution

- **ID**: GH-009
- **Description**: As an application developer, I want to cancel long-running command execution so that I can gracefully shut down or respond to user abort requests.
- **Acceptance criteria**:
  - Developer can pass `CancellationToken` to `Run()` method
  - Developer can signal cancellation via `CancellationTokenSource.Cancel()`
  - Framework propagates cancellation token to pipeline stages
  - Commands receive cancellation token and can check `IsCancellationRequested`
  - Async operations within commands respect cancellation token
  - Pipeline execution terminates gracefully on cancellation
  - No zombie threads remain after cancellation

### 10.10. Integrate framework with dependency injection

- **ID**: GH-010
- **Description**: As an application architect, I want to integrate the command framework with Microsoft.Extensions.DependencyInjection so that commands can receive injected services.
- **Acceptance criteria**:
  - Developer can install `Xcaciv.Command.DependencyInjection` NuGet package
  - Developer can call `services.AddXcacivCommand()` extension method
  - Framework registers `ICommandController` in DI container
  - Commands can declare constructor parameters for injected services
  - Framework resolves command dependencies from `IServiceProvider` during instantiation
  - Commands can receive scoped or singleton services
  - Controller lifetime can be configured as scoped or singleton

### 10.11. Configure pipeline stage timeouts

- **ID**: GH-011
- **Description**: As an application developer, I want to configure per-stage timeouts for pipeline execution so that hung commands do not block the entire pipeline indefinitely.
- **Acceptance criteria**:
  - Developer can configure `PipelineConfiguration.StageTimeoutSeconds`
  - Developer can pass configuration to `CommandController` constructor or factory
  - Framework enforces timeout for each pipeline stage independently
  - Timed-out stages raise `TimeoutException` and terminate gracefully
  - Subsequent pipeline stages do not execute after timeout
  - Error message indicates which stage timed out
  - Default value of 0 means unlimited (backward compatible)

### 10.12. Limit pipeline stage output to prevent resource exhaustion

- **ID**: GH-012
- **Description**: As an application developer, I want to limit pipeline stage output bytes and item counts so that runaway commands do not exhaust memory.
- **Acceptance criteria**:
  - Developer can configure `PipelineConfiguration.MaxStageOutputBytes`
  - Developer can configure `PipelineConfiguration.MaxStageOutputItems`
  - Framework enforces limits during pipeline execution
  - Commands exceeding limits are terminated
  - Error message indicates limit exceeded and which stage failed
  - Default value of 0 means unlimited (backward compatible)

### 10.13. Execute commands with isolated environment contexts

- **ID**: GH-013
- **Description**: As a framework maintainer, I want commands to execute within isolated child environment contexts so that environment mutations do not affect sibling commands or parent scope.
- **Acceptance criteria**:
  - Framework creates child context via `IEnvironmentContext.CreateChildContext()`
  - Commands receive child context in `Main()` method
  - Environment variable reads from child context access parent values
  - Environment variable writes to child context remain isolated
  - Commands with `ModifiesEnvironment=true` propagate changes to parent
  - Commands with `ModifiesEnvironment=false` do not affect parent environment
  - Pipeline stages each receive independent child contexts

### 10.14. Handle quoted arguments and escape sequences in command lines

- **ID**: GH-014
- **Description**: As a CLI tool builder, I want to use quoted arguments and escape sequences in command lines so that I can pass arguments containing spaces, pipes, and special characters.
- **Acceptance criteria**:
  - Developer can use double quotes: `SAY "Hello World"`
  - Developer can use single quotes: `SAY 'literal text'`
  - Developer can escape quotes: `SAY \"quoted\"`
  - Developer can escape pipe character: `SAY text\|more`
  - Developer can escape backslash: `SAY path\\to\\file`
  - `PipelineParser` correctly tokenizes quoted and escaped arguments
  - Quoted arguments preserve internal spaces
  - Escape sequences are processed correctly

### 10.15. Create sub-command hierarchies

- **ID**: GH-015
- **Description**: As a plugin developer, I want to create hierarchical sub-commands so that I can organize related commands under a common root.
- **Acceptance criteria**:
  - Developer can decorate command with `[CommandRoot("ROOT")]`
  - Root command can contain multiple sub-commands
  - Sub-command metadata stored in `ICommandDescription.SubCommands` collection
  - Help output indicates root commands with `-` prefix
  - User can execute sub-commands via `ROOT SUBCOMMAND` syntax
  - Sub-commands inherit root context

### 10.16. Implement custom IO context for UI integration

- **ID**: GH-016
- **Description**: As an application developer, I want to implement a custom `IIoContext` so that I can integrate command execution with my UI (console, web, WPF, etc.).
- **Acceptance criteria**:
  - Developer can implement `IIoContext` interface
  - Developer provides implementations for `OutputChunk()`, `PromptForCommand()`, `SetInputPipe()`, `SetOutputPipe()`
  - Framework calls `OutputChunk()` for command output
  - Framework calls `SetInputPipe()` and `SetOutputPipe()` for pipeline setup
  - Developer can route output to UI components (text boxes, terminal emulators, web sockets, etc.)
  - Framework does not assume console or terminal availability

### 10.17. Validate plugin security configuration

- **ID**: GH-017
- **Description**: As a security engineer, I want to validate security configuration at startup so that misconfigurations are detected early and do not lead to runtime security violations.
- **Acceptance criteria**:
  - Developer can call `AssemblySecurityConfiguration.Validate()`
  - Validation throws `InvalidOperationException` for invalid configurations
  - Validation checks: `EnforceBasePathRestriction=true` requires `SecurityPolicy=Strict`
  - Validation checks: `AllowReflectionEmit=true` is incompatible with `SecurityPolicy=Strict`
  - Validation occurs during controller initialization
  - Clear error messages guide developer to correct configuration

### 10.18. Support thread-safe command registry operations

- **ID**: GH-018
- **Description**: As a framework maintainer, I want thread-safe command registry operations so that concurrent command registration and execution do not cause race conditions or data corruption.
- **Acceptance criteria**:
  - `CommandRegistry` uses `ConcurrentDictionary<string, ICommandDescription>`
  - Concurrent `AddCommand()` calls do not corrupt registry state
  - Concurrent `GetCommand()` calls do not block or fail
  - `GetCommandSnapshot()` returns consistent snapshot for iteration
  - No explicit locks used for read operations (lock-free reads)
  - Unit tests verify concurrent registration and lookup
  - No race conditions detected under stress testing

### 10.19. Parse and validate command parameters using attributes

- **ID**: GH-019
- **Description**: As a plugin developer, I want the framework to parse and validate command parameters using attributes so that I do not need to write manual parsing logic.
- **Acceptance criteria**:
  - Framework extracts parameter definitions from command class attributes
  - Framework validates parameter count, types, and order
  - Ordered parameters must appear before named parameters
  - Named parameters support `--key value` syntax
  - Flags support `--flag` syntax (boolean)
  - Suffix parameters capture trailing arguments as array
  - Invalid parameter combinations raise clear error messages
  - Parameter binding occurs before `Main()` execution

### 10.20. Migrate from deprecated API methods to v2.0 conventions

- **ID**: GH-020
- **Description**: As an application developer, I want clear migration guidance from deprecated methods to v2.0 equivalents so that I can update my code before v3.0 removes deprecated methods.
- **Acceptance criteria**:
  - `EnableDefaultCommands()` marked `[Obsolete]` with message: "Use RegisterBuiltInCommands() instead. Will be removed in v3.0."
  - `GetHelp()` marked `[Obsolete]` with message: "Use GetHelpAsync() instead. Will be removed in v3.0."
  - Deprecated methods remain functional in v2.0 (backward compatible)
  - Migration guide document exists in `/docs` directory
  - Migration guide includes before/after code examples
  - Migration guide explains rationale for API changes
  - Compiler warnings guide developers to update code

---

**End of PRD**
