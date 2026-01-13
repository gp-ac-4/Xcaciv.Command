# Parameter Help Text Consistency Feature

## Overview
This document describes the implementation that ensures consistent help text display for all parameter types (ordered, named, and suffix) when they have `AllowedValues` defined.

## Problem Statement

Previously, there was an inconsistency in how help text was displayed:
- **Named parameters** (e.g., `-verbosity quiet`) would show: `"Log level (Allowed values: quiet, normal, detailed)"`
- **Ordered parameters** (e.g., `command mode`) would show: `"Operation mode"` (missing allowed values)
- **Suffix parameters** did not support `AllowedValues` at all

This inconsistency made it harder for users to understand valid values for ordered and suffix parameters.

## Solution

1. Added `GetValueDescription()` override to `CommandParameterOrderedAttribute` to match the existing behavior in `CommandParameterNamedAttribute`
2. Added `AllowedValues` property support to `CommandParameterSuffixAttribute` with validation and auto-default behavior
3. Added `GetValueDescription()` override to `CommandParameterSuffixAttribute` for consistency

### Implementation

**Location**: 
- `src\Xcaciv.Command.Interface\Attributes\CommandParameterOrderedAttribute.cs`
- `src\Xcaciv.Command.Interface\Attributes\CommandParameterSuffixAttribute.cs`

```csharp
public override string GetValueDescription()
{
    string description = ValueDescription;
    if (AllowedValues.Length > 0)
    {
        description += $" (Allowed values: {string.Join(", ", AllowedValues)})";
    }
    return description;
}
```

## Examples

### Before (Inconsistent)

```csharp
// Named parameter
[CommandParameterNamed("verbosity", "Log level", 
    AllowedValues = new[] { "quiet", "normal", "detailed" })]
// Help: "Log level (Allowed values: quiet, normal, detailed)" ?

// Ordered parameter
[CommandParameterOrdered("mode", "Operation mode",
    AllowedValues = new[] { "read", "write", "append" })]
// Help: "Operation mode" ? Missing allowed values

// Suffix parameter - NO AllowedValues support
[CommandParameterSuffix("extensions", "File extensions")]
// Help: "File extensions" ? No validation possible
```

### After (Consistent)

```csharp
// Named parameter
[CommandParameterNamed("verbosity", "Log level", 
    AllowedValues = new[] { "quiet", "normal", "detailed" })]
// Help: "Log level (Allowed values: quiet, normal, detailed)" ?

// Ordered parameter
[CommandParameterOrdered("mode", "Operation mode",
    AllowedValues = new[] { "read", "write", "append" })]
// Help: "Operation mode (Allowed values: read, write, append)" ?

// Suffix parameter
[CommandParameterSuffix("extensions", "File extensions",
    AllowedValues = new[] { ".txt", ".json", ".xml" })]
// Help: "File extensions (Allowed values: .txt, .json, .xml)" ?
```

## Usage in Commands

### Ordered Parameter with Allowed Values

```csharp
[CommandRegister("BUILD", "Build project")]
[CommandParameterOrdered(
    "configuration",
    "Build configuration",
    AllowedValues = new[] { "Debug", "Release", "Test" },
    IsRequired = false
)]
public class BuildCommand : AbstractCommand
{
    public string? Configuration;
    
    // When help is displayed:
    // <configuration>  Build configuration (Allowed values: Debug, Release, Test)
}
```

### Named Parameter with Allowed Values

```csharp
[CommandRegister("DEPLOY", "Deploy application")]
[CommandParameterNamed(
    "environment",
    "Target environment",
    AllowedValues = new[] { "dev", "staging", "production" }
)]
public class DeployCommand : AbstractCommand
{
    public string? Environment;
    
    // When help is displayed:
    // -environment     Target environment (Allowed values: dev, staging, production)
}
```

### Suffix Parameter with Allowed Values (NEW)

```csharp
[CommandRegister("PROCESS", "Process files")]
[CommandParameterSuffix(
    "extensions",
    "File extensions to include",
    AllowedValues = new[] { ".txt", ".json", ".xml", ".csv" }
)]
public class ProcessCommand : AbstractCommand
{
    public string? Extensions;
    
    // When help is displayed:
    // [extensions...]  File extensions to include (Allowed values: .txt, .json, .xml, .csv)
}
```

## Help Text Format

The format is consistent across all three parameter types:

```
<parameter-indicator>  <description> (Allowed values: <value1>, <value2>, ...)
```

**For Ordered Parameters**:
```
<mode>               Operation mode (Allowed values: read, write, append)
```

**For Named Parameters**:
```
-verbosity           Log level (Allowed values: quiet, normal, detailed)
```

**For Suffix Parameters (NEW)**:
```
[extensions...]      File extensions (Allowed values: .txt, .json, .xml)
```

## Edge Cases Handled

### 1. No Allowed Values
If `AllowedValues` is empty or not set, only the description is shown:
```csharp
[CommandParameterOrdered("filename", "Input file")]
// Help: "Input file"
```

### 2. Single Allowed Value
```csharp
[CommandParameterSuffix("format", "Format", AllowedValues = new[] { "json" })]
// Help: "Format (Allowed values: json)"
```

### 3. Multiple Allowed Values
Values are comma-separated:
```csharp
[CommandParameterOrdered("level", "Level", 
    AllowedValues = new[] { "low", "medium", "high", "maximum" })]
// Help: "Level (Allowed values: low, medium, high, maximum)"
```

### 4. Auto-Default with Allowed Values
When `AllowedValues` auto-sets `DefaultValue`, the help still shows ALL allowed values:
```csharp
[CommandParameterSuffix("type", "Resource type",
    AllowedValues = new[] { "file", "directory", "link" })]
// DefaultValue auto-set to "file"
// Help: "Resource type (Allowed values: file, directory, link)"
```

## Testing

### Test Coverage
Comprehensive tests in `ParameterHelpTextConsistencyTests.cs`:

**Ordered Parameters (10 tests)**:
1. ? Show allowed values in help text
2. ? Without allowed values show only description
3. ? Multiple values comma-separated
4. ? Consistent formatting with named parameters
5. ? ToString includes allowed values
6. ? Empty allowed values handled
7. ? Single value formatting
8. ? Special characters preserved
9. ? Auto-default doesn't hide other values
10. ? Case-insensitive validation

**Suffix Parameters (6 new tests)**:
11. ? Show allowed values in help text
12. ? Without allowed values show only description
13. ? Consistent formatting across all types
14. ? Validates default value
15. ? Auto-sets default to first value
16. ? ToString includes allowed values

### Running Tests

```bash
# Run all help text consistency tests
dotnet test --filter "FullyQualifiedName~ParameterHelpTextConsistencyTests"

# All tests should pass
Test summary: total: 16, failed: 0, succeeded: 16
```

## Benefits

### 1. **Complete Consistency**
All three parameter types now display allowed values in the same format.

### 2. **Enhanced Suffix Parameters**
Suffix parameters now support `AllowedValues`, enabling validation of variable-length argument lists.

### 3. **Improved User Experience**
Users can now see valid values for all parameter types in help text.

### 4. **Self-Documenting**
Commands with constrained input automatically document their valid values through help text.

### 5. **Error Prevention**
Users are less likely to provide invalid values when they can see the allowed options upfront.

## Suffix Parameter AllowedValues Feature

### New Capabilities

`CommandParameterSuffixAttribute` now supports:

1. **AllowedValues Property**: Define a list of valid values
2. **Default Value Validation**: Ensures default is in allowed list
3. **Auto-Default**: First allowed value becomes default when not specified
4. **Help Text Integration**: Shows allowed values in help output

### Example Use Cases

```csharp
// File type filtering
[CommandParameterSuffix("types", "File types to process",
    AllowedValues = new[] { "image", "document", "video", "audio" })]

// Status codes
[CommandParameterSuffix("statuses", "HTTP status codes to monitor",
    AllowedValues = new[] { "200", "301", "404", "500" })]

// Environment names
[CommandParameterSuffix("environments", "Deployment environments",
    AllowedValues = new[] { "dev", "test", "staging", "production" })]
```

## Related Features

This change works seamlessly with:
- **AllowedValues validation** - Runtime validation ensures values match
- **Auto-default feature** - First allowed value becomes default when not specified
- **Case-insensitive validation** - Validation is case-insensitive while display preserves original casing
- **Field injection** - Parameters automatically populate command fields

## Backward Compatibility

? **Fully backward compatible**:
- Existing commands without `AllowedValues` unchanged
- `CommandParameterSuffixAttribute` without `AllowedValues` works as before
- No breaking changes to public APIs
- Only enhances help text display
- No performance impact

## Implementation Details

### Class Hierarchy
```
AbstractCommandParameter (base class)
??? GetValueDescription() (virtual method)
??? CommandParameterNamedAttribute (override added in v3.2.5)
?   ??? GetValueDescription() returns description + allowed values
??? CommandParameterOrderedAttribute (override added in v3.2.6)
?   ??? GetValueDescription() returns description + allowed values
??? CommandParameterSuffixAttribute (AllowedValues + override added in v3.2.6) ? NEW
    ??? GetValueDescription() returns description + allowed values
```

### Suffix Parameter Implementation Details

```csharp
public class CommandParameterSuffixAttribute : AbstractCommandParameter
{
    private string _defaultValue = "";
    private string[] _allowedValues = [];

    // AllowedValues property with validation
    public string[] AllowedValues { get; set; }
    
    // DefaultValue property with validation
    public string DefaultValue { get; set; }
    
    // Help text override
    public override string GetValueDescription() { }
    
    // Internal validation
    private void ValidateDefaultValue(string defaultValue, string[] allowedValues) { }
}
```

## Version History

- **v3.2.5**: AllowedValues validation and auto-default feature added for Named and Ordered parameters
- **v3.2.6**: 
  - Help text consistency implemented for ordered parameters
  - AllowedValues support added to suffix parameters ? NEW
  - Help text consistency implemented for suffix parameters ? NEW

## See Also

- [AllowedValues Validation Feature](AllowedValues-validation-feature.md)
- [Parameter System Documentation](COMMANDPARAMETERS_CONVERSION.md)
- [API Attributes Guide](learn/api-attributes.md)
