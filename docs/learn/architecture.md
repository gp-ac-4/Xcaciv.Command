# Architecture and Design

Overview of Xcaciv.Command framework architecture and design patterns.

## High-Level Architecture

```
???????????????????????????????????????????????
?         User Application / UI Layer         ?
???????????????????????????????????????????????
                   ?
                   ?
???????????????????????????????????????????????
?         CommandController                   ?
?  (Orchestration & Pipeline Management)     ?
???????????????????????????????????????????????
       ?              ?              ?
       ?              ?              ?
???????????????? ???????????????? ????????????????
? CommandLoader? ?PipelineExecutor? CommandExecutor?
?(Discovery)   ? ? (Coordination) ? (Execution)   ?
???????????????? ???????????????? ????????????????
       ?                                 ?
       ?                                 ?
????????????????????????????????????????????????
?  CommandRegistry & CommandFactory            ?
?  (Registration & Instance Creation)          ?
????????????????????????????????????????????????
       ?                                    ?
       ?                                    ?
????????????????????          ????????????????????????
? Built-in Commands?          ? Plugin Assemblies    ?
? (Say, Set, Env,  ?          ? (Dynamically Loaded) ?
?  Regif, Help)    ?          ? (AssemblyContext)    ?
????????????????????          ????????????????????????
```

## Core Components

### CommandController
- **Responsibility:** Central orchestrator
- **Key Methods:** `Run()`, `AddPackageDirectory()`, `LoadCommands()`
- **Manages:** Command routing, pipeline setup, help system

### CommandLoader
- **Responsibility:** Plugin discovery and registration
- **Collaborates:** Crawler, VerifiedSourceDirectories
- **Ensures:** Security through directory verification

### PipelineExecutor
- **Responsibility:** Execute pipeline stages with proper channel management
- **Creates:** Bounded channels between stages
- **Handles:** Backpressure, timeouts, cancellation

### CommandFactory
- **Responsibility:** Instantiate command types
- **Uses:** AssemblyContext for plugin assembly loading
- **Ensures:** Security policy enforcement

### CommandRegistry
- **Responsibility:** Maintain command metadata
- **Stores:** Command descriptions, parameter definitions
- **Enables:** Help generation, command lookup

## Plugin Architecture

### Discovery Process

```
Directory Search
    ?
Find *.dll in bin/ subdirectories
    ?
Reflect on types in DLL
    ?
Find ICommandDelegate implementations
    ?
Extract CommandRegisterAttribute metadata
    ?
Create CommandDescription objects
    ?
Register in CommandRegistry
```

### Security Boundaries

1. **Directory Verification**: Plugins loaded from approved directories only
2. **AssemblyContext Isolation**: Plugin assemblies isolated from host
3. **Permission Checking**: Security policy enforced at load time
4. **Type Validation**: Only valid command types allowed

### Runtime Loading

```
Plugin Request
    ?
Lookup in CommandRegistry
    ?
Is type loaded? ? No ? Create AssemblyContext
                ?
              Load assembly
                ?
              Reflect type
                ?
              Create instance via CommandFactory
    ?
Execute command
    ?
Dispose instance
```

## Pipeline Architecture

### Channel-Based Communication

Each pipeline stage connects via bounded channels:

```
Stage 1        Channel 1-2       Stage 2       Channel 2-3      Stage 3
????????    ????????????????    ????????    ????????????????   ????????
?Output?????? Max 50 items ??????Input ?    ? Max 50 items ?????Output?
????????    ?              ?    ????????    ?              ?   ????????
            ? Backpressure ?               ? Backpressure ?
            ????????????????               ????????????????
```

### Stage Lifecycle

Each stage:
1. Reads parameters and piped input
2. Executes command logic
3. Yields output chunks
4. Framework writes to output channel
5. Disposes resources
6. Handles cancellation/timeout

### Backpressure Handling

```
Producer ? Channel (bounded)
              ?
         Is queue full?
         ?? No: Accept & continue
         ?? Yes:
            ?? Block: Wait for consumer
            ?? DropOldest: Remove oldest
            ?? DropNewest: Remove newest
```

## Environment Isolation

### Isolated Contexts

```
Parent Environment
??? Variable A
??? Variable B
??? Variable C
    ?
[Command runs in child context]
    ?
    ??? Variable A (copy)
    ??? Variable B (copy)
    ??? Variable C (copy)
    ??? Variable D (new)
    ?
[Changes discarded unless ModifiesEnvironment=true]
```

### Modification Tracking

Commands marked with `ModifiesEnvironment=true`:
- Execute in non-isolated context
- Changes propagate to parent
- Tracked in audit log
- Visible in subsequent commands

## Parameter Processing

### Parameter Resolution

```
Command Line: "MYCMD value1 --key value2 --flag arg1 arg2"
                 ?
            Parser
                 ?
         ??????????????????????
         ?                    ?
    OrderedParameters    NamedParameters    FlagParameters    SuffixParameters
    [value1]            [key=value2]       [flag=true]       [arg1, arg2]
         ?                    ?                  ?                  ?
         ????????????????????????????????????????????????????????????
                   ?
              Property Binding
                   ?
              Command Instance
```

### Validation

Parameters are validated:
- Required parameters provided
- Types match expected types
- Values within allowed set (if restricted)
- Piped input handled correctly

## Error Handling Strategy

### Exception Flow

```
Command Execution
    ?
    ?? Catches specific exceptions
    ?  (e.g., FileNotFoundException)
    ?  ?
    ?  Wraps with context
    ?  (e.g., ConfigurationException)
    ?  ?
    ?  Throws contextual exception
    ?
    ?? Unhandled exceptions propagate
    ?  ?
    ?  Pipeline stage catches
    ?  ?
    ?  Logs trace message
    ?  ?
    ?  Returns failure result
    ?
    ?? UI layer catches all exceptions
       ?? Logs & displays to user
```

## Audit Logging

### Logged Events

1. **Command Execution**
   - Start: command name, parameters
   - Complete: duration, success/failure
   - Errors: exception details

2. **Environment Changes**
   - Variable modifications
   - Only for ModifiesEnvironment commands
   - Tracks before/after values

3. **Pipeline Events**
   - Stage start/end
   - Backpressure events
   - Timeouts
   - Cancellation

### Audit Log Entry Format

```
[Timestamp] [Level] [Component] Message
[2024-01-15 10:30:45] [INFO] CommandController Running: MYCOMMAND arg1 arg2
[2024-01-15 10:30:45] [DEBUG] PipelineExecutor Stage 1 start: MYCOMMAND
[2024-01-15 10:30:45] [INFO] EnvironmentContext Set: USER=john
[2024-01-15 10:30:46] [INFO] CommandController Completed: MYCOMMAND in 1234ms
```

## Thread Safety

### Safe Patterns

1. **IIoContext**: Requires single-threaded access per command
2. **IEnvironmentContext**: Thread-safe for read operations, use locks for modifications
3. **Channels**: Thread-safe for multi-reader/writer scenarios (used internally)

### Pipeline Concurrency

```
Main Thread (Orchestration)
    ?
    ?? Task: Stage 1 execution
    ?? Task: Stage 2 execution
    ?? Task: Stage 3 execution
    ?? Await all tasks
        ?
        ?? Output collection
```

Each stage runs concurrently with proper synchronization via channels.

## Dependency Injection

### Inversion of Control

```
CustomLogger
      ?
      ?
CommandController ????? CommandLoader
                    ??? PipelineExecutor
                    ??? CommandExecutor
                    ??? CommandFactory
                    ??? CommandRegistry
```

Constructor accepts:
- Custom command registry
- Custom loader
- Custom executors
- Custom factory
- Service provider

## Security Model

### Trust Boundaries

1. **Plugin Loading**: Restricted to verified directories
2. **Assembly Resolution**: AssemblyContext with policy enforcement
3. **Environment Modification**: Explicit opt-in via ModifiesEnvironment
4. **Input Validation**: Framework validates parameters per attribute definitions

### FIASSE Principles Applied

- **Maintainability**: Clear separation of concerns, low cyclomatic complexity
- **Trustworthiness**: Audit logging, explicit permissions, parameter validation
- **Reliability**: Comprehensive error handling, resource cleanup, timeout protection

---

## See Also

- [Getting Started](index.md)
- [Advanced Command Patterns](advanced-command-patterns.md)
- [Plugin Security](advanced-plugin-security.md)
