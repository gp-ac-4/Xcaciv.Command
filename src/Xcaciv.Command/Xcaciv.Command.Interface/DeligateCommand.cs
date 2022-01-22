using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    public class DeligateCommand : ICommand
    {
        public DeligateCommand(string command, Func<string[], Task<string>> commandFunction)
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

        protected Func<string[], Task<string>>? commandFunction { get; set; }

        public string BaseCommand { get; }

        public string FriendlyName => BaseCommand;

        public Task<string> Main(string[] parameters, ITextIoContext outputMesser)
        {
            if (this.commandFunction != null) return this.commandFunction(parameters);
            return Task.FromResult(String.Empty);
        }

        public async Task Help(ITextIoContext messageContext)
        {
            await messageContext.WriteLine("Deligate Command, no help available.");
            return;
        }
    } 
}
