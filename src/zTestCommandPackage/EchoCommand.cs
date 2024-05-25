using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace zTestCommandPackage
{
    [BaseCommand("ECHO", "ECHO")]
    public class EchoCommand : ICommandDelegate
    {
        public string BaseCommand { get; protected set; } = "ECHO";

        public string FriendlyName { get; protected set; } = "echo";

        public void Help(IIoContext outputContext)
        {
            outputContext.OutputChunk($"[{BaseCommand}] ({FriendlyName}) - test command to output each parameter as a chunk");
        }

        public async IAsyncEnumerable<string> Main(IIoContext io, IEnvironmentContext statusContext)
        {
            await io.AddTraceMessage($"{this.BaseCommand} test start");
            if (io.HasPipedInput)
            {
                await foreach (var p in io.ReadInputPipeChunks())
                    yield return this.FormatEcho(p);
            }
            else
            {
                foreach (var p in io.Parameters)
                {
                    yield return this.FormatEcho(p);
                }
            }
            await io.AddTraceMessage($"{this.BaseCommand} test end");
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

        public void OneLineHelp(IIoContext context)
        {
            context.OutputChunk($"{BaseCommand} - {FriendlyName}");
        }
    }
}
