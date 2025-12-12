# Migration Guide: Xcaciv.Command 1.5.18 (Xcaciv.Loader 2.0.1)

## Overview

Xcaciv.Command 1.5.18 includes Xcaciv.Loader 2.0.1, which introduces instance-based security configuration. This guide explains what changed and how it affects your usage.

## What Changed?

### For Command Users (No Action Required)

If you're **using** Xcaciv.Command in your application:

? **No code changes required**

The migration is internal to the framework. Your existing code will continue to work:

```csharp
var controller = new CommandController();
controller.AddPackageDirectory("plugins");
controller.LoadCommands();
await controller.Run("command args", ioContext, env);
```

### For Plugin Developers (Enhanced Security)

If you're **developing command plugins**:

? **No code changes required**

Your plugins will automatically benefit from:
- Enhanced security with directory-based restrictions
- Better error messages for security violations
- Independent security context per plugin

### For Advanced Users (Custom Loading)

If you're **directly using AssemblyContext** in custom code:

?? **Action may be required**

#### Before (Xcaciv.Loader 1.x)
```csharp
// This still works but is deprecated
AssemblyContext.SetStrictDirectoryRestriction(true);
var context = new AssemblyContext(pluginPath, basePathRestriction: "*");
```

#### After (Xcaciv.Loader 2.0.1)
```csharp
// New instance-based approach
var context = new AssemblyContext(
    pluginPath,
    basePathRestriction: Path.GetDirectoryName(pluginPath),
    securityPolicy: AssemblySecurityPolicy.Default);
```

## Benefits of the Migration

### 1. Better Security
- Plugins restricted to their actual directories (no more wildcard `*`)
- Each plugin has independent security boundaries
- Security violations are explicitly handled

### 2. Better Testability
- No global state affecting tests
- Tests can run in parallel safely
- Each test has isolated security context

### 3. Better Error Messages
- Security violations include detailed context
- Clear error messages showing which plugin and path failed
- Trace logging for debugging

## Testing Your Migration

### Run Your Existing Tests
```powershell
dotnet test YourProject.Tests
```

All existing tests should pass without changes.

### Verify Plugin Loading
```csharp
[Fact]
public void MyPlugin_LoadsSuccessfully()
{
    var controller = new CommandController();
    controller.AddPackageDirectory("path/to/my/plugin");
    
    // Should complete without exception
    controller.LoadCommands();
}
```

### Check Security Boundaries
```csharp
[Fact]
public async Task MyPlugin_ExecutesWithinSecurityBoundaries()
{
    var controller = new CommandController();
    controller.AddPackageDirectory("plugins");
    controller.LoadCommands();
    
    var io = new MemoryIoContext();
    var env = new EnvironmentContext();
    
    // Should execute successfully
    await controller.Run("mycommand", io, env);
    
    Assert.NotEmpty(io.Output);
}
```

## Troubleshooting

### SecurityException When Loading Plugins

**Symptom**: `SecurityException` or `InvalidOperationException` mentioning security violation

**Cause**: Plugin dependencies are in directories outside the allowed path

**Solution**: 
1. Check trace logs for detailed path information
2. Ensure all plugin dependencies are in the plugin directory
3. Verify the plugin directory structure is correct

### Plugins Not Loading

**Symptom**: Plugins are silently skipped

**Cause**: Security violations are now logged but don't throw by default during discovery

**Solution**:
1. Check trace logs in your IIoContext
2. Enable verbose logging: `ioContext.Verbose = true`
3. Review the Crawler trace output

### Performance Concerns

**Question**: Does instance-based security affect performance?

**Answer**: No. The security policy is set once per AssemblyContext. There's no measurable performance impact.

## Example: Complete Migration Test

```csharp
using Xunit;
using Xcaciv.Command;
using Xcaciv.Command.FileLoader;

public class MigrationVerificationTests
{
    [Fact]
    public async Task CompleteWorkflow_WorksAfterMigration()
    {
        // Arrange
        var controller = new CommandController(
            new Crawler(), 
            baseDirectory: Environment.CurrentDirectory);
        
        controller.AddPackageDirectory("plugins");
        controller.LoadCommands();
        controller.EnableDefaultCommands();
        
        var io = new MemoryIoContext();
        var env = new EnvironmentContext();
        
        // Act - Test built-in command
        await controller.Run("say hello", io, env);
        
        // Assert
        Assert.Contains("hello", io.Output);
        
        // Act - Test plugin command
        io = new MemoryIoContext();
        await controller.Run("myplugin test", io, env);
        
        // Assert
        Assert.NotEmpty(io.Output);
    }
}
```

## FAQ

**Q: Do I need to update my plugin code?**  
A: No, plugins built against previous versions work without changes.

**Q: Will old versions of Xcaciv.Command work with my code?**  
A: Yes, but we recommend upgrading to get security enhancements.

**Q: Can I still use wildcard (`*`) path restrictions?**  
A: No, wildcard restrictions have been replaced with directory-based security for safety.

**Q: Does this affect command execution performance?**  
A: No, there is no measurable performance impact.

**Q: How do I get more details about security violations?**  
A: Enable verbose logging on your IIoContext and check trace messages.

## Support

- **GitHub Issues**: https://github.com/Xcaciv/Xcaciv.Command/issues
- **Xcaciv.Loader Migration Guide**: https://github.com/Xcaciv/Xcaciv.Loader/blob/main/docs/MIGRATION-v1-to-v2.md

## Summary

? Most users require **no code changes**  
? Enhanced security automatically applied  
? Better error messages and logging  
? All tests should pass without modification  
? Contact us if you have issues
