using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command.Tests.Commands
{
    public class ParameterTestCommand(CommandParameterOrderedAttribute[] ordered, CommandFlagAttribute[] flags, CommandParameterNamedAttribute[] named, CommandParameterSuffixAttribute[] suffix) : AbstractCommand
    {
        public override string HandleExecution(string[] parameters, IEnvironmentContext env)
        {
            var parameterList = parameters.ToList();

            var parameterLookup = new Dictionary<string, string>();

            CommandParameters.ProcessOrderedParameters(parameterList, parameterLookup, ordered);
            CommandParameters.ProcessFlags(parameterList, parameterLookup, flags);
            CommandParameters.ProcessNamedParameters(parameterList, parameterLookup, named);
            CommandParameters.ProcessSuffixParameters(parameterList, parameterLookup, suffix);

            var builder = new StringBuilder();

            foreach (var pair in parameterLookup)
            {
                builder.AppendLine($"{pair.Key} = {pair.Value}");
            }

            return builder.ToString();
        }

        public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env)
        {
            throw new NotImplementedException();
        }
    }
}
