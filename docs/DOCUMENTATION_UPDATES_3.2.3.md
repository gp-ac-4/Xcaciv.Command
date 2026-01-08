# Documentation Updates for v3.2.3 HandlePipedChunk Signature Change

## Summary

Updated all documentation, examples, and templates to reflect the `HandlePipedChunk` signature change from `string pipedChunk` to `IResult<string> pipedChunk` in version 3.2.3.

## Files Updated

### 1. COMMAND_TEMPLATE.md
**Changes:**
- Added prominent section explaining v3.2.3 signature change
- Updated all code examples to use `IResult<string> pipedChunk`
- Added new pattern example (#7) demonstrating error propagation
- Updated all examples to use `pipedChunk.Output ?? string.Empty`
- Added migration guide section
- Added best practices for null safety and error handling

### 2. docs/examples/parameter-field-injection-example.md
**Changes:**
- Updated `HandlePipedChunk` signature in example code
- Added note about v3.2.3+ signature change
- Updated code to use `pipedChunk.Output`

### 3. docs/learn/api-core.md
**Changes:**
- Updated `AbstractCommand` interface definition
- Changed `HandleExecution` return type documentation (was missing `IResult<string>`)
- Comprehensive update to `HandlePipedChunk` documentation:
  - Version 3.2.3+ badge added
  - New signature with `IResult<string>` parameter
  - Complete example showing error checking
  - Migration guide from v3.2.2
  - Benefits section explaining advantages

### 4. src/Xcaciv.Command.Core/readme.md
**Changes:**
- Updated example code to use `IResult<string> pipedChunk`
- Added inline comment explaining v3.2.3+ change
- Updated code to extract output using `pipedChunk.Output ?? string.Empty`
- Added version note to key behaviors section

### 5. README.md
**Changes:**
- Updated version history to show **3.2.3 (Current)**
- Added detailed 3.2.3 release notes:
  - HandlePipedChunk signature change
  - Breaking change notice
  - Error propagation capability
  - Version bump for all packages
- Moved 3.2.2 to previous versions
- Added link to HandlePipedChunk migration guide in documentation section

### 6. docs/changelog-3.2.3-handlePipedChunk-signature-change.md (Already exists)
**Content:**
- Comprehensive changelog document
- Migration examples
- Benefits explanation
- Complete list of updated files

## Pattern Examples Added/Updated

### 1. Basic Signature (All Examples)
```csharp
// Old (v3.2.2)
public override IResult<string> HandlePipedChunk(
    string pipedChunk, 
    Dictionary<string, IParameterValue> parameters, 
    IEnvironmentContext env)

// New (v3.2.3+)
public override IResult<string> HandlePipedChunk(
    IResult<string> pipedChunk, 
    Dictionary<string, IParameterValue> parameters, 
    IEnvironmentContext env)
```

### 2. Null Safety Pattern
```csharp
var input = pipedChunk.Output ?? string.Empty;
```

### 3. Error Propagation Pattern (NEW)
```csharp
if (!pipedChunk.IsSuccess)
{
    // Propagate error to downstream
    return pipedChunk;
}
```

### 4. Complete Example (NEW)
```csharp
public override IResult<string> HandlePipedChunk(
    IResult<string> pipedChunk, 
    Dictionary<string, IParameterValue> parameters, 
    IEnvironmentContext env)
{
    // Check if upstream command succeeded
    if (!pipedChunk.IsSuccess)
    {
        return pipedChunk; // Propagate error
    }

    // Access the output string
    var input = pipedChunk.Output ?? string.Empty;
    
    // Process the input
    return CommandResult<string>.Success(input.Trim());
}
```

## Best Practices Added

1. **Null Safety**: Always use `pipedChunk.Output ?? string.Empty`
2. **Error Handling**: Check `pipedChunk.IsSuccess` for error-aware commands
3. **Error Propagation**: Return `pipedChunk` directly to pass errors downstream
4. **Access Metadata**: Use `pipedChunk.ResultFormat`, `CorrelationId`, `ErrorMessage`, `Exception`

## Documentation Completeness

? **Template updated** - COMMAND_TEMPLATE.md with all patterns
? **Examples updated** - parameter-field-injection-example.md
? **API docs updated** - api-core.md with comprehensive migration guide
? **Package docs updated** - Core/readme.md
? **Main docs updated** - README.md with version history
? **Changelog created** - changelog-3.2.3-handlePipedChunk-signature-change.md
? **Migration examples** - Before/after code in multiple files
? **Error patterns** - New pattern example for error propagation

## Search Keywords Covered

Documentation now covers these search terms:
- "HandlePipedChunk v3.2.3"
- "HandlePipedChunk IResult"
- "piped chunk signature change"
- "HandlePipedChunk migration"
- "pipedChunk.Output"
- "error propagation pipeline"
- "HandlePipedChunk breaking change"

## For Developers

When implementing new commands in v3.2.3+:
1. Start with COMMAND_TEMPLATE.md
2. Use `IResult<string> pipedChunk` parameter
3. Extract output: `var input = pipedChunk.Output ?? string.Empty`
4. Optional: Check errors with `if (!pipedChunk.IsSuccess)`
5. See api-core.md for detailed API reference

## For Upgrading Existing Commands

1. Change parameter type from `string` to `IResult<string>`
2. Replace `pipedChunk` with `pipedChunk.Output ?? string.Empty`
3. Optionally add error checking
4. See migration guide in changelog-3.2.3-handlePipedChunk-signature-change.md
