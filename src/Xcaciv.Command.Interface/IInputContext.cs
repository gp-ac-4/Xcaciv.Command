using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    public interface IInputContext
    {
        /// <summary>
        /// signals the presense of pipeline input
        /// </summary>
        bool HasPipedInput { get; }
        /// <summary>
        /// command parameter list
        /// </summary>
        string[] Parameters { get; }
        /// <summary>
        /// when running in a pipeline this will be the pipe that the command will read from
        /// </summary>
        /// <param name="reader"></param>
        void setInputPipe(ChannelReader<string> reader);
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
    }
}
