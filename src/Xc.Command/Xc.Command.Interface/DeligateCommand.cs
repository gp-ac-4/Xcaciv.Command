using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xc.Command.Interface
{
    public class DeligateCommand : ICommand
    {
        public DeligateCommand(Func<Dictionary<string, string>, Task<string>> command)
        {
            this.command = command;
        }
        public Func<ValueTask>? Dispose { get; set; }
        public ValueTask DisposeAsync()
        {
            if (Dispose != null) return Dispose();
            return ValueTask.CompletedTask;
        }

        protected Func<Dictionary<string, string>, Task<string>>? command { get; set; }

        public Task<string> Operate(Dictionary<string, string> parameters, IOutputMessageContext outputMesser)
        {
            if (this.command != null) return this.command(parameters);
            return Task.FromResult(String.Empty);
        }
    } 
}
