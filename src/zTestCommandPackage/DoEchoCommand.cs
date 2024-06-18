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
    [CommandRegister("ECHO", "SUB DO echo")]
    public class DoEchoCommand : AbstractCommand, ICommandDelegate
    {
        public override string HandleExecution(string[] parameters, IEnvironmentContext env)
        {
            return String.Join(' ', parameters);
        }

        public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env)
        {
            return pipedChunk;
        }
    }
}
