using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace zTestCommandPackage
{
    public class EchoCommand : ICommandDelegate
    {
        public string BaseCommand { get; protected set; } = "ECHO";

        public string FriendlyName { get; protected set; } = "echo";

        public Task Help(ITextIoContext messageContext)
        {
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<string> Main(IInputContext input, IStatusContext statusContext)
        {
            await statusContext.SetStatusMessage($"{this.BaseCommand} test start");
            if (input.HasPipedInput)
            {
                await foreach (var p in input.ReadInputPipeChunks())
                    yield return this.FormatEcho(p);
            }
            else
            {
                foreach (var p in input.Parameters)
                {
                    yield return this.FormatEcho(p);
                }
            }
            await statusContext.SetStatusMessage($"{this.BaseCommand} test end");
        }

        public virtual string FormatEcho(string p)
        {
            return $"{p}";
        }

        public ValueTask DisposeAsync()
        {
            // nothing to dispose
            return ValueTask.CompletedTask;
        }
    }
}
