using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command.Commands
{
    [BaseCommand("Set", "Set environment values", Prototype = "SET <varname> = <value>")]
    [CommandParameter("Key")]
    [CommandParameter("Value")]
    [CommandHelpRemarks("This is a special command that is able to modify the Env outside its own context.")]
    internal class SetCommand : AbstractCommand
    {
        public override string HandleExecution(string[] parameters, IEnvironment env)
        {
            var varName = String.Empty;
            var varValue = String.Empty;

            foreach (var token in parameters)
            {
                // skip assignment character
                if (token == "=") continue;
                if (String.IsNullOrEmpty(varName))
                {
                    // set case insenstive var name removing assignment character from ends
                    varName = token.Trim('=');
                }
                else if (String.IsNullOrEmpty(varValue))
                {
                    // set value removing assignment character from ends
                    varValue = token.Trim('=');
                }

                // when we get both, add it to the values
                if (!String.IsNullOrEmpty(varName) && !String.IsNullOrEmpty(varValue))
                {
                    env.SetValue(varName, varValue);
                    varName = String.Empty;
                    varValue = String.Empty;
                }
            }
            // nothing to display
            return String.Empty;
        }

        public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironment status)
        {
            throw new NotImplementedException();
        }
    }
}
