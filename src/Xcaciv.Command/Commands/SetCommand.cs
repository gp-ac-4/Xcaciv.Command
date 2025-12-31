using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;

namespace Xcaciv.Command.Commands
{
    [CommandRegister("Set", "Set environment values", Prototype = "SET <varname> <value>")]
    [CommandParameterOrdered("Key", "Key used to access value")]
    [CommandParameterOrdered("Value", "Value stored for accessing")]
    [CommandHelpRemarks("This is a special command that is able to modify the Env outside its own context.")]
    internal class SetCommand : AbstractCommand
    {
        public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            var key = parameters.TryGetValue("key", out var keyParam) && keyParam.IsValid ? keyParam.GetValue<string>() : string.Empty;
            var value = parameters.TryGetValue("value", out var valueParam) && valueParam.IsValid ? valueParam.GetValue<string>() : string.Empty;

            if (!String.IsNullOrEmpty(key) && !String.IsNullOrEmpty(value))
            {
                env.SetValue(key, value);
            }
            // nothing to display
            return String.Empty;
        }

        public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext status)
        {
            throw new NotImplementedException();
        }
    }
}
