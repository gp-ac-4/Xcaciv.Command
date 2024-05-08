using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command
{
    public class DeligateCommand : ICommandDirective
    {
        public DeligateCommand(string command, Func<IInputContext, IAsyncEnumerable<string>> commandFunction)
        {
            BaseCommand = command;
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

        async IAsyncEnumerable<string> ICommandDirective.Main(IInputContext input, IStatusContext statusContext)
        {
            if (commandFunction != null)
            {
                await foreach (var p in commandFunction(input))
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
