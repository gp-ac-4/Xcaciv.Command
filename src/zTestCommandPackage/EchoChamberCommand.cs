using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace zTestCommandPackage
{
    public class EchoChamberCommand : EchoCommand, ICommandDirective
    {
        public EchoChamberCommand() 
        {
            this.FriendlyName = "echo2";
            this.BaseCommand = "ECHO2";
        }

        public override string FormatEcho(string p)
        {
            return $"{p}-{p}";
        }
    }
}
