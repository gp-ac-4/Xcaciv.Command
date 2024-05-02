using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace zTestCommandPackage
{
    public class EchoCommand : ICommand
    {
        public string BaseCommand => "ECHO";

        public string FriendlyName => "echo";

        public ValueTask DisposeAsync()
        {
            // nothing to dispose
            return ValueTask.CompletedTask;
        }

        public Task Help(ITextIoContext messageContext)
        {
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<string> Main(IInputContext input, IStatusContext statusContext)
        {
            await statusContext.SetStatusMessage("ECHO test start");
            foreach (var p in input.Parameters)
            {
                yield return $"{p}";
            }
            await statusContext.SetStatusMessage("ECHO test end");
        }
    }
}
