# Maintenance v2.0 Execution Plan

## Risk-Ordered Implementation Strategy

This document outlines the phased execution plan for v2.0 maintenance, organized from lowest to highest risk. Each phase must compile and pass tests before proceeding to the next.

---

## Phase 1: Auditing and Tracing Enhancements (LOW RISK)

**Risk Level:** LOW - Additive features that don't break existing APIs

**Changes:**
- [ ] Standardize audit logs with structured events and masking strategies while allowing configurable parameter redaction
- [ ] Emit correlation IDs for commands and pipeline stages and include command package origin metadata
- [ ] Compile and run tests

**Rationale:** These are purely additive features. Existing code continues to work while new audit capabilities are introduced.

---

## Phase 2: Configuration and Dependency Injection (LOW RISK)

**Risk Level:** LOW - Adding optional DI support without breaking existing instantiation patterns

**Changes:**
- [x] Bind configuration via `IOptions` where appropriate and provide default wiring inside a startup module (moved to adapter)
- [x] Support external DI containers explicitly via a separate adapter project `Xcaciv.Command.DependencyInjection` and extension methods
- [x] Added `CommandControllerFactory` for non-DI usage
- [x] Compile and run tests

**Rationale:** DI support is opt-in. Existing direct instantiation patterns remain valid.

---

## Phase 3: Cancellation Token Propagation (MEDIUM RISK)

**Risk Level:** MEDIUM - API extensions with default parameters maintain backwards compatibility

**Changes:**
- [x] Extend controller `Run(..., CancellationToken)` and executor/pipeline overloads to accept `CancellationToken`
- [x] Thread the token through `Run` → `PipelineExecutor.ExecuteAsync` → `CommandExecutor.ExecuteAsync`
- [x] Compile and run tests

**Rationale:** Adding optional `CancellationToken` parameters with defaults preserves binary compatibility. Existing callers continue to work.

---

## Phase 4: Registry and Factory Lifecycle (MEDIUM RISK)

**Risk Level:** MEDIUM - Internal improvements with new async overloads

**Changes:**
- [ ] Make `CommandRegistry` thread-safe (internal implementation change)
- [ ] Add async `CreateCommandAsync(ICommandDescription, IIoContext)` with sync wrapper for legacy
- [ ] Compile and run tests

**Rationale:** Thread-safety is an internal improvement. Adding async factory methods while keeping sync wrappers maintains compatibility.

---

## Phase 5: Parameters and Help Consistency (MEDIUM RISK)

**Risk Level:** MEDIUM - Centralization of logic without breaking command implementations

**Changes:**
- [ ] Normalize `--HELP` handling in the controller rather than in individual executors
- [ ] Introduce `IHelpService` to centralize help building (commands supply metadata)
- [ ] Compile and run tests

**Rationale:** Centralizing help logic reduces duplication. Commands adapt to new patterns but existing help output remains functional.

---

## Phase 6: Pipeline Hardening (MEDIUM-HIGH RISK)

**Risk Level:** MEDIUM-HIGH - Significant refactoring with backwards compatibility layer

**Changes:**
- [ ] Move pipeline parsing to a dedicated parser with formal grammar (quoted args, escape delimiters)
- [ ] Add per-stage cancellation, timeouts, and resource bounding in `PipelineConfiguration`
- [ ] Support typed pipe payloads (JSON schema-validated)
- [ ] Compile and run tests

**Rationale:** New parser changes fundamental behavior. Must maintain backwards compatibility for existing pipeline syntax while adding new capabilities.

---

## Phase 7: Security Alignment with Xcaciv.Loader 2.x (HIGH RISK)

**Risk Level:** HIGH - Changes security defaults and plugin loading behavior

**Changes:**
- [ ] Use stricter `AssemblySecurityPolicy` defaults with explicit allowlists
- [ ] Enforce `basePathRestriction` and disallow reflection emit by default
- [ ] Require signed plugin assemblies or trusted source metadata (with verification hooks in `CommandLoader`)
- [ ] Prefer safe activation APIs from Loader 2.x and propagate security exceptions with context
- [ ] Compile and run tests

**Rationale:** Stricter security defaults may break existing plugins. Requires careful migration guidance and opt-out mechanisms during transition.

---

## Phase 8: Breaking API Changes (HIGH RISK)

**Risk Level:** HIGH - Intentional breaking changes requiring major version bump

**Changes:**
- [ ] Make `GetHelp` async: ship `Task GetHelpAsync(...)` and deprecate sync `GetHelp`
- [ ] Remove legacy overloads that imply synchronous execution
- [ ] Rename ambiguous members (e.g., `EnableDefaultCommands()` → `RegisterBuiltInCommands()`)
- [ ] Consolidate interfaces in `AbstractCommandParameter.cs` with clearer responsibilities and XML docs
- [ ] Compile and run tests

**Rationale:** These are intentional breaking changes. Must be the final phase to ensure all preparatory work is stable first.

---

## Testing Strategy

After each phase:
1. Run full test suite: `dotnet test Xcaciv.Command.sln --no-build`
2. Review test failures and determine if test needs update or code needs fix
3. Update tests only when they test obsolete behavior
4. Fix code when tests correctly identify regressions
5. Document any behavioral changes in CHANGELOG.md

## Rollback Plan

- Each phase is a separate commit
- Failed phase blocks progression to next phase
- Can rollback to previous phase commit if needed
- Mark corresponding checkboxes in maintenance_v2.0.md only after successful phase completion
