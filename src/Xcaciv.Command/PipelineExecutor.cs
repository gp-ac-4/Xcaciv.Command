using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command;

/// <summary>
/// Default pipeline executor that builds bounded channels between command stages.
/// </summary>
public class PipelineExecutor : IPipelineExecutor
{
    public PipelineConfiguration Configuration { get; set; } = new PipelineConfiguration();

    public async Task ExecuteAsync(
        string commandLine,
        IIoContext ioContext,
        IEnvironmentContext environmentContext,
        Func<string, IIoContext, IEnvironmentContext, Task> executeCommand)
    {
        if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));
        if (ioContext == null) throw new ArgumentNullException(nameof(ioContext));
        if (environmentContext == null) throw new ArgumentNullException(nameof(environmentContext));
        if (executeCommand == null) throw new ArgumentNullException(nameof(executeCommand));

        var (tasks, outputChannel) = await CreatePipelineStages(commandLine, ioContext, environmentContext, executeCommand).ConfigureAwait(false);
        await Task.WhenAll(tasks).ConfigureAwait(false);
        await CollectPipelineOutput(outputChannel, ioContext).ConfigureAwait(false);
    }

    private async Task<(List<Task>, Channel<string>?)> CreatePipelineStages(
        string commandLine,
        IIoContext ioContext,
        IEnvironmentContext environmentContext,
        Func<string, IIoContext, IEnvironmentContext, Task> executeCommand)
    {
        var tasks = new List<Task>();
        Channel<string>? pipeChannel = null;

        foreach (var command in commandLine.Split('|'))
        {
            var commandName = CommandDescription.GetValidCommandName(command).ToString();
            var args = CommandDescription.GetArgumentsFromCommandline(command);
            await using var childContext = await ioContext.GetChild(args).ConfigureAwait(false);

            if (pipeChannel != null)
            {
                childContext.SetInputPipe(pipeChannel.Reader);
            }

            pipeChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(Configuration.MaxChannelQueueSize)
            {
                FullMode = GetChannelFullMode(Configuration.BackpressureMode)
            });
            childContext.SetOutputPipe(pipeChannel.Writer);

            tasks.Add(executeCommand(commandName, childContext, environmentContext));
        }

        return (tasks, pipeChannel);
    }

    private async Task CollectPipelineOutput(Channel<string>? outputChannel, IIoContext ioContext)
    {
        if (outputChannel == null)
        {
            outputChannel = Channel.CreateBounded<string>(0);
        }

        await foreach (var output in outputChannel.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            await ioContext.OutputChunk(output).ConfigureAwait(false);
        }
    }

    private static BoundedChannelFullMode GetChannelFullMode(PipelineBackpressureMode mode) => mode switch
    {
        PipelineBackpressureMode.DropOldest => BoundedChannelFullMode.DropOldest,
        PipelineBackpressureMode.DropNewest => BoundedChannelFullMode.DropNewest,
        PipelineBackpressureMode.Block => BoundedChannelFullMode.Wait,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported backpressure mode")
    };
}
