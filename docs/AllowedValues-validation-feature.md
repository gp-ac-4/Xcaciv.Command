# AllowedValues Validation and Auto-Default Feature

## Overview
This document describes the enhanced validation and auto-default behavior for parameters with `AllowedValues` constraints.

## Features Implemented

### 1. Default Value Validation
When a parameter has `AllowedValues` defined, the `DefaultValue` is automatically validated to ensure it exists in the allowed list.

**Behavior:**
- Validation is **case-insensitive**
- Throws `ArgumentException` if `DefaultValue` is not in `AllowedValues`
- Validation occurs when either property is set or modified

### 2. Auto-Default Assignment
When `AllowedValues` is set and `DefaultValue` is empty, the first value in `AllowedValues` is automatically assigned as the default.

**Behavior:**
- Only applies when `DefaultValue` is empty/null
- Uses the first item in the `AllowedValues` array
- Does not override an existing `DefaultValue`

## Affected Attributes

### CommandParameterNamedAttribute
```csharp
var param = new CommandParameterNamedAttribute("verbosity", "Log level")
{
    AllowedValues = new[] { "quiet", "normal", "detailed" }
    // DefaultValue automatically set to "quiet"
};
```

### CommandParameterOrderedAttribute
```csharp
var param = new CommandParameterOrderedAttribute("mode", "Operation mode")
{
    AllowedValues = new[] { "read", "write", "append" }
    // DefaultValue automatically set to "read"
};
```

## Validation Rules

### 1. Valid Scenarios
```csharp
// ? Auto-set default to first allowed value
new CommandParameterNamedAttribute("param", "desc")
{
    AllowedValues = new[] { "a", "b", "c" }
    // DefaultValue = "a" (auto-set)
};

// ? Explicit default in allowed list
new CommandParameterNamedAttribute("param", "desc")
{
    AllowedValues = new[] { "a", "b", "c" },
    DefaultValue = "b"
};

// ? Case-insensitive match
new CommandParameterNamedAttribute("param", "desc")
{
    AllowedValues = new[] { "Quiet", "Normal" },
    DefaultValue = "quiet"  // lowercase OK
};
```

### 2. Invalid Scenarios (Throws ArgumentException)
```csharp
// ? Default not in allowed list
new CommandParameterNamedAttribute("param", "desc")
{
    AllowedValues = new[] { "a", "b" },
    DefaultValue = "c"  // throws ArgumentException
};

// ? Setting AllowedValues when existing default is invalid
var param = new CommandParameterNamedAttribute("param", "desc")
{
    DefaultValue = "x"
};
param.AllowedValues = new[] { "a", "b" };  // throws ArgumentException
```

## Property Order Independence

The validation works regardless of property initialization order:

```csharp
// Both are valid and produce the same result

// Option 1: AllowedValues first
new CommandParameterNamedAttribute("p", "d")
{
    AllowedValues = new[] { "a", "b", "c" },
    DefaultValue = "b"
};

// Option 2: DefaultValue first (will validate when AllowedValues is set)
new CommandParameterNamedAttribute("p", "d")
{
    DefaultValue = "b",
    AllowedValues = new[] { "a", "b", "c" }
};
```

## Error Messages

When validation fails, a descriptive error message is provided:

```
ArgumentException: Default value 'invalid' is not in the allowed values list for parameter 'verbosity'. 
Allowed values: quiet, normal, detailed
```

## Implementation Details

### Private Fields
- `_defaultValue`: Backing field for `DefaultValue` property
- `_allowedValues`: Backing field for `AllowedValues` property

### Validation Logic
The `ValidateDefaultValue()` private method:
1. Returns early if `DefaultValue` is empty or `AllowedValues` is null/empty
2. Performs case-insensitive comparison using `StringComparer.OrdinalIgnoreCase`
3. Throws `ArgumentException` with detailed message if validation fails

### Auto-Default Logic
In the `AllowedValues` setter:
1. Normalizes null to empty array
2. If `DefaultValue` is empty and `AllowedValues` has items, sets `DefaultValue` to first item
3. Validates existing `DefaultValue` against new `AllowedValues`

## Testing

Comprehensive test coverage includes:
- ? Auto-set default to first allowed value
- ? Valid explicit default in allowed list
- ? Invalid default throws exception
- ? Case-insensitive validation
- ? Property order independence
- ? Empty/null AllowedValues handling
- ? Existing default preservation

See `AllowedValuesValidationTests.cs` for complete test suite.

## Backward Compatibility

This feature is **backward compatible**:
- Existing code without `AllowedValues` continues to work unchanged
- No breaking changes to public API
- Only adds validation where `AllowedValues` is explicitly set

## Best Practices

1. **Define AllowedValues first** when possible for clearer intent:
   ```csharp
   AllowedValues = new[] { "option1", "option2" },
   DefaultValue = "option1"  // explicit and validated
   ```

2. **Use the auto-default feature** when the first value is the desired default:
   ```csharp
   AllowedValues = new[] { "default", "other" }
   // DefaultValue automatically = "default"
   ```

3. **Handle exceptions** during attribute construction in dynamic scenarios:
   ```csharp
   try {
       var attr = CreateAttribute(userConfig);
   } catch (ArgumentException ex) {
       // Log validation error and provide user feedback
   }
   ```
