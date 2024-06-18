using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace zTestCommandPackage
{
    [CommandRoot("do", "does stuff")]
    [CommandRegister("SAY", "a funny test sub command like echo", Prototype ="do say <some text>")]
    public class DoSayCommand : AbstractCommand
    {
        public override string HandleExecution(string[] parameters, IEnvironmentContext env)
        {
            // var justParameters = parameters.Skip(1).ToArray();
            return String.Join(' ', parameters);
        }

        public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env)
        {
            // var justParameters = parameters.Skip(1).ToArray();
            return pipedChunk + String.Join(' ', parameters);
        }
    }
}
