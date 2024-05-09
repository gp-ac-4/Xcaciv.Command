using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    public class DeligateCommand : ICommandDelegate
    {
        public DeligateCommand(string command, Func<IInputContext, IAsyncEnumerable<string>> commandFunction)
        {
            this.BaseCommand = command;
            this.commandFunction = commandFunction;
        }
        public Func<ValueTask>? Dispose { get; set; }
        public ValueTask DisposeAsync()
        {
            if (Dispose != null) return Dispose();
            return ValueTask.CompletedTask;
        }

        protected Func<IInputContext, IAsyncEnumerable<string>>? commandFunction { get; set; }

        public string BaseCommand { get; }

        public string FriendlyName => BaseCommand;
        /// <summary>
        /// primary command execution method
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        async IAsyncEnumerable<string> ICommandDelegate.Main(IInputContext input, IStatusContext statusContext)
        {
            if (this.commandFunction != null)
            {
                await foreach (var p in this.commandFunction(input))
                {
                    yield return p;
                }
            }
            else
            {
                yield break;
            }
        }

        public async Task Help(ITextIoContext messageContext)
        {
            await messageContext.OutputChunk("Deligate Command, no help available.");
            return;
        }

    } 
}
