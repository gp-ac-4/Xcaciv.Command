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
        private readonly CommandParameterOrderedAttribute[] _ordered = ordered;
        private readonly CommandFlagAttribute[] _flags = flags;
        private readonly CommandParameterNamedAttribute[] _named = named;
        private readonly CommandParameterSuffixAttribute[] _suffix = suffix;

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

        // Override parameter getters to return the attributes provided in the constructor
        protected override CommandParameterOrderedAttribute[] GetOrderedParameters(bool hasPipedInput)
        {
            var result = _ordered ?? Array.Empty<CommandParameterOrderedAttribute>();
            if (hasPipedInput)
            {
                result = result.Where(x => !x.UsePipe).ToArray();
            }
            return result;
        }

        protected override CommandFlagAttribute[] GetFlagParameters()
        {
            return _flags ?? Array.Empty<CommandFlagAttribute>();
        }

        protected override CommandParameterNamedAttribute[] GetNamedParameters(bool hasPipedInput)
        {
            var result = _named ?? Array.Empty<CommandParameterNamedAttribute>();
            if (hasPipedInput)
            {
                result = result.Where(x => !x.UsePipe).ToArray();
            }
            return result;
        }

        protected override CommandParameterSuffixAttribute[] GetSuffixParameters(bool hasPipedInput)
        {
            var result = _suffix ?? Array.Empty<CommandParameterSuffixAttribute>();
            if (hasPipedInput)
            {
                result = result.Where(x => !x.UsePipe).ToArray();
            }
            return result;
        }
    }
}
