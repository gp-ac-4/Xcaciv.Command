using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;

namespace Xcaciv.Command.Tests.Commands
{
    public class ParameterTestCommand(CommandParameterOrderedAttribute[] ordered, CommandFlagAttribute[] flags, CommandParameterNamedAttribute[] named, CommandParameterSuffixAttribute[] suffix) : AbstractCommand
    {
        public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            var builder = new StringBuilder();

            foreach (var pair in parameters)
            {
                builder.AppendLine($"{pair.Key} = {pair.Value.RawValue}");
            }

            return builder.ToString();
        }

        public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            throw new NotImplementedException();
        }
    }
}
