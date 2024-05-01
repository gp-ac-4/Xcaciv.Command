using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    public class DeligateCommand : ICommand
    {
        public DeligateCommand(string command, Func<string[], IAsyncEnumerable<string>> commandFunction)
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

        protected Func<string[], IAsyncEnumerable<string>>? commandFunction { get; set; }

        public string BaseCommand { get; }

        public string FriendlyName => BaseCommand;

        async IAsyncEnumerable<string> ICommand.Main(string[] parameters, ITextIoContext messageContext)
        {
            if (this.commandFunction != null)
            {
                await foreach (var p in this.commandFunction(parameters))
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
            await messageContext.OutputLine("Deligate Command, no help available.");
            return;
        }

    } 
}
