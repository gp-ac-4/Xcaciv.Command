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
        await ExecuteAsync(commandLine, ioContext, environmentContext,
            async (cmd, ctx, env, ct) => await executeCommand(cmd, ctx, env).ConfigureAwait(false),
            CancellationToken.None).ConfigureAwait(false);
    }

    public async Task ExecuteAsync(
        string commandLine,
        IIoContext ioContext,
        IEnvironmentContext environmentContext,
        Func<string, IIoContext, IEnvironmentContext, CancellationToken, Task> executeCommand,
        CancellationToken cancellationToken)
    {
        if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));
        if (ioContext == null) throw new ArgumentNullException(nameof(ioContext));
        if (environmentContext == null) throw new ArgumentNullException(nameof(environmentContext));
        if (executeCommand == null) throw new ArgumentNullException(nameof(executeCommand));

        cancellationToken.ThrowIfCancellationRequested();

        var (tasks, outputChannel) = await CreatePipelineStages(commandLine, ioContext, environmentContext, executeCommand, cancellationToken).ConfigureAwait(false);
        await Task.WhenAll(tasks).ConfigureAwait(false);

        await CollectPipelineOutput(outputChannel, ioContext).ConfigureAwait(false);
    }

    private async Task<(List<Task>, Channel<string>?)> CreatePipelineStages(
        string commandLine,
        IIoContext ioContext,
        IEnvironmentContext environmentContext,
        Func<string, IIoContext, IEnvironmentContext, CancellationToken, Task> executeCommand,
        CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();
        Channel<string>? pipeChannel = null;

        // Use formal parser to handle quoted arguments, escapes, and delimiters
        var commands = PipelineParser.ParsePipeline(commandLine);
        var totalStages = commands.Length;
        var currentStage = 1;

        foreach (var command in commands)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var commandName = CommandDescription.GetValidCommandName(command).ToString();
            var args = CommandDescription.GetArgumentsFromCommandline(command);
            var childContext = await ioContext.GetChild(args).ConfigureAwait(false);

            // Set pipeline stage metadata for audit logging
            childContext.SetPipelineStage(currentStage, totalStages);

            if (pipeChannel != null)
            {
                childContext.SetInputPipe(pipeChannel.Reader);
            }

            pipeChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(Configuration.MaxChannelQueueSize)
            {
                FullMode = GetChannelFullMode(Configuration.BackpressureMode)
            });
            childContext.SetOutputPipe(pipeChannel.Writer);

            tasks.Add(RunStageAsync(commandName, childContext, environmentContext, executeCommand, Configuration, cancellationToken));
            currentStage++;
        }

        return (tasks, pipeChannel);
    }

    private static Task RunStageAsync(
        string commandName,
        IIoContext childContext,
        IEnvironmentContext environmentContext,
        Func<string, IIoContext, IEnvironmentContext, CancellationToken, Task> executeCommand,
        PipelineConfiguration configuration,
        CancellationToken cancellationToken)
    {
        return RunStageInternal();

        async Task RunStageInternal()
        {
            await childContext.AddTraceMessage($"Pipeline stage start: {commandName}").ConfigureAwait(false);
            await using (childContext)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Create stage-specific timeout if configured
                using var stageCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                if (configuration.StageTimeoutSeconds > 0)
                {
                    stageCts.CancelAfter(TimeSpan.FromSeconds(configuration.StageTimeoutSeconds));
                }

                try
                {
                    await executeCommand(commandName, childContext, environmentContext, stageCts.Token).ConfigureAwait(false);
                    await childContext.Complete(null).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (stageCts.Token.IsCancellationRequested)
                {
                    // Determine if this was a stage timeout or parent cancellation
                    if (!cancellationToken.IsCancellationRequested && configuration.StageTimeoutSeconds > 0)
                    {
                        // Stage timeout occurred
                        await childContext.AddTraceMessage($"Pipeline stage timeout: {commandName} (exceeded {configuration.StageTimeoutSeconds}s)").ConfigureAwait(false);
                        await childContext.Complete($"Stage '{commandName}' exceeded timeout of {configuration.StageTimeoutSeconds} seconds").ConfigureAwait(false);
                    }
                    else
                    {
                        // Parent cancellation - complete gracefully without error message
                        await childContext.AddTraceMessage($"Pipeline stage cancelled: {commandName}").ConfigureAwait(false);
                        await childContext.Complete(null).ConfigureAwait(false);
                        throw; // Re-throw to propagate cancellation
                    }
                }
            }
        }
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
