using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace zTestCommandPackage
{
    [CommandRegister("PING", "PING")]
    [CommandParameterOrdered("echo_word", "ECHO")]
    [CommandParameterNamed("optional", "option", AllowedValues = ["1", "2"])]
    public class PingCommand : ICommandDelegate
    {
        public string BaseCommand { get; protected set; } = "ECHO";

        public string FriendlyName { get; protected set; } = "echo";

        public string Help(string[] parameters, IEnvironmentContext evn)
        {
            return $"[{BaseCommand}] ({FriendlyName}) - test command to output each parameter as a chunk";
        }

        public async IAsyncEnumerable<string> Main(IIoContext io, IEnvironmentContext statusContext)
        {
            await io.AddTraceMessage($"{this.BaseCommand} test start");
            if (io.HasPipedInput)
            {
                await foreach (var p in io.ReadInputPipeChunks())
                    yield return this.FormatEcho(p);
            }
            else if (io.Parameters[0].Equals("--HELP", StringComparison.CurrentCultureIgnoreCase))
            {
                yield return this.Help(io.Parameters, statusContext);
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

        public string OneLineHelp(string[] paramseters)
        {
            return $"{BaseCommand} - {FriendlyName}";
        }
    }
}
