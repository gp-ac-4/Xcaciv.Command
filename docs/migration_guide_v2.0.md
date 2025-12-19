# Migration Guide: Upgrading to Xcaciv.Command v2.0

This guide helps you migrate from Xcaciv.Command v1.x to v2.0.

---

## Breaking Changes Summary

v2.0 introduces intentional breaking changes to improve security, clarity, and async patterns. However, **backward compatibility is maintained** through deprecated methods marked for removal in v3.0.

### Quick Migration Checklist

- [ ] Update API calls to use new method names
- [ ] Replace sync `GetHelp()` with async `GetHelpAsync()`
- [ ] Replace `EnableDefaultCommands()` with `RegisterBuiltInCommands()`
- [ ] Configure security policies if using plugins
- [ ] Review pipeline configuration for new timeout options
- [ ] Update tests to eliminate deprecation warnings
- [ ] Test cancellation token propagation

---

## API Changes

### 1. RegisterBuiltInCommands() [BREAKING]

**Old (deprecated in v2.0, removed in v3.0):**
```csharp
var controller = new CommandController();
controller.EnableDefaultCommands();
```

**New (v2.0+):**
```csharp
var controller = new CommandController();
controller.RegisterBuiltInCommands();
```

**Rationale:** "Register" more clearly indicates the action (adding commands to a registry) vs. "Enable" (which is ambiguous).

---

### 2. GetHelpAsync() [BREAKING]

**Old (deprecated in v2.0, removed in v3.0):**
```csharp
controller.GetHelp(string.Empty, ioContext, env);
```

**New (v2.0+):**
```csharp
await controller.GetHelpAsync(string.Empty, ioContext, env);
// Or with cancellation:
await controller.GetHelpAsync(string.Empty, ioContext, env, cancellationToken);
```

**Rationale:** Async pattern avoids blocking and potential deadlocks. Follows Task-based Asynchronous Pattern (TAP).

---

### 3. Security Configuration [NEW]

**Default security policy changed from `AssemblySecurityPolicy.Default` to `AssemblySecurityPolicy.Strict`.**

#### Option A: Accept stricter defaults (recommended)
```csharp
var controller = new CommandController();
// Strict security is now default - no action needed
```

#### Option B: Opt back to permissive mode (for legacy plugins)
```csharp
var factory = new CommandFactory();
factory.SetSecurityConfiguration(new AssemblySecurityConfiguration
{
    SecurityPolicy = AssemblySecurityPolicy.Default,
    EnforceBasePathRestriction = false,
    AllowReflectionEmit = true
});

var controller = new CommandController(
    commandRegistry,
    commandExecutor,
    commandLoader,
    factory // Use configured factory
);
```

**What changed:**
- `SecurityPolicy`: `Default` (permissive) → `Strict` (restrictive)
- `EnforceBasePathRestriction`: Always enabled
- `AllowReflectionEmit`: Disabled by default

**Impact:** Plugins using dynamic code generation or accessing outside their directory will fail to load. Configure security policies explicitly for legacy plugins.

---

### 4. Pipeline Configuration [NEW]

New pipeline timeout and resource limit options are available:

```csharp
var controller = new CommandController();
controller.PipelineExecutor.Configuration = new PipelineConfiguration
{
    MaxChannelQueueSize = 10_000,
    BackpressureMode = PipelineBackpressureMode.Block,
    ExecutionTimeoutSeconds = 30,      // NEW: Overall pipeline timeout
    StageTimeoutSeconds = 10,          // NEW: Per-stage timeout
    MaxStageOutputBytes = 10_485_760,  // NEW: 10MB limit per stage
    MaxStageOutputItems = 100_000      // NEW: Item count limit per stage
};
```

**Default behavior:** No timeouts or limits (0 = unlimited). Opt-in for DoS protection.

---

### 5. Cancellation Token Propagation [NEW]

Cancellation tokens now propagate through the entire execution chain:

```csharp
using var cts = new CancellationTokenSource();

// Pass cancellation token to Run
await controller.Run(commandLine, ioContext, env, cts.Token);

// Cancel from another thread
cts.Cancel();
```

**What changed:**
- `Run()` overload with `CancellationToken` parameter added
- Cancellation propagates to: Pipeline → Executor → Individual commands
- Commands should respect cancellation in long-running operations

---

### 6. Help System Centralization [NEW]

Help generation is now centralized in `IHelpService`:

```csharp
// Automatic - no code changes needed
// Help is generated from command attributes and metadata
```

**What changed:**
- Commands no longer need custom `Help()` implementations
- Help is generated from `CommandRegisterAttribute` and parameter attributes
- Override `Help()` only for custom formatting needs

---

### 7. Dependency Injection Support [NEW]

Optional DI support via separate adapter package:

```csharp
// Install: Xcaciv.Command.DependencyInjection
services.AddXcacivCommand(options =>
{
    options.EnableDefaultCommands = true;
    options.PackageDirectories = new[] { @"C:\Plugins" };
});

// Resolve from DI container
var controller = serviceProvider.GetRequiredService<ICommandController>();
```

**What changed:**
- DI support moved to separate `Xcaciv.Command.DependencyInjection` package
- Core framework remains DI-agnostic
- `CommandControllerFactory` added for non-DI scenarios

---

## Deprecation Timeline

| API | Status in v2.0 | Removal in v3.0 |
|-----|----------------|-----------------|
| `EnableDefaultCommands()` | Deprecated (warning) | ❌ Removed |
| `GetHelp()` | Deprecated (warning) | ❌ Removed |
| `AssemblySecurityPolicy.Default` | Discouraged | Available but not recommended |

---

## Step-by-Step Migration Process

### Step 1: Update Package References

```xml
<PackageReference Include="Xcaciv.Command" Version="2.0.0" />
<PackageReference Include="Xcaciv.Loader" Version="2.0.1" />
```

### Step 2: Build and Review Warnings

```powershell
dotnet build
```

Look for:
- `CS0618` warnings for deprecated APIs
- Security policy warnings for plugins

### Step 3: Replace Deprecated Methods

Use find-and-replace:
- `EnableDefaultCommands()` → `RegisterBuiltInCommands()`
- `GetHelp(` → `GetHelpAsync(`

### Step 4: Add Async/Await

Update help calls to async:
```csharp
// Before
controller.GetHelp(cmd, io, env);

// After
await controller.GetHelpAsync(cmd, io, env);
```

### Step 5: Configure Security (if needed)

If plugins fail to load, configure security:
```csharp
var factory = new CommandFactory();
factory.SetSecurityConfiguration(new AssemblySecurityConfiguration
{
    SecurityPolicy = AssemblySecurityPolicy.Default
});
```

### Step 6: Test Thoroughly

```powershell
dotnet test
```

Verify:
- All commands load successfully
- Plugin discovery works
- Pipeline execution functions correctly
- Cancellation tokens are respected

---

## Common Migration Issues

### Issue 1: Plugin Load Failures

**Symptom:** `SecurityException` or plugins not discovered

**Solution:** Configure permissive security policy:
```csharp
factory.SetSecurityConfiguration(new AssemblySecurityConfiguration
{
    SecurityPolicy = AssemblySecurityPolicy.Default,
    EnforceBasePathRestriction = false
});
```

### Issue 2: Async Context Deadlocks

**Symptom:** Application hangs when calling help

**Solution:** Use `GetHelpAsync()` instead of `GetHelp()`:
```csharp
await controller.GetHelpAsync(cmd, io, env);
```

### Issue 3: Compiler Warnings (CS0618)

**Symptom:** Deprecation warnings during build

**Solution:** Replace with new API names as documented above

---

## Testing Your Migration

### Automated Testing

```csharp
[Fact]
public async Task Migration_RegisterBuiltInCommands_Works()
{
    var controller = new CommandController();
    controller.RegisterBuiltInCommands();  // New API
    
    var io = new MemoryIoContext();
    var env = new EnvironmentContext();
    
    await controller.Run("Say Hello", io, env);
    
    Assert.Contains("Hello", io.GetOutput());
}

[Fact]
public async Task Migration_GetHelpAsync_Works()
{
    var controller = new CommandController();
    controller.RegisterBuiltInCommands();
    
    var io = new MemoryIoContext();
    var env = new EnvironmentContext();
    
    await controller.GetHelpAsync(string.Empty, io, env);  // New async API
    
    Assert.NotEmpty(io.GetOutput());
}
```

### Manual Testing

1. Run your application with v2.0
2. Execute all command types (built-in, plugins, pipelines)
3. Test help command: `--HELP`, `-?`, `/?`
4. Test cancellation (Ctrl+C during long operations)
5. Verify plugin loading from directories

---

## Rollback Plan

If migration issues are blocking:

### Option 1: Stay on v1.x
```xml
<PackageReference Include="Xcaciv.Command" Version="1.6.0" />
```

### Option 2: Use v2.0 with deprecated APIs
v2.0 maintains backward compatibility - deprecated methods still work with warnings.

### Option 3: Gradual Migration
1. Update to v2.0
2. Keep deprecated methods temporarily
3. Migrate incrementally
4. Remove deprecated usage before v3.0

---

## Benefits of v2.0

✅ **Better Security:** Stricter default policies prevent malicious plugins  
✅ **Async Patterns:** Proper Task-based async avoids deadlocks  
✅ **Cancellation Support:** Graceful shutdown and timeout handling  
✅ **Pipeline Hardening:** Formal grammar parser, timeouts, resource limits  
✅ **DI Support:** Optional integration with Microsoft.Extensions.DependencyInjection  
✅ **Clearer APIs:** Better naming (Register vs. Enable, Async suffix)  
✅ **Thread Safety:** ConcurrentDictionary-based registry  
✅ **Centralized Help:** Consistent help generation from attributes  

---

## Need Help?

- **Documentation:** See [maintenance_v2.0_execution_plan.md](maintenance_v2.0_execution_plan.md)
- **Security:** See [SECURITY.md](../SECURITY.md)
- **Issues:** Report at https://github.com/Xcaciv/Xcaciv.Command/issues
- **Discussions:** https://github.com/Xcaciv/Xcaciv.Command/discussions

---

## Version Support

| Version | Status | Support Until |
|---------|--------|---------------|
| v1.6.x | Maintenance | End of 2026 |
| v2.0.x | Current | Active |
| v3.0.x | Planned | TBD |
