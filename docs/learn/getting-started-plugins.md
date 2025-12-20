# Build a Plugin Package

Learn how to create, package, and distribute command plugins for Xcaciv.Command.

## What is a Plugin Package?

A plugin package is a .NET class library containing command implementations that can be discovered and loaded at runtime by the Xcaciv.Command framework.

## Create a Plugin Project

### 1. Create Class Library

```bash
dotnet new classlib -n MyCommandPlugin
cd MyCommandPlugin
```

### 2. Add Framework References

```bash
dotnet add package Xcaciv.Command.Core
dotnet add package Xcaciv.Command.Interface
```

### 3. Create Command Class

```csharp
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace MyCommandPlugin.Commands
{
    [CommandRegister("MYECHO", "Echo input with transformation")]
    public class EchoCommand : AbstractCommand
    {
        [CommandParameterOrdered("text", "Text to echo")]
        public string Text { get; set; }

        [CommandFlag("uppercase", "Convert to uppercase")]
        public bool Uppercase { get; set; }

        public override string HandleExecution(string[] parameters, IEnvironmentContext env)
        {
            var text = Uppercase ? Text.ToUpper() : Text;
            return text;
        }

        public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env)
        {
            return Uppercase ? pipedChunk.ToUpper() : pipedChunk;
        }
    }
}
```

### 4. Build the Plugin

```bash
dotnet build -c Release
```

Output DLL will be in `bin/Release/net8.0/MyCommandPlugin.dll`

## Plugin Directory Structure

The Xcaciv.Command framework expects plugins in a specific directory structure:

```
/plugins/
  MyCommandPlugin/
    bin/
      MyCommandPlugin.dll
      MyCommandPlugin.deps.json
      (other dependencies)
```

When you call:
```csharp
controller.AddPackageDirectory("/plugins");
controller.LoadCommands(); // searches bin/ by default
```

The framework discovers and loads all `*.dll` files from the `bin/` subdirectory.

## Plugin Discovery Process

The `Crawler` class discovers plugins:

1. Searches specified directory recursively
2. Finds `.dll` files in `bin/` subdirectories
3. Uses reflection to find types implementing `ICommandDelegate`
4. Checks for `CommandRegisterAttribute` decoration
5. Creates `ICommandDescription` metadata for each command

```csharp
var crawler = new Crawler();
var commands = crawler.Crawl("/plugins");

foreach (var command in commands)
{
    Console.WriteLine($"Found: {command.BaseCommand}");
}
```

## Plugin Security

### Verified Source Directories

The framework restricts plugin loading to verified locations for security:

```csharp
// Only load from verified directories
var directories = new VerifiedSourceDirectories(new FileSystem());
var controller = new CommandController(directories);
```

### Assembly Context

Plugin assemblies are loaded with `AssemblyContext` using security restrictions:

```csharp
// Enforces basePathRestriction to prevent loading from unexpected locations
using var context = new AssemblyContext(
    assemblyPath,
    basePathRestriction: "/plugins",
    securityPolicy: AssemblySecurityPolicy.Strict);
```

### Dependency Isolation

Plugin dependencies are resolved within their own assembly context, preventing version conflicts with the host application.

## Plugin Configuration

### Environment-Modifying Commands

Commands that modify environment variables must be explicitly marked:

```csharp
[CommandRegister("SETENV", "Set environment variable")]
public class SetEnvCommand : AbstractCommand
{
    [CommandParameterOrdered("name", "Variable name")]
    public string Name { get; set; }

    [CommandParameterOrdered("value", "Variable value")]
    public string Value { get; set; }

    public override string HandleExecution(string[] parameters, IEnvironmentContext env)
    {
        env.SetVariable(Name, Value);
        return $"Set {Name}={Value}";
    }
}
```

When registering:
```csharp
controller.AddCommand("MyPlugin", typeof(SetEnvCommand), modifiesEnvironment: true);
```

Or discover automatically (framework reads attribute from plugin):
```csharp
controller.AddPackageDirectory("/plugins");
controller.LoadCommands();
```

## Sub-Commands (Command Hierarchy)

Create command hierarchies using `CommandRootAttribute`:

```csharp
[CommandRoot("DATABASE")]
[CommandRegister("CREATE", "Create a new database")]
public class DatabaseCreateCommand : AbstractCommand
{
    public override string HandleExecution(string[] parameters, IEnvironmentContext env)
    {
        return "Database created";
    }
}

[CommandRoot("DATABASE")]
[CommandRegister("DELETE", "Delete a database")]
public class DatabaseDeleteCommand : AbstractCommand
{
    public override string HandleExecution(string[] parameters, IEnvironmentContext env)
    {
        return "Database deleted";
    }
}
```

Usage:
```
DATABASE CREATE mydb
DATABASE DELETE mydb
```

## Packaging for Distribution

### NuGet Package

Create a `.nuspec` file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
  <metadata>
    <id>MyCommandPlugin</id>
    <version>1.0.0</version>
    <title>My Command Plugin</title>
    <authors>Your Name</authors>
    <description>Plugin adding custom commands to Xcaciv.Command</description>
    <dependencies>
      <dependency id="Xcaciv.Command.Core" version="1.0.0" />
      <dependency id="Xcaciv.Command.Interface" version="1.0.0" />
    </dependencies>
  </metadata>
  <files>
    <file src="bin/Release/net8.0/*.dll" target="lib/net8.0/" />
    <file src="bin/Release/net8.0/*.deps.json" target="lib/net8.0/" />
  </files>
</package>
```

Pack it:
```bash
nuget pack MyCommandPlugin.nuspec
```

### Direct Distribution

Package the entire plugin directory:

```bash
# Create plugin structure
mkdir -p MyPlugin/bin
cp bin/Release/net8.0/*.dll MyPlugin/bin/
cp bin/Release/net8.0/*.deps.json MyPlugin/bin/

# Create archive
tar -czf MyPlugin.tar.gz MyPlugin/
# or on Windows
Compress-Archive -Path MyPlugin -DestinationPath MyPlugin.zip
```

Distribute the archive and users extract to their plugins directory.

## Example: Complete Plugin

Here's a complete example plugin with multiple commands:

```csharp
// CalculatorPlugin.cs
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace CalculatorPlugin
{
    [CommandRegister("ADD", "Add two numbers")]
    public class AddCommand : AbstractCommand
    {
        [CommandParameterOrdered("a", "First number")]
        public int A { get; set; }

        [CommandParameterOrdered("b", "Second number")]
        public int B { get; set; }

        public override string HandleExecution(string[] parameters, IEnvironmentContext env)
        {
            return (A + B).ToString();
        }
    }

    [CommandRegister("SUBTRACT", "Subtract two numbers")]
    public class SubtractCommand : AbstractCommand
    {
        [CommandParameterOrdered("a", "First number")]
        public int A { get; set; }

        [CommandParameterOrdered("b", "Second number")]
        public int B { get; set; }

        public override string HandleExecution(string[] parameters, IEnvironmentContext env)
        {
            return (A - B).ToString();
        }
    }

    [CommandRegister("MULTIPLY", "Multiply two numbers")]
    public class MultiplyCommand : AbstractCommand
    {
        [CommandParameterOrdered("a", "First number")]
        public int A { get; set; }

        [CommandParameterOrdered("b", "Second number")]
        public int B { get; set; }

        public override string HandleExecution(string[] parameters, IEnvironmentContext env)
        {
            return (A * B).ToString();
        }
    }
}
```

Build and deploy:
```bash
dotnet build -c Release
# Output: bin/Release/net8.0/CalculatorPlugin.dll
```

Load and use:
```csharp
var controller = new CommandController();
controller.EnableDefaultCommands();
controller.AddPackageDirectory("./plugins");
controller.LoadCommands();

var io = new MemoryIoContext();
var env = new EnvironmentContext();

await controller.Run("ADD 5 3", io, env);
Console.WriteLine(io.GetOutput()); // "8"
```

## Troubleshooting

### Commands Not Loading

Check these items:

1. **DLL in correct location**: Verify `bin/PluginName/` directory structure
2. **Dependencies available**: Ensure all referenced assemblies are in the same directory
3. **Attribute present**: Confirm `CommandRegisterAttribute` is on the command class
4. **Interface implemented**: Verify class implements `ICommandDelegate` (via `AbstractCommand`)
5. **Correct framework version**: Ensure plugin targets same .NET version as host

### Security Errors

If you get `SecurityException`:

1. Verify plugin path is within `basePathRestriction`
2. Ensure `VerifiedSourceDirectories` approves the path
3. Check file permissions allow reading the DLL

## Next Steps

- [Use Pipelines](getting-started-pipelines.md)
- [Create Advanced Commands](advanced-command-patterns.md)
- [Plugin Security](advanced-plugin-security.md)
