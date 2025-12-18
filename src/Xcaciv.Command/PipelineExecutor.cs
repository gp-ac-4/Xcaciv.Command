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

        var (tasks, channels, outputChannel) = await CreatePipelineStages(commandLine, ioContext, environmentContext, executeCommand).ConfigureAwait(false);
        await Task.WhenAll(tasks).ConfigureAwait(false);
        
        // Complete all channel writers after tasks finish
        foreach (var channel in channels)
        {
            channel.Writer.Complete();
        }
        
        await CollectPipelineOutput(outputChannel, ioContext).ConfigureAwait(false);
    }

    private async Task<(List<Task>, List<Channel<string>>, Channel<string>?)> CreatePipelineStages(
        string commandLine,
        IIoContext ioContext,
        IEnvironmentContext environmentContext,
        Func<string, IIoContext, IEnvironmentContext, Task> executeCommand)
    {
        var tasks = new List<Task>();
        var channels = new List<Channel<string>>();
        Channel<string>? pipeChannel = null;

        foreach (var command in commandLine.Split('|'))
        {
            var commandName = CommandDescription.GetValidCommandName(command).ToString();
            var args = CommandDescription.GetArgumentsFromCommandline(command);
            var childContext = await ioContext.GetChild(args).ConfigureAwait(false);

            if (pipeChannel != null)
            {
                childContext.SetInputPipe(pipeChannel.Reader);
            }

            pipeChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(Configuration.MaxChannelQueueSize)
            {
                FullMode = GetChannelFullMode(Configuration.BackpressureMode)
            });
            channels.Add(pipeChannel);
            childContext.SetOutputPipe(pipeChannel.Writer);

            tasks.Add(executeCommand(commandName, childContext, environmentContext));
        }

        return (tasks, channels, pipeChannel);
    }

    private async Task CollectPipelineOutput(Channel<string>? outputChannel, IIoContext ioContext)
    {
        if (outputChannel == null)
        {
            return;
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
