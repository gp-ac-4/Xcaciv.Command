using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command.Commands
{
    public abstract class AbstractCommand : Xcaciv.Command.Interface.ICommandDelegate
    {
        public abstract string BaseCommand { get; }

        public abstract string FriendlyName { get; }

        public abstract string HelpString { get; }

        /// <summary>
        /// this should be overwritten to dispose of any unmanaged items
        /// </summary>
        /// <returns></returns>
        public virtual ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public virtual void Help(IOutputContext outputContext)
        {
            outputContext.OutputChunk($"[{BaseCommand}] ({FriendlyName}): {HelpString}");
        }

        public async IAsyncEnumerable<string> Main(IInputContext input, IEnvironment environment)
        {
            if (input.HasPipedInput)
            {
                await foreach (var p in input.ReadInputPipeChunks())
                {
                    if (string.IsNullOrEmpty(p)) continue;
                    yield return this.HandlePipedChunk(p, input.Parameters, environment);
                }
            }
            else
            {
                yield return HandleExecution(input.Parameters, environment);
            }
        }

        public abstract string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironment env);

        public abstract string HandleExecution(string[] parameters, IEnvironment env);

    }
}
