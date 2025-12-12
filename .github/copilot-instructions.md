## Purpose
Help AI coding agents become immediately productive in the Xcaciv.Command codebase by summarizing the architecture, conventions, developer flows, and concrete code examples the agent will need to operate safely and effectively.

## Big picture
- **What this repo is:** an extensible .NET command framework that discovers and runs command plugins (class library DLLs) and also ships a set of built-in commands.
- **Primary components:**
  - `src/Xcaciv.Command/CommandController.cs` — central orchestrator: loads commands, routes command lines, handles piping and help.
  - `src/Xcaciv.Command.Core/` — base command helper code such as `AbstractCommand.cs` and `CommandParameters.cs` (parameter parsing and help generation).
  - `src/Xcaciv.Command.Interface/` — public interfaces and Attributes (e.g. `ICommandDelegate`, `ICommandController`, and attribute types under `Attributes/`).
  - `src/Xcaciv.Command.FileLoader` — plugin discovery (`ICrawler` / `Crawler`) and verified package directory handling.
  - `src/Xcaciv.Command/Commands/` — built-in commands (e.g. `SayCommand`, `SetCommand`, `EnvCommand`, `RegifCommand`).

## Key design & important behaviors
- Commands are loaded either as compiled-in types (via `EnableDefaultCommands()` or `AddCommand(Type)`) or discovered from plugin folders (`AddPackageDirectory()` + `LoadCommands()`).
- Command registration relies on attributes: `CommandRegisterAttribute` (required) and `CommandRootAttribute` (for multi-level sub-commands). See `src/Xcaciv.Command.Interface/Attributes/` for definitions.
- Parameter binding uses attributes on the command class: ordered, named, flag, and suffix parameters are defined via attributes and processed by `CommandParameters` in `Xcaciv.Command.Core`.
- Pipelining: the controller splits on `|` (PIPELINE_CHAR in `CommandController`) and connects `IIoContext` pipes; commands should support piped input via `AbstractCommand.HandlePipedChunk` or `IAsyncEnumerable` semantics.
- Runtime loading: when a type is not already loaded, `CommandController` uses `AssemblyContext(packagePath)` to load plugin assemblies and create instances dynamically.

## Files to consult for behavioral examples
- Controller & orchestration: `src/Xcaciv.Command/CommandController.cs` (routing, pipeline, help, instance creation)
- Command base and parameter rules: `src/Xcaciv.Command.Core/AbstractCommand.cs` and `src/Xcaciv.Command.Core/CommandParameters.cs`
- Attributes and parameter definitions: `src/Xcaciv.Command.Interface/Attributes/` (e.g. `CommandRegisterAttribute.cs`, `CommandParameterOrderedAttribute.cs`, `CommandFlagAttribute.cs`)
- Built-in commands: `src/Xcaciv.Command/Commands/` for canonical implementations of how to implement `ICommandDelegate` and produce help, piping, and parameter processing.

## Project-specific conventions an AI should follow
- Command classes must implement `ICommandDelegate` (typically by inheriting `AbstractCommand`) and must be decorated with `CommandRegisterAttribute`. If the command is a root that contains sub-commands, also use `CommandRootAttribute`.
- Use attribute-driven parameter definitions (ordered, named, flag, suffix). The controller expects ordered parameters to precede named parameters.
- Help is triggered by `--HELP` (case-insensitive) or by calling controller `GetHelp`. `AbstractCommand.BuildHelpString` generates canonical help output; follow its format for consistency.
- Only certain commands may modify environment. The controller tracks `ModifiesEnvironment` on `ICommandDescription`; when a command sets `ModifiesEnvironment` to true, the environment will be updated after command execution.
- When adding or modifying commands, ensure `CommandDescription.CreatePackageDescription` logic (see `CommandParameters.CreatePackageDescription`) stays compatible with the attribute semantics.

## Typical developer workflows (concrete commands)
- Build solution:
  - PowerShell (from repo root):
    ```powershell
    dotnet build Xcaciv.Command.sln -c Debug
    ```
- Run all tests:
  - PowerShell:
    ```powershell
    dotnet test Xcaciv.Command.sln --no-build
    ```
- Run a quick dev scenario (programmatic example found in `README.md`):
  - Example usage to exercise built-ins from code:
    ```csharp
    var controller = new Xc.Command.CommandController();
    controller.EnableDefaultCommands();
    await controller.Run("Say Hello to my little friend", ioContext, env);
    ```
- Load external plugin packages at runtime (from `CommandController`):
  - call `AddPackageDirectory("<plugin-base-path>")` then `LoadCommands()` and then `Run(...)` as needed.

## Testing and debugging tips for agents
- Unit tests live in `src/*Tests/` projects. Use `dotnet test` for targeted test runs (e.g. `dotnet test src/Xcaciv.Command.FileLoaderTests`).
- When a command fails during Run, `CommandController` writes trace messages and returns a brief error output chunk; inspect trace with `IIoContext.AddTraceMessage` call sites when debugging.
- For assembly-loading issues, check `AssemblyContext` use sites in `CommandController.GetCommandInstance(string, string)` and ensure plugin DLLs are in the expected `bin/<pluginName>/` layout.

## Example patterns the agent can safely apply/modify
- Adding a new built-in command: create a class under `src/Xcaciv.Command/Commands/`, inherit `AbstractCommand`, decorate with `CommandRegisterAttribute`, implement `HandleExecution` and `HandlePipedChunk`.
- Adding plugin discovery support: adjust `AddPackageDirectory` usage or `IVerfiedSourceDirectories` configuration; do not change `AssemblyContext` behavior unless you also update `GetCommandInstance` paths.

## Safety & style constraints for automated edits
- Preserve attribute names and parameter order semantics. Changes that break `CommandParameters.Process*` behavior will cause downstream runtime errors.
- Keep help text formatting consistent by using `AbstractCommand.BuildHelpString` patterns.
- Avoid changing the pipeline separator (`|`) or `IIoContext` contract without updating `CommandController` pipeline logic.

## Where to ask for human guidance
- If a change affects dynamic assembly loading, command registration, or parameter parsing, request a human review.

## FIASSE / SSEM guidance (apply before code generation)
- High-level SSEM pillars to obey: **Maintainability**, **Trustworthiness**, **Reliability**. Use these as decision criteria when generating changes.
  - Maintainability: prefer small, well-named methods (split `CommandController` logic if you must), add unit tests for `CommandParameters` when changing parsing rules, and prefer explicit attribute usage over implicit string parsing.
  - Trustworthiness: treat plugin assembly loading, environment mutation (`ModifiesEnvironment`), and run-time type activation (via `AssemblyContext`) as trust boundaries — require human review for widening these boundaries.
  - Reliability: commands and pipeline code must fail gracefully — always use trace messages (`IIoContext.AddTraceMessage`) and avoid bubbling raw exceptions to callers.

## .NET / OWASP-derived actionable rules (practical)
- Input validation: use allowlists for structured command parameters. For free-form text, normalize and context-encode on output. Validate pipe input before using it for integrity-critical values (respect `UsePipe` on parameter attributes).
- Serialization: avoid enabling automatic type resolution. When using JSON, prefer System.Text.Json with strongly-typed DTOs; if using Newtonsoft.Json, do NOT enable `TypeNameHandling` for untrusted payloads.
- Cryptography & RNG: use .NET primitives from `System.Security.Cryptography`.
  - Use `RandomNumberGenerator` for CSPRNG and `AesGcm` / `AesCng` for authenticated encryption. Do not write custom crypto.
- Secrets & keys: never hardcode secrets. Use platform vaults (Azure Key Vault, HashiCorp Vault) or `ProtectedData` for local developer scenarios. Do not commit `.secret` or key material to git.
- Logging & telemetry: emit structured logs and avoid logging secrets (session tokens, raw environment snapshots). If session correlation is needed, log a salted hash of the session id rather than the id itself.
- Dependency & runtime loading: pin versions and vet transitive dependencies. For runtime plugin assembly loading, preserve `basePathRestriction` semantics and do not widen to `"*"` without explicit review.
- Error messages & help: return generic error messages to the user but record detailed trace information via `IIoContext.AddTraceMessage` for diagnostics.



