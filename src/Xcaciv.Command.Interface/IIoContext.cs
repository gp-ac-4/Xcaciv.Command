using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    /// <summary>
    /// Abstraction for command input/output and context management.
    /// Decouples command execution from specific UI implementations.
    /// </summary>
    /// <remarks>
    /// The IIoContext interface provides:
    /// - Parameter management for command arguments
    /// - Pipeline support via ChannelReader/ChannelWriter
    /// - Output handling (standard output, status messages, trace logging)
    /// - Progress tracking for long-running operations
    /// 
    /// Implementations must be thread-safe and prioritize pipeline output
    /// when channels are available. All output methods are asynchronous
    /// to support non-blocking I/O operations.
    /// 
    /// Security: Parameters are validated by the framework; commands should
    /// trust parameter content.
    /// </remarks>
    public partial interface IIoContext: ICommandContext<IIoContext>
    {
        /// <summary>
        /// Indicates whether this context has input available from a pipeline.
        /// </summary>
        /// <value>true if this command is receiving input from a previous command in a pipeline; false otherwise.</value>
        /// <remarks>
        /// When true, commands should read input via ReadInputPipeChunks()
        /// instead of using PromptForCommand().
        /// </remarks>
        bool HasPipedInput { get; }

        /// <summary>
        /// Gets the command-line arguments for this command.
        /// </summary>
        /// <value>Array of command parameters (may be empty if no parameters provided).</value>
        /// <remarks>
        /// Parameters have been validated by the framework before reaching the command.
        /// Commands use parameter attributes to define expected parameters.
        /// </remarks>
        string[] Parameters { get; }

        /// <summary>
        /// Sets the input channel for reading piped output from a previous command.
        /// </summary>
        /// <param name="reader">The ChannelReader to read piped input from.</param>
        /// <remarks>
        /// Called by the framework when setting up a piped command sequence.
        /// Implementations should store this reader for use in ReadInputPipeChunks().
        /// </remarks>
        void SetInputPipe(ChannelReader<IResult<string>> reader);

        /// <summary>
        /// Asynchronously reads all output chunks from the input pipe.
        /// </summary>
        /// <returns>An async enumerable yielding output strings from the previous command.</returns>
        /// <remarks>
        /// Called by commands that accept piped input. Each string is a separate
        /// output chunk from the previous command.
        /// Blocks until input is available or the pipe is closed.
        /// </remarks>
        IAsyncEnumerable<IResult<string>> ReadInputPipeChunks();

        /// <summary>
        /// Prompts the user for a command via the configured input mechanism.
        /// </summary>
        /// <param name="prompt">The prompt text to display to the user.</param>
        /// <returns>The command text entered by the user.</returns>
        /// <remarks>
        /// Only used when this context does not have piped input (HasPipedInput = false).
        /// Implementation depends on UI context (console, web, etc.).
        /// </remarks>
        Task<string> PromptForCommand(string prompt);

        /// <summary>
        /// Sets the output channel for writing output to the next command in a pipeline.
        /// </summary>
        /// <param name="writer">The ChannelWriter to send output to the next command.</param>
        /// <remarks>
        /// Called by the framework when setting up a piped command sequence.
        /// Implementations should write OutputChunk() data to this writer when available.
        /// </remarks>
        void SetOutputPipe(ChannelWriter<IResult<string>> writer);

        /// <summary>
        /// Outputs a chunk of command result data.
        /// </summary>
        /// <param name="message">The output text to send.</param>
        /// <returns>A task representing the asynchronous output operation.</returns>
        /// <remarks>
        /// The implementation prioritizes pipeline output (ChannelWriter) when available,
        /// otherwise outputs to the default output (console, UI buffer, etc.).
        /// Each OutputChunk() call represents one discrete unit of output.
        /// </remarks>
        Task OutputChunk(IResult<string> message);

        /// <summary>
        /// Sets the current status message (replaces previous status).
        /// </summary>
        /// <param name="message">The status message to display.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Used for displaying transient status (not command output).
        /// Should NOT contain actual command results—use OutputChunk() for that.
        /// Example use: "Processing item 5 of 10" during long operations.
        /// </remarks>
        Task SetStatusMessage(string message);

        /// <summary>
        /// Adds a trace message for diagnostic/logging purposes.
        /// </summary>
        /// <param name="message">The trace message to record.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Trace messages are intended for developers and advanced users for troubleshooting.
        /// Not normally shown in standard output; typically logged or output in verbose mode.
        /// Use for exception details, internal state info, security events, etc.
        /// </remarks>
        Task AddTraceMessage(string message);

        /// <summary>
        /// Updates progress for a long-running operation.
        /// </summary>
        /// <param name="total">Total number of items/steps to process.</param>
        /// <param name="step">Current step/item number (0 to total).</param>
        /// <returns>A task that returns the computed percentage complete (0-100).</returns>
        /// <remarks>
        /// Used to provide progress feedback during long operations.
        /// Example: SetProgress(100, 25) would return 25 (percent complete).
        /// </remarks>
        Task<int> SetProgress(int total, int step);

        /// <summary>
        /// Signals that the operation is complete.
        /// </summary>
        /// <param name="message">Optional completion message to display.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Called after command execution completes (success or failure).
        /// Implementations may use this to finalize output, close pipes, etc.
        /// </remarks>
        Task Complete(string? message);

        /// <summary>
        /// Replaces the current parameter array with a new set of parameters.
        /// </summary>
        /// <param name="parameters">The new parameter array.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Used internally by the framework when handling sub-commands.
        /// Sub-command name is removed from parameters before calling the sub-command.
        /// </remarks>
        Task SetParameters(string[] parameters);

        /// <summary>
        /// Sets the output encoder for encoding command output.
        /// </summary>
        /// <param name="encoder">The IOutputEncoder to apply to all output chunks.</param>
        /// <remarks>
        /// Called by the framework to propagate the output encoder from CommandController.
        /// The encoder is applied to each OutputChunk() call before the output is displayed or piped.
        /// 
        /// This is optional; if not called, the context uses the default encoder (typically NoOpEncoder).
        /// </remarks>
        void SetOutputEncoder(IOutputEncoder encoder);

        /// <summary>
        /// Gets the pipeline stage number (1-based) if this command is part of a pipeline.
        /// Returns null for standalone command execution.
        /// </summary>
        int? PipelineStage { get; }

        /// <summary>
        /// Gets the total number of stages in the pipeline if this command is part of one.
        /// Returns null for standalone command execution.
        /// </summary>
        int? PipelineTotalStages { get; }

        /// <summary>
        /// Sets pipeline stage metadata for audit logging and diagnostics.
        /// </summary>
        /// <param name="stage">The 1-based stage number.</param>
        /// <param name="totalStages">The total number of stages in the pipeline.</param>
        void SetPipelineStage(int stage, int totalStages);
    }
}
