# Maintenance v2.0 Plan

## Intentional Breaking Changes

- [ ] Make `GetHelp(string, IIoContext, IEnvironmentContext)` async by shipping `Task GetHelpAsync(string command, IIoContext output, IEnvironmentContext env, CancellationToken ct = default)` and removing sync-blocking patterns from `CommandController`. Deprecate the sync overload or provide a compatibility shim in a separate package.
- [ ] Compile and run tests

## Security Alignment with Xcaciv.Loader 2.x

- [ ] Use stricter `AssemblySecurityPolicy` defaults and allow the consuming application code to use explicit allowlists. Enforce `basePathRestriction` and disallow reflection emit by default.
- [ ] Allow the consuming application code to require signed plugin assemblies or trusted source metadata (when provided by Loader 2.x) and add verification hooks in `CommandLoader`.
- [ ] Sandbox activation by preferring the safe activation APIs added in Loader 2.x and propagate security exceptions with added context types.
- [ ] Compile and run tests

## Cancellation Token Propagation

- [ ] Extend `Main(IIoContext, IEnvironmentContext)` to accept `CancellationToken`.
- [ ] Thread the token through `Run(string, IIoContext, IEnvironmentContext)` and both `ExecuteAsync(string, IIoContext, IEnvironmentContext)` overloads.
- [ ] Compile and run tests

## Pipeline Hardening

- [ ] Move pipeline parsing to a dedicated parser with a formal grammar that allows quoted arguments and delimiter escaping.
- [ ] Add per-stage cancellation, timeouts, and resource bounding in `PipelineConfiguration`.
- [ ] Support typed pipe payloads (for example JSON schema-validated payloads) to improve trustworthiness.
- [ ] Compile and run tests

## Registry and Factory Lifecycle

- [ ] Make `CommandRegistry` thread-safe 
- [ ] Update `CreateCommand(ICommandDescription, IIoContext)` to be async to avoid sync waits on `SetParameters(string[])` and provide a sync wrapper for legacy callers.
- [ ] Compile and run tests

## Parameters and Help Consistency

- [ ] Normalize `--HELP` handling in the controller rather than in individual executors and provide standardized help output (single source) with consistent formatting.
- [ ] Introduce `IHelpService` to centralize help building so commands supply metadata instead of rendering help strings directly.
- [ ] Compile and run tests

## Auditing and Tracing

- [x] Standardize audit logs with structured events and masking strategies while allowing configurable parameter redaction.
- [x] Emit correlation IDs for commands and pipeline stages and include command package origin metadata.
- [x] Compile and run tests

## Configuration and Dependency Injection

- [ ] Bind configuration via `IOptions` where appropriate and provide default wiring inside a startup module.
- [ ] Support external DI containers explicitly and expose extension methods to register components.
- [ ] Compile and run tests

## Breaking API Cleanup

- [ ] Remove legacy overloads that imply synchronous execution.
- [ ] Rename ambiguous members for clarity (for example rename `EnableDefaultCommands()` to `RegisterBuiltInCommands`).
- [ ] Consolidate interfaces in `AbstractCommandParameter.cs` with clearer responsibilities and XML documentation.
- [ ] Compile and run tests
