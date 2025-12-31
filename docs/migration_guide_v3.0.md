# Migration Guide: Upgrading to Xcaciv.Command v3.0

This guide helps you migrate from Xcaciv.Command v2.x to v3.0.

---

## Overview

Version 3.0 is a **breaking change release** that removes the deprecated APIs that were marked for removal in v2.0. This is a clean-up release to streamline the API surface and enforce best practices.

### What's Removed

| API | Status | Replacement |
|-----|--------|-------------|
| `EnableDefaultCommands()` | ? REMOVED | Use `RegisterBuiltInCommands()` |
| `GetHelp()` | ? REMOVED | Use `GetHelpAsync()` |

---

## Breaking Changes

### 1. EnableDefaultCommands() - REMOVED

**Status:** ? Removed in v3.0

**Old Code (v2.x):**
```csharp
var controller = new CommandController();
controller.EnableDefaultCommands(); // ? NO LONGER EXISTS
```

**New Code (v3.0):**
```csharp
var controller = new CommandController();
controller.RegisterBuiltInCommands(); // ? USE THIS
```

**Why:** "Register" more accurately describes the action (adding commands to a registry) versus the ambiguous "Enable".

---

### 2. GetHelp() - REMOVED

**Status:** ? Removed in v3.0

**Old Code (v2.x):**
```csharp
controller.GetHelp("", ioContext, env); // ? NO LONGER EXISTS
```

**New Code (v3.0):**
```csharp
await controller.GetHelpAsync("", ioContext, env); // ? USE THIS

// With cancellation token
await controller.GetHelpAsync("", ioContext, env, cancellationToken);
```

**Why:** Async pattern prevents blocking and follows .NET best practices (TAP - Task-based Asynchronous Pattern).

---

## Quick Migration Steps

### Step 1: Update Package References

Update your `.csproj` files:

```xml
<PackageReference Include="Xcaciv.Command" Version="3.0.0" />
<PackageReference Include="Xcaciv.Command.Core" Version="3.0.0" />
<PackageReference Include="Xcaciv.Command.Interface" Version="3.0.0" />
<PackageReference Include="Xcaciv.Command.FileLoader" Version="3.0.0" />
<PackageReference Include="Xcaciv.Command.DependencyInjection" Version="3.0.0" />
<PackageReference Include="Xcaciv.Command.Extensions.Commandline" Version="3.0.0" />
```

### Step 2: Build and Fix Compilation Errors

```powershell
dotnet build
```

You'll see compilation errors for removed methods. Use the table above to find replacements.

### Step 3: Find and Replace

Use your IDE's find-and-replace:

1. **Find:** `EnableDefaultCommands()`  
   **Replace with:** `RegisterBuiltInCommands()`

2. **Find:** `GetHelp(`  
   **Replace with:** `await GetHelpAsync(`  
   ?? Make sure to add `await` and ensure the calling method is `async`

### Step 4: Fix Async Context

If you weren't already using async/await, you'll need to update method signatures:

```csharp
// OLD (v2.x)
public void ShowHelp()
{
    controller.GetHelp("", ioContext, env); // ?
}

// NEW (v3.0)
public async Task ShowHelp()
{
    await controller.GetHelpAsync("", ioContext, env); // ?
}
```

### Step 5: Test Thoroughly

```powershell
dotnet test
```

Verify that:
- All commands register successfully
- Help system works correctly
- All tests pass
- No runtime errors

---

## Common Migration Scenarios

### Scenario 1: Console Application

**Before (v2.x):**
```csharp
class Program
{
    static void Main(string[] args)
    {
        var controller = new CommandController();
        controller.EnableDefaultCommands();
        
        controller.GetHelp("", ioContext, env);
        
        controller.Run("Say Hello", ioContext, env).Wait();
    }
}
```

**After (v3.0):**
```csharp
class Program
{
    static async Task Main(string[] args)
    {
        var controller = new CommandController();
        controller.RegisterBuiltInCommands();
        
        await controller.GetHelpAsync("", ioContext, env);
        
        await controller.Run("Say Hello", ioContext, env);
    }
}
```

### Scenario 2: ASP.NET Core Integration

**Before (v2.x):**
```csharp
public class CommandService
{
    private readonly ICommandController _controller;
    
    public CommandService(ICommandController controller)
    {
        _controller = controller;
        _controller.EnableDefaultCommands();
    }
    
    public void ShowHelp()
    {
        _controller.GetHelp("", ioContext, env);
    }
}
```

**After (v3.0):**
```csharp
public class CommandService
{
    private readonly ICommandController _controller;
    
    public CommandService(ICommandController controller)
    {
        _controller = controller;
        _controller.RegisterBuiltInCommands();
    }
    
    public async Task ShowHelpAsync()
    {
        await _controller.GetHelpAsync("", ioContext, env);
    }
}
```

### Scenario 3: Unit Tests

**Before (v2.x):**
```csharp
[Fact]
public void Test_Help_Command()
{
    var controller = new CommandController();
    controller.EnableDefaultCommands();
    
    controller.GetHelp("", ioContext, env);
    
    Assert.NotEmpty(ioContext.GetOutput());
}
```

**After (v3.0):**
```csharp
[Fact]
public async Task Test_Help_Command()
{
    var controller = new CommandController();
    controller.RegisterBuiltInCommands();
    
    await controller.GetHelpAsync("", ioContext, env);
    
    Assert.NotEmpty(ioContext.GetOutput());
}
```

---

## Troubleshooting

### Error: "EnableDefaultCommands does not exist"

**Solution:** Replace with `RegisterBuiltInCommands()`

```csharp
// ? ERROR
controller.EnableDefaultCommands();

// ? FIX
controller.RegisterBuiltInCommands();
```

### Error: "GetHelp does not exist"

**Solution:** Replace with `await GetHelpAsync()`

```csharp
// ? ERROR
controller.GetHelp("", ioContext, env);

// ? FIX
await controller.GetHelpAsync("", ioContext, env);
```

### Error: "Cannot await void method"

**Solution:** Change method signature to `async Task`

```csharp
// ? ERROR
public void MyMethod()
{
    await controller.GetHelpAsync("", ioContext, env);
}

// ? FIX
public async Task MyMethod()
{
    await controller.GetHelpAsync("", ioContext, env);
}
```

---

## Benefits of v3.0

? **Cleaner API**: Removed legacy methods  
? **Best Practices**: Full async/await support  
? **Better Naming**: More descriptive method names  
? **Easier Maintenance**: Smaller API surface  
? **Future-Proof**: Aligns with modern .NET patterns  

---

## Rollback Plan

If you encounter issues and need to stay on v2.x temporarily:

```xml
<!-- Rollback to v2.1.3 -->
<PackageReference Include="Xcaciv.Command" Version="2.1.3" />
```

**Note:** v2.x will continue to receive security patches until end of 2026.

---

## Version Support Policy

| Version | Status | Support Until |
|---------|--------|---------------|
| v1.x | End of Life | Ended 2025 |
| v2.x | Maintenance | End of 2026 |
| v3.x | **Current** | Active |
| v4.x | Planned | TBD |

---

## Need Help?

- **Documentation:** See [v3.0 Release Notes](../CHANGELOG.md#300)
- **Previous Migration:** See [Migration Guide v2.0](migration_guide_v2.0.md)
- **Security:** See [SECURITY.md](../SECURITY.md)
- **Issues:** Report at https://github.com/Xcaciv/Xcaciv.Command/issues
- **Discussions:** https://github.com/Xcaciv/Xcaciv.Command/discussions

---

**Last Updated:** January 2025  
**Applies To:** Xcaciv.Command v3.0.0+
