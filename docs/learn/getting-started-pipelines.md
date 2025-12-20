# Using Pipelines

Learn how to chain multiple commands together using the pipeline feature.

## Pipeline Basics

Pipelines allow you to connect multiple commands using the `|` character. Output from one command becomes input to the next.

```csharp
// Simple pipeline
await controller.Run("SAY Hello World | REGIF World", ioContext, env);
```

Output:
```
Hello World
```

(The `REGIF` command filters by regex, keeping only lines matching "World")

## Pipeline Architecture

```
Input  ?  Command 1  ?  Channel  ?  Command 2  ?  Channel  ?  Output
           (produces)    (pipes)     (consumes)   (produces)
```

Each command stage:
1. Reads from input (user args or previous pipe)
2. Produces output chunks
3. Sends to output channel
4. Next stage receives and processes

## Building Pipeline-Aware Commands

### Consuming Piped Input

Commands can read output from previous commands:

```csharp
[CommandRegister("UPPERCASE", "Convert input to uppercase")]
public class UppercaseCommand : AbstractCommand
{
    public override async IAsyncEnumerable<IResult<string>> Main(
        IIoContext ioContext, 
        IEnvironmentContext env)
    {
        // Check if we have piped input
        if (ioContext.HasPipedInput)
        {
            // Read from previous command in pipeline
            await foreach (var chunk in ioContext.ReadInputPipeChunks())
            {
                yield return CommandResult<string>.Success(chunk.ToUpper());
            }
        }
        else
        {
            // Standalone execution
            var parameters = ioContext.Parameters;
            if (parameters.Length > 0)
            {
                yield return CommandResult<string>.Success(parameters[0].ToUpper());
            }
        }
    }
}
```

### Producing Output for Piping

All commands that override `Main()` automatically support piping. The framework writes each `yield return` to the output channel:

```csharp
[CommandRegister("LINES", "Output multiple lines")]
public class LinesCommand : AbstractCommand
{
    public override async IAsyncEnumerable<IResult<string>> Main(
        IIoContext ioContext, 
        IEnvironmentContext env)
    {
        // Each yield creates a separate output chunk
        yield return CommandResult<string>.Success("Line 1");
        yield return CommandResult<string>.Success("Line 2");
        yield return CommandResult<string>.Success("Line 3");
    }
}
```

Usage:
```csharp
await controller.Run("LINES | UPPERCASE", ioContext, env);
```

Output:
```
LINE 1
LINE 2
LINE 3
```

## Examples

### Filter Lines by Regular Expression

```csharp
[CommandRegister("REGIF", "Filter lines matching regex")]
public class RegexFilterCommand : AbstractCommand
{
    [CommandParameterOrdered("pattern", "Regular expression pattern")]
    public string Pattern { get; set; }

    public override async IAsyncEnumerable<IResult<string>> Main(
        IIoContext ioContext, 
        IEnvironmentContext env)
    {
        var regex = new Regex(Pattern);
        
        await foreach (var line in ioContext.ReadInputPipeChunks())
        {
            if (regex.IsMatch(line))
            {
                yield return CommandResult<string>.Success(line);
            }
        }
    }
}
```

Usage:
```csharp
// Get environment variables and filter for PATH
await controller.Run("ENV | REGIF PATH", ioContext, env);
```

### Transform Each Line

```csharp
[CommandRegister("PREFIX", "Add prefix to each line")]
public class PrefixCommand : AbstractCommand
{
    [CommandParameterOrdered("prefix", "Text to prepend")]
    public string Prefix { get; set; }

    public override async IAsyncEnumerable<IResult<string>> Main(
        IIoContext ioContext, 
        IEnvironmentContext env)
    {
        await foreach (var line in ioContext.ReadInputPipeChunks())
        {
            yield return CommandResult<string>.Success(Prefix + line);
        }
    }
}
```

Usage:
```csharp
await controller.Run("SAY line1 | SAY line2 | PREFIX [INFO] ", ioContext, env);
```

### Count Lines

```csharp
[CommandRegister("LINECOUNT", "Count input lines")]
public class LineCountCommand : AbstractCommand
{
    public override async IAsyncEnumerable<IResult<string>> Main(
        IIoContext ioContext, 
        IEnvironmentContext env)
    {
        int count = 0;
        
        await foreach (var line in ioContext.ReadInputPipeChunks())
        {
            count++;
        }
        
        yield return CommandResult<string>.Success($"Total lines: {count}");
    }
}
```

Usage:
```csharp
// Count how many commands are available
await controller.Run("HELP | LINECOUNT", ioContext, env);
```

### Accumulate Output

```csharp
[CommandRegister("JOIN", "Join lines with separator")]
public class JoinCommand : AbstractCommand
{
    [CommandParameterNamed("sep", "Separator between lines")]
    public string Separator { get; set; } = ", ";

    public override async IAsyncEnumerable<IResult<string>> Main(
        IIoContext ioContext, 
        IEnvironmentContext env)
    {
        var lines = new List<string>();
        
        await foreach (var line in ioContext.ReadInputPipeChunks())
        {
            lines.Add(line);
        }
        
        yield return CommandResult<string>.Success(string.Join(Separator, lines));
    }
}
```

Usage:
```csharp
await controller.Run("LINES | JOIN --sep ; ", ioContext, env);
```

Output:
```
Line 1; Line 2; Line 3
```

## Pipeline Configuration

Control pipeline behavior via `PipelineConfiguration`:

```csharp
var executor = new PipelineExecutor();

// Maximum items to queue between stages
executor.Configuration.MaxChannelQueueSize = 100;

// How to handle queue overflow
executor.Configuration.BackpressureMode = PipelineBackpressureMode.Block;

// Timeout for individual pipeline stages
executor.Configuration.StageTimeoutSeconds = 30;
```

### Backpressure Modes

| Mode | Behavior |
|------|----------|
| `Block` | Wait when queue is full (prevents data loss) |
| `DropOldest` | Remove oldest item when queue is full |
| `DropNewest` | Remove newest item when queue is full |

Use `Block` (default) for reliability. Use `DropOldest` or `DropNewest` for high-throughput scenarios where losing data is acceptable.

## Error Handling in Pipelines

### Propagating Errors

Return failure results to signal errors:

```csharp
[CommandRegister("VALIDATE", "Validate input format")]
public class ValidateCommand : AbstractCommand
{
    [CommandParameterOrdered("format", "Expected format")]
    public string Format { get; set; }

    public override async IAsyncEnumerable<IResult<string>> Main(
        IIoContext ioContext, 
        IEnvironmentContext env)
    {
        await foreach (var line in ioContext.ReadInputPipeChunks())
        {
            if (IsValidFormat(line, Format))
            {
                yield return CommandResult<string>.Success(line);
            }
            else
            {
                yield return CommandResult<string>.Failure($"Invalid format: {line}");
            }
        }
    }

    private bool IsValidFormat(string line, string format)
    {
        // Validation logic
        return true;
    }
}
```

### Stopping the Pipeline

Throw an exception to stop the pipeline immediately:

```csharp
public override async IAsyncEnumerable<IResult<string>> Main(
    IIoContext ioContext, 
    IEnvironmentContext env)
{
    try
    {
        await foreach (var line in ioContext.ReadInputPipeChunks())
        {
            if (line.Contains("STOP"))
            {
                throw new InvalidOperationException("Stop keyword found");
            }
            
            yield return CommandResult<string>.Success(line);
        }
    }
    catch (Exception ex)
    {
        yield return CommandResult<string>.Failure(ex.Message);
    }
}
```

## Advanced Patterns

### Conditional Piping

```csharp
// Only use UPPERCASE if --upper flag is present
if (flags.Contains("upper"))
{
    await controller.Run("LINES | UPPERCASE", ioContext, env);
}
else
{
    await controller.Run("LINES", ioContext, env);
}
```

### Multiple Pipelines in Sequence

```csharp
// First pipeline
await controller.Run("SAY file1.txt | PREFIX [FILE1] ", ioContext, env);

// Clear output and run second pipeline
ioContext.Clear();

await controller.Run("SAY file2.txt | PREFIX [FILE2] ", ioContext, env);
```

### Pipeline with Named Parameters

```csharp
await controller.Run(
    "LINES | REGIF ^[A-Z] | PREFIX [MATCH] --verbose",
    ioContext,
    env);
```

## Performance Considerations

### Large Data Streams

For streaming large amounts of data:

1. **Use appropriate queue size**: Balance memory vs throughput
2. **Prefer Block backpressure**: Prevents overwhelming stages
3. **Add timeouts**: Detect stuck stages early
4. **Implement early exit**: Stop processing when not needed

```csharp
executor.Configuration.MaxChannelQueueSize = 1000;
executor.Configuration.BackpressureMode = PipelineBackpressureMode.Block;
executor.Configuration.StageTimeoutSeconds = 60;
```

### Memory-Efficient Processing

Process lines as they arrive instead of accumulating:

```csharp
// Good: Stream-based (memory efficient)
public override async IAsyncEnumerable<IResult<string>> Main(
    IIoContext ioContext, 
    IEnvironmentContext env)
{
    await foreach (var line in ioContext.ReadInputPipeChunks())
    {
        // Process immediately
        var result = ProcessLine(line);
        yield return CommandResult<string>.Success(result);
    }
}

// Avoid: Accumulation (memory intensive for large inputs)
public override async IAsyncEnumerable<IResult<string>> Main(
    IIoContext ioContext, 
    IEnvironmentContext env)
{
    var allLines = new List<string>();
    await foreach (var line in ioContext.ReadInputPipeChunks())
    {
        allLines.Add(line); // Bad: Accumulates everything in memory
    }
    
    var results = ProcessAll(allLines);
    foreach (var result in results)
    {
        yield return CommandResult<string>.Success(result);
    }
}
```

## Next Steps

- [Advanced Command Patterns](advanced-command-patterns.md)
- [Pipeline Configuration Reference](api-pipeline-configuration.md)
- [IIoContext Interface](api-interfaces.md#iiocont
ext)
