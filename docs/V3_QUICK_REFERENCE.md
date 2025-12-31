# V3.0 Breaking Changes - Quick Reference

## ?? Quick Reference Card

Print this or keep it handy during migration!

---

## Removed APIs

### 1. EnableDefaultCommands()

```csharp
// ? REMOVED in v3.0
controller.EnableDefaultCommands();

// ? USE THIS
controller.RegisterBuiltInCommands();
```

**One-line fix:** Find and replace `EnableDefaultCommands()` with `RegisterBuiltInCommands()`

---

### 2. GetHelp()

```csharp
// ? REMOVED in v3.0
controller.GetHelp("", ioContext, env);

// ? USE THIS
await controller.GetHelpAsync("", ioContext, env);
```

**Steps to fix:**
1. Add `await` before the call
2. Change method name to `GetHelpAsync`
3. Make calling method `async Task`

---

## Common Patterns

### Pattern 1: Controller Initialization

```csharp
// ? OLD
var controller = new CommandController();
controller.EnableDefaultCommands();

// ? NEW
var controller = new CommandController();
controller.RegisterBuiltInCommands();
```

### Pattern 2: Showing Help

```csharp
// ? OLD
public void ShowHelp()
{
    controller.GetHelp("", io, env);
}

// ? NEW
public async Task ShowHelpAsync()
{
    await controller.GetHelpAsync("", io, env);
}
```

### Pattern 3: Main Method

```csharp
// ? OLD
static void Main(string[] args)
{
    var controller = new CommandController();
    controller.EnableDefaultCommands();
    controller.GetHelp("", io, env);
}

// ? NEW
static async Task Main(string[] args)
{
    var controller = new CommandController();
    controller.RegisterBuiltInCommands();
    await controller.GetHelpAsync("", io, env);
}
```

---

## Find & Replace

Use these in your IDE:

1. **Find:** `EnableDefaultCommands()`  
   **Replace:** `RegisterBuiltInCommands()`

2. **Find:** `.GetHelp(`  
   **Replace:** `await .GetHelpAsync(`  
   ?? Then fix method signatures to be `async Task`

---

## Compiler Errors

### Error: 'ICommandController' does not contain a definition for 'EnableDefaultCommands'

**Fix:** Use `RegisterBuiltInCommands()` instead

### Error: 'ICommandController' does not contain a definition for 'GetHelp'

**Fix:** Use `await GetHelpAsync()` instead (and make method async)

### Error: The 'await' operator can only be used within an async method

**Fix:** Change method signature from `void MethodName()` to `async Task MethodName()`

---

## Migration Time Estimates

| Project Size | Estimated Time |
|--------------|----------------|
| Small (1-10 files) | 5 minutes |
| Medium (11-50 files) | 15 minutes |
| Large (50+ files) | 30-60 minutes |

---

## Quick Test

After migration, verify:

```powershell
# 1. Build succeeds
dotnet build

# 2. Tests pass
dotnet test

# 3. Application runs
dotnet run
```

---

## Rollback

If you need to revert:

```xml
<!-- Change this in your .csproj -->
<PackageReference Include="Xcaciv.Command" Version="3.0.0" />

<!-- To this -->
<PackageReference Include="Xcaciv.Command" Version="2.1.3" />
```

Then restore and rebuild:

```powershell
dotnet restore
dotnet build
```

---

## Need Help?

- ?? **Full Migration Guide:** `docs/migration_guide_v3.0.md`
- ?? **Release Notes:** `CHANGELOG.md`
- ?? **Issues:** https://github.com/Xcaciv/Xcaciv.Command/issues
- ?? **Discussions:** https://github.com/Xcaciv/Xcaciv.Command/discussions

---

## Version Info

**Current Version:** 3.0.0  
**Previous Version:** 2.1.3  
**Support for v2.x:** Until end of 2026  
**Breaking Changes:** 2 APIs removed  

---

**Last Updated:** January 2025
