# Command Implementation Template

This document provides a template and guidelines for implementing new commands in the Xcaciv.Command framework.

## Quick Reference: Parameter Attributes

| Attribute | Usage | Notes |
|-----------|-------|-------|
| `CommandRegisterAttribute` | **Required** on class | Registers the command with a name and description |
| `CommandParameterOrderedAttribute` | Position-based parameters | Must precede named parameters |
| `CommandParameterNamedAttribute` | Named parameters (with `-name` flag) | Used with `-name value` syntax |
| `CommandFlagAttribute` | Boolean toggle flags | Presence = true, absence = false |
| `CommandParameterSuffixAttribute` | Capture remaining arguments | Collects all remaining args as single value |
| `CommandHelpRemarksAttribute` | Additional help information | Multiple remarks can be added |
| `CommandRootAttribute` | Multi-level sub-commands | For commands with sub-commands |

## Basic Command Template

```csharp
using System;
using System.Collections.Generic;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;

namespace Xcaciv.Command.Commands
{
    [CommandRegister("MyCommand", "Description of what the command does", Prototype = "MYCOMMAND <param1> <param2>")]
    [CommandParameterOrdered("param1", "Description of first parameter")]
    [CommandParameterOrdered("param2", "Description of second parameter")]
    internal class MyCommand : AbstractCommand
    {
        public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            // Extract typed parameters safely
            var param1 = parameters.TryGetValue("param1", out var p1) && p1.IsValid 
                ? p1.GetValue<string>() 
                : string.Empty;
            
            var param2 = parameters.TryGetValue("param2", out var p2) && p2.IsValid 
                ? p2.GetValue<string>() 
                : string.Empty;

            // Execute command logic
            var result = $"Processed: {param1}, {param2}";
            
            return result;
        }

        public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            // Handle piped input
            return pipedChunk.ToUpper();
        }
    }
}
```

## Pattern Examples

### 1. Simple Command (No Parameters)

```csharp
[CommandRegister("Now", "Display current timestamp", Prototype = "NOW")]
internal class NowCommand : AbstractCommand
{
    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return DateTime.UtcNow.ToString("O");
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return pipedChunk; // Pass through piped input
    }
}
```

### 2. Command with Ordered Parameters

```csharp
[CommandRegister("Add", "Add two numbers", Prototype = "ADD <number1> <number2>")]
[CommandParameterOrdered("Number1", "First number")]
[CommandParameterOrdered("Number2", "Second number")]
internal class AddCommand : AbstractCommand
{
    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var num1 = parameters.TryGetValue("number1", out var p1) && p1.IsValid 
            ? p1.GetValue<int>() 
            : 0;
        var num2 = parameters.TryGetValue("number2", out var p2) && p2.IsValid 
            ? p2.GetValue<int>() 
            : 0;

        return (num1 + num2).ToString();
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return string.Empty; // Not designed for piping
    }
}
```

### 3. Command with Named Parameters

```csharp
[CommandRegister("Copy", "Copy with optional verbose flag", Prototype = "COPY <source> -dest <destination> [-v]")]
[CommandParameterOrdered("Source", "Source path")]
[CommandParameterNamed("Dest", "Destination path", IsRequired = true)]
[CommandFlag("Verbose", "Show verbose output")]
internal class CopyCommand : AbstractCommand
{
    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var source = parameters.TryGetValue("source", out var src) && src.IsValid 
            ? src.GetValue<string>() 
            : string.Empty;
        
        var dest = parameters.TryGetValue("dest", out var d) && d.IsValid 
            ? d.GetValue<string>() 
            : string.Empty;
        
        var verbose = parameters.TryGetValue("verbose", out var v) && v.IsValid 
            ? v.GetValue<bool>() 
            : false;

        // Execute copy logic
        return $"Copied {source} to {dest}" + (verbose ? " [verbose mode]" : "");
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return string.Empty;
    }
}
```

### 4. Command with Suffix Parameter (Capture All Remaining Args)

```csharp
[CommandRegister("Echo", "Echo text with interpolation", Prototype = "ECHO <text...>")]
[CommandParameterSuffix("text", "Text to echo")]
internal class EchoCommand : AbstractCommand
{
    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var text = parameters.TryGetValue("text", out var t) && t.IsValid 
            ? t.GetValue<string>() 
            : string.Empty;
        
        return text;
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return pipedChunk;
    }
}
```

### 5. Command with Piped Input (SET pattern)

```csharp
[CommandRegister("Buffer", "Buffer piped input", Prototype = "BUFFER <name>")]
[CommandParameterOrdered("Name", "Variable name")]
[CommandParameterOrdered("Value", "Initial value", UsePipe = true)]
[CommandHelpRemarks("This command accumulates piped input into a variable.")]
internal class BufferCommand : AbstractCommand
{
    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var name = parameters.TryGetValue("name", out var n) && n.IsValid 
            ? n.GetValue<string>() 
            : string.Empty;
        var value = parameters.TryGetValue("value", out var v) && v.IsValid 
            ? v.GetValue<string>() 
            : string.Empty;

        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
        {
            env.SetValue(name, value);
        }
        return string.Empty;
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var name = parameters.TryGetValue("name", out var n) && n.IsValid 
            ? n.GetValue<string>() 
            : string.Empty;
        
        if (!string.IsNullOrEmpty(name))
        {
            var current = env.GetValue(name);
            env.SetValue(name, current + pipedChunk);
        }
        return string.Empty;
    }

    protected override void OnStartPipe(Dictionary<string, IParameterValue> processedParameters, IEnvironmentContext environment)
    {
        var name = processedParameters.TryGetValue("name", out var n) && n.IsValid 
            ? n.GetValue<string>() 
            : string.Empty;
        
        if (!string.IsNullOrEmpty(name))
        {
            environment.SetValue(name, string.Empty);
        }
        base.OnStartPipe(processedParameters, environment);
    }
}
```

### 6. Command with Stateful Processing (REGIF pattern)

```csharp
[CommandRegister("Filter", "Filter piped input by condition", Prototype = "FILTER <condition>")]
[CommandParameterOrdered("Condition", "Filter condition")]
internal class FilterCommand : AbstractCommand
{
    private bool cachedCondition = false;

    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        // When not piping, just initialize
        if (parameters.TryGetValue("condition", out var c) && c.IsValid)
        {
            cachedCondition = c.GetValue<bool>();
        }
        return string.Empty;
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        if (cachedCondition)
        {
            return pipedChunk;
        }
        return string.Empty;
    }
}
```

### 7. Command with Help Remarks and Aliases

```csharp
[CommandRegister("Count", "Count lines or characters", Prototype = "COUNT [-type lines|chars]")]
[CommandParameterNamed("Type", "Count type", DefaultValue = "lines", AllowedValues = new[] { "lines", "chars" })]
[CommandHelpRemarks("Counts the number of lines or characters in piped input.")]
[CommandHelpRemarks("Default behavior counts lines. Use -type chars to count characters.")]
internal class CountCommand : AbstractCommand
{
    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return "0";
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var type = parameters.TryGetValue("type", out var t) && t.IsValid 
            ? t.GetValue<string>() 
            : "lines";

        var count = type == "chars" 
            ? pipedChunk.Length 
            : pipedChunk.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None).Length;

        return count.ToString();
    }
}
```

## Parameter Type Examples

### Numeric Types (int, long, double, decimal)

```csharp
[CommandRegister("Calculate", "Demonstrate numeric parameter types", Prototype = "CALCULATE <integer> <largenum> <decimal>")]
[CommandParameterOrdered("Integer", "An integer value")]
[CommandParameterOrdered("LargeNum", "A long integer value")]
[CommandParameterOrdered("Decimal", "A decimal value")]
internal class CalculateCommand : AbstractCommand
{
    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var intValue = parameters.TryGetValue("integer", out var intParam) && intParam.IsValid 
            ? intParam.GetValue<int>() 
            : 0;
        
        var longValue = parameters.TryGetValue("largenum", out var longParam) && longParam.IsValid 
            ? longParam.GetValue<long>() 
            : 0L;
        
        var decimalValue = parameters.TryGetValue("decimal", out var decParam) && decParam.IsValid 
            ? decParam.GetValue<decimal>() 
            : 0m;

        var result = $"Int: {intValue}, Long: {longValue}, Decimal: {decimalValue}";
        return result;
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return string.Empty;
    }
}
```

### GUID Type

```csharp
[CommandRegister("ValidateGuid", "Validate or work with GUIDs", Prototype = "VALIDATEGUID <guid>")]
[CommandParameterOrdered("Guid", "A GUID value")]
internal class ValidateGuidCommand : AbstractCommand
{
    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var guid = parameters.TryGetValue("guid", out var guidParam) && guidParam.IsValid 
            ? guidParam.GetValue<Guid>() 
            : Guid.Empty;

        if (guid == Guid.Empty)
        {
            return "Invalid GUID provided";
        }

        return $"Valid GUID: {guid:D}";
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return string.Empty;
    }
}
```

### DateTime Type

```csharp
[CommandRegister("DateFormat", "Format a date value", Prototype = "DATEFORMAT <date> [-format ISO|Short|Long]")]
[CommandParameterOrdered("Date", "A date value")]
[CommandParameterNamed("Format", "Output format", DefaultValue = "ISO", AllowedValues = new[] { "ISO", "Short", "Long" })]
internal class DateFormatCommand : AbstractCommand
{
    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var date = parameters.TryGetValue("date", out var dateParam) && dateParam.IsValid 
            ? dateParam.GetValue<DateTime>() 
            : DateTime.Now;
        
        var format = parameters.TryGetValue("format", out var formatParam) && formatParam.IsValid 
            ? formatParam.GetValue<string>() 
            : "ISO";

        return format switch
        {
            "ISO" => date.ToString("O"),
            "Short" => date.ToString("d"),
            "Long" => date.ToString("D"),
            _ => date.ToString("O")
        };
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return string.Empty;
    }
}
```

### Boolean and Nullable Types

```csharp
[CommandRegister("Conditional", "Demonstrate boolean and nullable types", Prototype = "CONDITIONAL <enabled> [-optional <value>]")]
[CommandParameterOrdered("Enabled", "Enable feature")]
[CommandParameterNamed("Optional", "Optional nullable value")]
internal class ConditionalCommand : AbstractCommand
{
    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var enabled = parameters.TryGetValue("enabled", out var enabledParam) && enabledParam.IsValid 
            ? enabledParam.GetValue<bool>() 
            : false;
        
        var optional = parameters.TryGetValue("optional", out var optionalParam) && optionalParam.IsValid 
            ? optionalParam.GetValue<int?>() 
            : null;

        var result = $"Enabled: {enabled}";
        if (optional.HasValue)
        {
            result += $", Optional value: {optional.Value}";
        }
        return result;
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return string.Empty;
    }
}
```

## Output Format Examples

### CSV Output Format

```csharp
[CommandRegister("ListCsv", "Output data in CSV format", Prototype = "LISTCSV")]
internal class ListCsvCommand : AbstractCommand
{
    private class Record
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
    }

    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var records = new List<Record>
        {
            new Record { Name = "Alice Johnson", Age = 28, Email = "alice@example.com" },
            new Record { Name = "Bob Smith", Age = 35, Email = "bob@example.com" },
            new Record { Name = "Carol White", Age = 42, Email = "carol@example.com" }
        };

        var csvOutput = new StringBuilder();
        // Header row
        csvOutput.AppendLine("Name,Age,Email");
        
        // Data rows
        foreach (var record in records)
        {
            var escapedName = EscapeCsvField(record.Name);
            var escapedEmail = EscapeCsvField(record.Email);
            csvOutput.AppendLine($"{escapedName},{record.Age},{escapedEmail}");
        }

        return csvOutput.ToString().TrimEnd();
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return string.Empty;
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return field;
    }
}
```

### TSV (Tab-Separated Values) / TDL Output Format

```csharp
[CommandRegister("ListTsv", "Output data in TSV/TDL format", Prototype = "LISTTSV")]
internal class ListTsvCommand : AbstractCommand
{
    private class Record
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string City { get; set; }
    }

    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var records = new List<Record>
        {
            new Record { Name = "Alice", Age = 28, City = "New York" },
            new Record { Name = "Bob", Age = 35, City = "San Francisco" },
            new Record { Name = "Carol", Age = 42, City = "Chicago" }
        };

        var tsvOutput = new StringBuilder();
        // Header row
        tsvOutput.AppendLine("Name\tAge\tCity");
        
        // Data rows
        foreach (var record in records)
        {
            tsvOutput.AppendLine($"{record.Name}\t{record.Age}\t{record.City}");
        }

        return tsvOutput.ToString().TrimEnd();
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return string.Empty;
    }
}
```

### JSON Output Format

```csharp
using System.Text.Json;

[CommandRegister("ListJson", "Output data in JSON format", Prototype = "LISTJSON [-pretty]")]
[CommandFlag("Pretty", "Pretty-print JSON output")]
internal class ListJsonCommand : AbstractCommand
{
    private class Record
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
    }

    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var records = new List<Record>
        {
            new Record { Name = "Alice Johnson", Age = 28, Email = "alice@example.com" },
            new Record { Name = "Bob Smith", Age = 35, Email = "bob@example.com" },
            new Record { Name = "Carol White", Age = 42, Email = "carol@example.com" }
        };

        var pretty = parameters.TryGetValue("pretty", out var prettyParam) && prettyParam.IsValid 
            ? prettyParam.GetValue<bool>() 
            : false;

        var options = new JsonSerializerOptions
        {
            WriteIndented = pretty
        };

        return JsonSerializer.Serialize(records, options);
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return string.Empty;
    }
}
```

### JSON Object with Metadata

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

[CommandRegister("DataExport", "Export data with metadata in JSON", Prototype = "DATAEXPORT [-format json|compact]")]
[CommandParameterNamed("Format", "Output format", DefaultValue = "json", AllowedValues = new[] { "json", "compact" })]
internal class DataExportCommand : AbstractCommand
{
    [JsonPropertyName("items")]
    public class ExportData
    {
        public List<string> Items { get; set; }
        
        [JsonPropertyName("exported_at")]
        public DateTime ExportedAt { get; set; }
        
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var format = parameters.TryGetValue("format", out var formatParam) && formatParam.IsValid 
            ? formatParam.GetValue<string>() 
            : "json";

        var data = new ExportData
        {
            Items = new List<string> { "item1", "item2", "item3" },
            ExportedAt = DateTime.UtcNow,
            Count = 3
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = format == "json",
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(data, options);
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return string.Empty;
    }
}
```

### Mixed Output with Conditionals

```csharp
[CommandRegister("Report", "Generate report in multiple formats", Prototype = "REPORT <format>")]
[CommandParameterOrdered("Format", "Output format", AllowedValues = new[] { "text", "csv", "json" })]
internal class ReportCommand : AbstractCommand
{
    private class DataItem
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public decimal Value { get; set; }
    }

    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var format = parameters.TryGetValue("format", out var formatParam) && formatParam.IsValid 
            ? formatParam.GetValue<string>() 
            : "text";

        var items = new List<DataItem>
        {
            new DataItem { Id = "A001", Description = "Item A", Value = 100.50m },
            new DataItem { Id = "B002", Description = "Item B", Value = 200.75m },
            new DataItem { Id = "C003", Description = "Item C", Value = 150.25m }
        };

        return format switch
        {
            "csv" => GenerateCsv(items),
            "json" => GenerateJson(items),
            _ => GenerateText(items)
        };
    }

    private string GenerateText(List<DataItem> items)
    {
        var output = new StringBuilder();
        output.AppendLine("=== Report ===");
        foreach (var item in items)
        {
            output.AppendLine($"ID: {item.Id,-6} Description: {item.Description,-15} Value: {item.Value:C}");
        }
        return output.ToString().TrimEnd();
    }

    private string GenerateCsv(List<DataItem> items)
    {
        var output = new StringBuilder();
        output.AppendLine("ID,Description,Value");
        foreach (var item in items)
        {
            output.AppendLine($"{item.Id},{item.Description},{item.Value}");
        }
        return output.ToString().TrimEnd();
    }

    private string GenerateJson(List<DataItem> items)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(items, options);
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return string.Empty;
    }
}
```

## Key Implementation Guidelines

### Parameter Extraction Pattern
Always use the safe pattern for parameter extraction:
```csharp
var paramValue = parameters.TryGetValue("paramname", out var param) && param.IsValid 
    ? param.GetValue<DesiredType>() 
    : defaultValue;
```

This pattern:
- Uses `TryGetValue` for safe dictionary access
- Checks `IsValid` to ensure the parameter was successfully parsed
- Uses `GetValue<T>()` for type-safe access
- Provides a default value fallback

### Return Values
- **HandleExecution**: Return the command output as a string. Return `string.Empty` if no output.
- **HandlePipedChunk**: Process one chunk of piped input and return the result.
- Both methods should never return null; use `string.Empty` instead.

### Piped Input Support
To support piped input, set `UsePipe = true` on one parameter:
```csharp
[CommandParameterOrdered("Value", "Value to process", UsePipe = true)]
```

When piping is active:
1. `OnStartPipe()` is called first (optional override)
2. `HandlePipedChunk()` is called for each chunk
3. `OnEndPipe()` is called last (optional override)

The framework automatically excludes piped parameters from normal parameter processing.

### Environment Modification
Commands that modify environment (like `SET`) should set `ModifiesEnvironment = true` on the `CommandRegisterAttribute` if available, though most commands simply call `env.SetValue()` when needed.

### Type Support
Parameters support these types:
- `string`
- `int`, `long`, `double`, `decimal`
- `bool`
- `Guid`
- `DateTime`
- Nullable variants (e.g., `int?`, `bool?`)
- JSON types (when available)

### No Parameters Required
A command doesn't need any parameters:
```csharp
[CommandRegister("Now", "Get current time")]
internal class NowCommand : AbstractCommand
{
    // No parameter attributes needed
    public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return DateTime.UtcNow.ToString("O");
    }

    public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        return pipedChunk;
    }
}
```

## Attribute Properties Reference

### CommandRegisterAttribute (Required)
```csharp
[CommandRegister(
    command: "NAME",           // How to invoke the command
    description: "...",        // Short description
    Prototype = "PROTOTYPE",   // Usage example
    Version = "1.0.0",         // Command version
    Alias = "N"                // Single-char alias (optional)
)]
```

### CommandParameterOrderedAttribute
```csharp
[CommandParameterOrdered(
    name: "ParamName",
    description: "...",
    IsRequired = true,         // Default: true
    UsePipe = false,           // Default: false
    DefaultValue = "",         // Default: ""
    AllowedValues = new[] { }  // Default: empty
)]
```

### CommandParameterNamedAttribute
```csharp
[CommandParameterNamed(
    name: "ParamName",
    description: "...",
    IsRequired = false,        // Default: false
    UsePipe = false,           // Default: false
    DefaultValue = "",         // Default: ""
    ShortAlias = "",           // Default: ""
    AllowedValues = new[] { }  // Default: empty
)]
```

### CommandFlagAttribute
```csharp
[CommandFlag(
    name: "FlagName",
    description: "..."
)]
```

### CommandParameterSuffixAttribute
```csharp
[CommandParameterSuffix(
    name: "ParamName",
    description: "...",
    IsRequired = true,         // Default: true
    UsePipe = false,           // Default: false
    DefaultValue = ""          // Default: ""
)]
```

## Common Patterns

### Pass-Through Filter
```csharp
public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
{
    return pipedChunk; // Return unchanged
}
```

### Conditional Filter
```csharp
public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
{
    if (SomeCondition(pipedChunk))
        return pipedChunk;
    return string.Empty; // Filter out
}
```

### Transform
```csharp
public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
{
    return pipedChunk.ToUpper(); // Transform and return
}
```

### Accumulate
```csharp
protected override void OnStartPipe(Dictionary<string, IParameterValue> processedParameters, IEnvironmentContext environment)
{
    accumulated = string.Empty;
    base.OnStartPipe(processedParameters, environment);
}

public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
{
    accumulated += pipedChunk;
    return string.Empty;
}

protected override void OnEndPipe(Dictionary<string, IParameterValue> processedParameters, IEnvironmentContext environment)
{
    FinalizeAccumulation();
    base.OnEndPipe(processedParameters, environment);
}
```

## Testing Your Command

Commands should be tested for:
- Parameter parsing with valid inputs
- Parameter validation with invalid inputs
- Piped input handling
- Non-piped input handling
- Help text generation
- Environment interaction (if applicable)

Example test structure:
```csharp
[TestClass]
public class MyCommandTests
{
    private MyCommand command;
    private Dictionary<string, IParameterValue> parameters;
    private EnvironmentContext env;

    [TestInitialize]
    public void Setup()
    {
        command = new MyCommand();
        parameters = new Dictionary<string, IParameterValue>(StringComparer.OrdinalIgnoreCase);
        env = new EnvironmentContext();
    }

    [TestMethod]
    public void HandleExecution_WithValidParameters_ReturnsExpectedOutput()
    {
        // Arrange
        // Set up parameters

        // Act
        var result = command.HandleExecution(parameters, env);

        // Assert
        Assert.IsNotNull(result);
    }
}
```

## Security Considerations

- **Input Validation**: All parameters are pre-validated by the framework using attribute constraints
- **Type Safety**: Always use `GetValue<T>()` to ensure type safety
- **Environment Access**: Use `IEnvironmentContext` methods for safe environment variable access
- **Piped Input**: Treat piped input as potentially untrusted; validate and sanitize as needed
- **Resource Limits**: Be mindful of memory when processing large piped inputs

## See Also

- [Parameter System Implementation](PARAMETER_SYSTEM_IMPLEMENTATION.md)
- [Built-in Commands](src/Xcaciv.Command/Commands/) - Reference implementations
- [Command Interface](src/Xcaciv.Command.Interface/ICommandDelegate.cs)
- [AbstractCommand](src/Xcaciv.Command.Core/AbstractCommand.cs)
