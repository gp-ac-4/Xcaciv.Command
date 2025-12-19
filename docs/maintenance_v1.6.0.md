# Maintenance Plan v1.6.0

Scope
- Goal: Improve maintainability, trustworthiness, and reliability per repo guidance while keeping behavior stable.
- Applies to: `src/Xcaciv.Command/*`, `src/Xcaciv.Command.Interface/*`, `src/Xcaciv.Command.Core/*`, `src/Xcaciv.Command.FileLoader/*`, packaging in `Xcaciv.Command.csproj`.

Inputs Reviewed
- .github/copilot-instructions.md (security, exception handling, naming, SSEM/maintainability)
- No `.github/instructions/*.md` files found in workspace.
- Key orchestration and runtime files reviewed: `CommandController`, `CommandExecutor`, `PipelineExecutor`, `CommandLoader`, `CommandFactory`, `CommandRegistry`, related interfaces and attributes.

Findings (high level)
- Overall design is modular and clean; DI seams exist (`ICommandFactory`, `ICommandLoader`, `IPipelineExecutor`).
- Exception handling mostly complies (context added and traced; general exceptions caught at orchestration boundaries).
- A few blocking-on-async call sites risk deadlocks in non-default sync contexts.
- Minor duplication and clarity issues around help routing.
- Pipeline uses hardcoded delimiter in two places (`CommandController.PIPELINE_CHAR` vs literal in `PipelineExecutor`).
- `CommandRegistry` uses a non-thread-safe `Dictionary` (registration likely at startup; worth documenting).
- `Xcaciv.Command.csproj` has metadata typos/misuse on `ProjectReference` entries that don’t affect build but reduce clarity and could break packaging intent.

Refactoring/Hardening Plan

Phase 1: Packaging and build clarity (low risk)
1. Fix `Xcaciv.Command.csproj` metadata issues:
   - Remove invalid `IncludeAssets` on `ProjectReference` and misspelled `Packge` metadata. Rely on existing `CopyProjectReferencesToPackage` target for packaging outputs.
   - No behavior change; improves MSBuild correctness/readability.
2. Add XML comment on `TargetsForTfmSpecificBuildOutput` purpose to aid future maintainers.

Phase 2: Async safety and help flow (low risk)
1. `CommandController.GetHelp`:
   - Keep signature (interface constraint) but avoid `Task.Run(...).GetAwaiter().GetResult()` if feasible by factoring shared async path and using a dedicated helper that blocks minimally. Add an XML doc warning of sync-blocking and recommend async `Run --HELP` usage.
   - Maintain behavior; reduce deadlock risk and clarify usage.
2. `CommandFactory.CreateCommand`:
   - Replace synchronous `.GetAwaiter().GetResult()` on `SetParameters` with a micro-optimization that avoids potential context capture (e.g., `ConfigureAwait(false)` already used; document that SetParameters must be fast/non-blocking). Add a guard comment to highlight this contract.
3. `CommandExecutor.ExecuteAsync` help handling:
   - Consolidate duplicated `--HELP` first-arg checks into a single branch that decides between one-line help (for roots) vs full help. Improves readability without changing behavior.

Phase 3: Pipeline consistency and diagnostics (low risk)
1. Introduce a shared delimiter definition:
   - Add `CommandSyntax` static class in `Xcaciv.Command.Interface` with `public const char PipelineDelimiter = '|'`.
   - Use it in both `CommandController` and `PipelineExecutor` to avoid divergence.
2. Add trace diagnostics in `PipelineExecutor` for each stage start/finish to increase observability (uses existing `IIoContext.AddTraceMessage`).

Phase 4: Robustness, clarity, and docs (very low risk)
1. `CommandRegistry`:
   - Document non-thread-safe nature; clarify expected usage (registration during startup only), or add a simple lock around modifications if multi-threaded registration is expected.
2. Security/telemetry notes:
   - In `CommandExecutor` audit logging, ensure parameters are logged responsibly. Add docstring note: commands must not place secrets in plain parameters; if needed, mask before logging.
3. Unit test additions (if coverage missing):
   - Help routing scenarios (root vs leaf with `--HELP`).
   - Pipeline execution with backpressure modes.
   - Loader behavior when no directories configured (throws `NoPluginsFoundException`).

Out of Scope / Deferred (needs human review)
- Changing interface shapes (e.g., making `GetHelp` async) — would be a breaking change.
- Deeper changes to assembly loading policy or widening trust boundaries.

Proposed File Changes
- src/Xcaciv.Command/Xcaciv.Command.csproj
  - Fix `ProjectReference` metadata typos/misuse; add brief comments.
- src/Xcaciv.Command/CommandExecutor.cs
  - Consolidate help-argument branching; minor readability improvements only.
- src/Xcaciv.Command/CommandController.cs
  - Improve `GetHelp` sync path doc and structure to reduce risk of sync deadlocks.
- src/Xcaciv.Command/CommandFactory.cs
  - Add contract comment about `SetParameters` sync usage; ensure continued `ConfigureAwait(false)`.
- src/Xcaciv.Command/PipelineExecutor.cs
  - Use shared pipeline delimiter constant; add start/finish trace for each stage.
- src/Xcaciv.Command.Interface/CommandSyntax.cs (new)
  - Host `PipelineDelimiter` constant.

Risk & Validation
- All changes are non-breaking and focused on clarity/safety.
- Build validation with `dotnet build` (Debug/Release).
- Run tests: `dotnet test` across all test projects.
- Manual smoke checks: default commands, `--HELP` for root/leaf, a simple pipeline like `say a | say b`.

Task Checklist
- [x] Phase 1: csproj cleanup
- [x] Phase 2: async/help flow simplification
- [x] Phase 3: pipeline constant + diagnostics
- [x] Phase 4: docs/comments + optional lock or docs in registry
- [x] Tests pass in CI and locally

Notes for Future
- Consider adding cancellation support in public APIs.
- Evaluate masking strategy for sensitive parameters in audit logs.

---
Progress Update (v1.6.0)
- Applied Phase 1: Cleaned `ProjectReference` metadata in `src/Xcaciv.Command/Xcaciv.Command.csproj` and added maintainer comments.
- Applied Phase 2: Consolidated `--HELP` handling in `src/Xcaciv.Command/CommandExecutor.cs`; clarified sync help path in `CommandController` remarks; documented `SetParameters` sync contract in `CommandFactory`.
- Applied Phase 3: Introduced `src/Xcaciv.Command.Interface/CommandSyntax.cs` and updated `CommandController` and `PipelineExecutor` to use the shared delimiter; added per-stage start/finish trace in `PipelineExecutor`.
- Applied Phase 4: Documented `CommandRegistry` concurrency expectations; retained audit logging semantics and added notes.
- Validation: Build succeeded. Test summary — total: 150, failed: 0, succeeded: 150, skipped: 0, duration: ~2.6s.
- Build script: Updated `build.ps1` to resolve repository root robustly, auto-discover the solution, and fall back to project-only build when needed. Verified pack works with .NET 8 and with `-UseNet10` enabling .NET 10 multi-targeting; packages saved under `artifacts/packages`.
