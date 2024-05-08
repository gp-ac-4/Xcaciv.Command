using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace zTestCommandPackage
{
    public class EchoEncodeCommand : EchoCommand, ICommandDirective
    {
        public EchoEncodeCommand()
        {
            this.FriendlyName = "echoe";
            this.BaseCommand = "ECHOE";
        }

        public override string FormatEcho(string p)
        {
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(p));
            return $":{encoded}:";
        }
    }
}
