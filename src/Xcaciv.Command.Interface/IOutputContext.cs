using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    public interface IOutputContext
    {
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
    }
}
