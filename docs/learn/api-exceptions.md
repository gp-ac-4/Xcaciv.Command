# API Reference: Exceptions

Complete reference for exception types in Xcaciv.Command framework.

## XcCommandException

Base exception class for framework-specific exceptions.

```csharp
public class XcCommandException : Exception
{
    public XcCommandException(string message);
    public XcCommandException(string message, Exception innerException);
}
```

### Remarks

All framework exceptions inherit from this class. Catch this exception to handle any framework-specific error.

### Example

```csharp
try
{
    await controller.Run("UNKNOWN", ioContext, env);
}
catch (XcCommandException ex)
{
    Console.WriteLine($"Framework error: {ex.Message}");
}
```

---

## NoPackageDirectoryFoundException

Thrown when a required plugin directory is not found.

```csharp
public class NoPackageDirectoryFoundException : XcCommandException
{
    public NoPackageDirectoryFoundException(string message);
}
```

### When Thrown

- Plugin directory path doesn't exist
- Directory is not readable
- Security policy blocks directory access

### Example

```csharp
try
{
    controller.AddPackageDirectory("/nonexistent/path");
    controller.LoadCommands();
}
catch (NoPackageDirectoryFoundException ex)
{
    Console.WriteLine($"Plugin directory error: {ex.Message}");
}
```

---

## NoPluginFilesFoundException

Thrown when no plugin files are found in a package directory.

```csharp
public class NoPluginFilesFoundException : XcCommandException
{
    public NoPluginFilesFoundException(string message);
}
```

### When Thrown

- Directory exists but contains no `.dll` files
- Subdirectory (e.g., `bin/`) doesn't exist

### Example

```csharp
try
{
    controller.AddPackageDirectory("/plugins");
    controller.LoadCommands();
}
catch (NoPluginFilesFoundException ex)
{
    Console.WriteLine($"No plugins found: {ex.Message}");
}
```

---

## NoPluginsFoundException

Thrown when plugin files are found but don't contain any valid commands.

```csharp
public class NoPluginsFoundException : XcCommandException
{
    public NoPluginsFoundException(string message);
}
```

### When Thrown

- Plugin DLLs don't implement `ICommandDelegate`
- No classes decorated with `CommandRegisterAttribute`
- Reflection fails to analyze plugin types

### Example

```csharp
try
{
    controller.AddPackageDirectory("/plugins");
    controller.LoadCommands();
}
catch (NoPluginsFoundException ex)
{
    Console.WriteLine($"No valid commands found: {ex.Message}");
}
```

---

## InValidConfigurationException

Thrown when configuration is invalid.

```csharp
public class InValidConfigurationException : XcCommandException
{
    public InValidConfigurationException(string message);
    public InValidConfigurationException(string message, Exception innerException);
}
```

### When Thrown

- Invalid command registration
- Parameter configuration is malformed
- Attribute metadata is inconsistent
- Pipeline configuration is invalid

### Example

```csharp
try
{
    var command = new InvalidCommand(); // Missing CommandRegisterAttribute
    controller.AddCommand("pkg", command);
}
catch (InValidConfigurationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
```

---

## Exception Handling Best Practices

### UI Layer Only

Only the UI/presentation layer should catch general exceptions:

```csharp
// Good: UI layer
public async Task RunCommandAsync(string commandLine)
{
    try
    {
        await controller.Run(commandLine, ioContext, env);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        logger.Log(ex);
    }
}
```

### Inner Layers

Inner layers should only catch specific exceptions they can handle:

```csharp
// Good: Command layer
public override string HandleExecution(string[] parameters, IEnvironmentContext env)
{
    try
    {
        var file = File.ReadAllText(filename);
        return file;
    }
    catch (FileNotFoundException ex)
    {
        throw new InvalidOperationException($"Config file not found: {filename}", ex);
    }
}
```

---

## Common Error Scenarios

### Command Not Found

**Error:** "Command 'UNKNOWN' is not registered"

**Cause:** Command name doesn't match any registered command

**Solution:**
```csharp
// Check available commands
controller.GetHelp("", ioContext, env);

// Verify command is loaded
controller.AddPackageDirectory("/plugins");
controller.LoadCommands();
```

### Invalid Parameters

**Error:** "Required parameter 'name' not provided"

**Cause:** Required parameter missing from command line

**Solution:**
```csharp
// Show help for command
controller.GetHelp("MYCOMMAND", ioContext, env);

// Provide required parameters
await controller.Run("MYCOMMAND value1 value2", ioContext, env);
```

### Plugin Loading Fails

**Error:** "No plugin files found in /plugins/bin"

**Cause:** Plugin DLLs not in expected location

**Solution:**
```
/plugins/
  MyPlugin/
    bin/
      MyPlugin.dll          <- DLL must be here
      MyPlugin.deps.json
```

### Security Exception

**Error:** "Security violation loading type from ..."

**Cause:** Plugin path outside restricted directory

**Solution:**
```csharp
// Ensure plugin is in verified directory
var verified = new VerifiedSourceDirectories(new FileSystem());
var controller = new CommandController(verified);
```

### Timeout in Pipeline

**Error:** "Pipeline stage timeout exceeded"

**Cause:** Command didn't complete within timeout

**Solution:**
```csharp
// Increase timeout or make command faster
var executor = new PipelineExecutor();
executor.Configuration.StageTimeoutSeconds = 60; // Increase from default

// Or optimize command implementation
```

---

## Debugging Tips

### Enable Audit Logging

```csharp
public class DebugAuditLogger : IAuditLogger
{
    public async Task LogAsync(string entry)
    {
        Debug.WriteLine($"[AUDIT] {entry}");
        await Task.CompletedTask;
    }
}

controller.SetAuditLogger(new DebugAuditLogger());
```

### Check Trace Messages

```csharp
var ioContext = new MemoryIoContext();

await controller.Run("MYCOMMAND", ioContext, env);

// Access trace messages (depends on IIoContext implementation)
// Some implementations provide trace access
```

### Validate Command Attributes

```csharp
var type = typeof(MyCommand);
var attr = (CommandRegisterAttribute)Attribute.GetCustomAttribute(
    type, 
    typeof(CommandRegisterAttribute));

if (attr == null)
{
    Console.WriteLine("Missing CommandRegisterAttribute");
}
```

### Test Plugin Loading

```csharp
var crawler = new Crawler();
var commands = crawler.Crawl("/plugins");

foreach (var cmd in commands)
{
    Console.WriteLine($"Found: {cmd.BaseCommand} ({cmd.FullTypeName})");
}
```

---

## See Also

- [API Core Types](api-core.md)
- [API Interfaces](api-interfaces.md)
- [Getting Started: Create a Command](getting-started-create-command.md)
