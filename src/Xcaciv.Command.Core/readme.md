# Xcaciv.Command.Core

Core abstractions for Xcaciv Command: base command orchestration and IO pipeline helpers.

## AbstractCommand

`AbstractCommand` implements `ICommandDelegate` and wraps help generation, parameter parsing, and piped execution. Derive from it to implement `HandleExecution` (non-piped) and `HandlePipedChunk` (piped) while leveraging attribute-driven parameters.

```csharp
using System.Collections.Generic;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;

[CommandRegister("Echo", "Echo text", Prototype = "ECHO <text...>")]
[CommandParameterSuffix("text", "Text to echo")]
internal sealed class EchoCommand : AbstractCommand
{
    public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        var text = parameters.TryGetValue("text", out var t) && t.IsValid ? t.GetValue<string>() : string.Empty;
        return CommandResult<string>.Success(text, OutputFormat);
    }

    public override IResult<string> HandlePipedChunk(IResult<string> pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
    {
        // v3.2.3+: pipedChunk is now IResult<string> - access output via pipedChunk.Output
        var input = pipedChunk.Output ?? string.Empty;
        return CommandResult<string>.Success(input, OutputFormat);
    }
}
```

**Important (v3.2.3+):** `HandlePipedChunk` now accepts `IResult<string>` instead of `string`, providing access to the full result context including success status, error messages, and metadata from upstream commands.

Key behaviors:
- Uses `ProcessParameters` to map ordered, named, flag, and suffix attributes into typed `IParameterValue` entries.
- Handles `--help` via `Help`/`OneLineHelp` when `AbstractCommand.SetHelpService` is configured.
- Provides `OnStartPipe` and `OnEndPipe` hooks for initializing and finalizing state during pipeline execution.

## AbstractTextIo

`AbstractTextIo` is a base `IIoContext` implementation for text-oriented pipelines. It manages piped input/output channels, progress/status reporting, and optional verbose trace emission.

```csharp
using System;
using System.Threading.Tasks;
using System.Threading.Channels;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;

internal sealed class MemoryIoContext(string name, string[] args, Guid? parentId = null) : AbstractTextIo(name, args, parentId)
{
    public override Task<IIoContext> GetChild(string[]? childArguments = null)
    {
        return Task.FromResult<IIoContext>(new MemoryIoContext(Name + "-child", childArguments ?? Array.Empty<string>(), Id));
    }

    public override Task HandleOutputChunk(IResult<string> result)
    {
        Console.WriteLine(result.Output);
        return Task.CompletedTask;
    }

    public override Task<string> PromptForCommand(string prompt)
    {
        Console.Write(prompt);
        return Task.FromResult(Console.ReadLine() ?? string.Empty);
    }

    public override Task<int> SetProgress(int total, int step)
    {
        Console.WriteLine($"Progress: {step}/{total}");
        return Task.FromResult(step);
    }

    public override Task SetStatusMessage(string message)
    {
        Console.WriteLine($"Status: {message}");
        return Task.CompletedTask;
    }
}
```

Highlights:
- `SetInputPipe`/`SetOutputPipe` wire the command pipeline; `ReadInputPipeChunks` yields piped `IResult<string>` items.
- `OutputChunk` routes output through the pipeline writer when present or directly to `HandleOutputChunk` otherwise.
- `AddTraceMessage` honors `Verbose` mode and emits trace messages to the output or to diagnostics.

