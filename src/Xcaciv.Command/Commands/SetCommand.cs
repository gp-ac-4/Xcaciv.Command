using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command.Commands
{
    [CommandRegister("Set", "Set environment values", Prototype = "SET <varname> <value>")]
    [CommandParameterOrdered("Key", "Key used to access value")]
    [CommandParameterOrdered("Value", "Value stored for accessing")]
    [CommandHelpRemarks("This is a special command that is able to modify the Env outside its own context.")]
    internal class SetCommand : AbstractCommand
    {
        public override string HandleExecution(string[] parameters, IEnvironmentContext env)
        {
            var setPair = this.ProcessParameters(parameters);


            if (!String.IsNullOrEmpty(setPair["key"]) && !String.IsNullOrEmpty(setPair["value"]))
            {
                env.SetValue(setPair["key"], setPair["value"]);
            }
            // nothing to display
            return String.Empty;
        }

        public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext status)
        {
            throw new NotImplementedException();
        }
    }
}
