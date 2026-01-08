# HandlePipedChunk Signature Change - Version 3.2.3

## Summary

Updated the `HandlePipedChunk` method signature in `AbstractCommand` to accept `IResult<string>` instead of `string`, allowing commands to access the full result context including success status and error information from upstream piped commands.

## Changes Made

### 1. Interface Change - AbstractCommand.cs
**File**: `src/Xcaciv.Command.Core/AbstractCommand.cs`

**Old Signature**:
```csharp
public abstract IResult<string> HandlePipedChunk(
    string pipedChunk, 
    Dictionary<string, IParameterValue> parameters, 
    IEnvironmentContext env);
```

**New Signature**:
```csharp
public abstract IResult<string> HandlePipedChunk(
    IResult<string> pipedChunk, 
    Dictionary<string, IParameterValue> parameters, 
    IEnvironmentContext env);
```

### 2. Updated Command Implementations

All command implementations inheriting from `AbstractCommand` were updated:

#### Built-in Commands (src/Xcaciv.Command/Commands/)
- **SayCommand.cs**: Updated to use `pipedChunk.Output` with null-coalescing
- **EnvCommand.cs**: Updated signature (parameter not used in implementation)
- **SetCommand.cs**: Updated to use `pipedChunk.Output` with null-coalescing
- **RegifCommand.cs**: Updated to use `pipedChunk.Output` with null-coalescing

#### Test Package Commands (src/zTestCommandPackage/)
- **DoEchoCommand.cs**: Updated to use `pipedChunk.Output`
- **DoSayCommand.cs**: Updated to use `pipedChunk.Output`

#### Test Commands (src/Xcaciv.Command.Tests/Commands/)
- **InstallCommand.cs**: Updated to use `pipedChunk.Output`
- **ParameterTestCommand.cs**: Updated signature (throws NotImplementedException)

### 3. Test Updates

**File**: `src/Xcaciv.Command.Tests/PrameterParsingTests.cs`
- Updated test to pass `CommandResult<string>.Success("piped chunk")` instead of plain string

### 4. Version Bumps (3.2.2 → 3.2.3)

Updated version numbers in all project files:
- `src/Xcaciv.Command.Core/Xcaciv.Command.Core.csproj`
- `src/Xcaciv.Command.Interface/Xcaciv.Command.Interface.csproj`
- `src/Xcaciv.Command.FileLoader/Xcaciv.Command.FileLoader.csproj`
- `src/Xcaciv.Command.DependencyInjection/Xcaciv.Command.DependencyInjection.csproj`
- `src/Xcaciv.Command.Extensions.Commandline/Xcaciv.Command.Extensions.Commandline.csproj`
- `src/Xcaciv.Command/Xcaciv.Command.csproj` (already at 3.2.3)

### 5. Null Safety

Added null-coalescing operators where `pipedChunk.Output` could be null:
```csharp
var input = pipedChunk.Output ?? string.Empty;
```

## Benefits

1. **Access to Result Context**: Commands can now check `pipedChunk.IsSuccess` to handle upstream failures
2. **Error Propagation**: Commands can access `pipedChunk.ErrorMessage` and `pipedChunk.Exception`
3. **Metadata Access**: Commands have access to `ResultFormat` and `CorrelationId`
4. **Better Pipeline Awareness**: Commands know if they're processing successful or failed results

## Example Usage

```csharp
public override IResult<string> HandlePipedChunk(
    IResult<string> pipedChunk, 
    Dictionary<string, IParameterValue> parameters, 
    IEnvironmentContext env)
{
    // Check if upstream command succeeded
    if (!pipedChunk.IsSuccess)
    {
        // Optionally handle failure or pass through
        return pipedChunk;
    }

    // Access the output data
    var input = pipedChunk.Output ?? string.Empty;
    
    // Process the data
    var processed = ProcessData(input);
    
    return CommandResult<string>.Success(processed);
}
```

## Commands Not Affected

These commands implement `ICommandDelegate` directly (not `AbstractCommand`) and were not changed:
- `EchoCommand.cs`
- `EchoChamberCommand.cs`
- `EchoEncodeCommand.cs`
- `PingCommand.cs`
- `CommandLineCommand.cs`

## Test Results

✓ **All 224 tests passing**
- `Xcaciv.Command.Tests`: 212 tests passed
- `Xcaciv.Command.FileLoaderTests`: 12 tests passed
- No breaking changes for direct `ICommandDelegate` implementations
- All `AbstractCommand` implementations updated successfully

## Breaking Change

**This is a breaking change** for any custom commands that inherit from `AbstractCommand`. Developers will need to:
1. Update the `HandlePipedChunk` signature
2. Replace `string pipedChunk` parameter with `IResult<string> pipedChunk`
3. Use `pipedChunk.Output` to access the string value
4. Optionally check `pipedChunk.IsSuccess` for error handling

## Migration Example

**Before**:
```csharp
public override IResult<string> HandlePipedChunk(
    string pipedChunk, 
    Dictionary<string, IParameterValue> parameters, 
    IEnvironmentContext env)
{
    return CommandResult<string>.Success(pipedChunk.ToUpper());
}
```

**After**:
```csharp
public override IResult<string> HandlePipedChunk(
    IResult<string> pipedChunk, 
    Dictionary<string, IParameterValue> parameters, 
    IEnvironmentContext env)
{
    var input = pipedChunk.Output ?? string.Empty;
    return CommandResult<string>.Success(input.ToUpper());
}
```
