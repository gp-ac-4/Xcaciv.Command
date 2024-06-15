using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    /// <summary>
    /// String pump for UI syncronization context.
    /// This interface is used to abstract the UI from the command processor.
    /// Implementations shoudl be thread safe. They should handle pipeline input and 
    /// output via ChannelReader and ChannelWriter. The implementation should default
    /// to a particular bound output, but should prioritise the pipeline when channels
    /// are available.
    /// </summary>
    public partial interface IIoContext: ICommandContext<IIoContext>
    {
        /// <summary>
        /// signals the presense of pipeline input
        /// </summary>
        bool HasPipedInput { get; }
        /// <summary>
        /// command parameter list
        /// </summary>
        string[] Parameters { get; set; }
        /// <summary>
        /// when running in a pipeline this will be the pipe that the command will read from
        /// </summary>
        /// <param name="reader"></param>
        void SetInputPipe(ChannelReader<string> reader);
        /// <summary>
        /// when in a pipeline, read input from the pipe
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<string> ReadInputPipeChunks();
        /// <summary>
        /// get command text from context
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        Task<string> PromptForCommand(string prompt);
        /// <summary>
        /// when running in a pipeline this will be the pipe that the command will write to
        /// the implementation should write what is passed to OutputChunk() to this channel writer
        /// </summary>
        /// <param name="writer"></param>
        void SetOutputPipe(ChannelWriter<string> writer);
        /// <summary>
        /// output text used for cumulative standard output
        /// should prioritize pipeline output using the ChannelWriter
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task OutputChunk(string message);
        /// <summary>
        /// replace status text with new text
        /// used for static status output
        /// SHOULD NOT CONTAIN ACTUAL RESULT
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SetStatusMessage(string message);
        /// <summary>
        /// adds to the trace collection
        /// can be output to the screen when being verbose
        /// messages should help troubleshooting for developers and users
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task AddTraceMessage(string message);
        /// <summary>
        /// set proces progress based on total
        /// </summary>
        /// <param name="total"></param>
        /// <param name="step"></param>
        /// <returns>whole number signifying percentage</returns>
        Task<int> SetProgress(int total, int step);
        /// <summary>
        /// signal that the status is complete
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task Complete(string? message);
        /// <summary>
        /// overwrite the current parameters with a new array
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task SetParameters(string[] parameters);
    }
}
