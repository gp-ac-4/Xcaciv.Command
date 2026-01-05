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
    [CommandRegister("ENV", "Output all environment variables", Prototype = "ENV")]
    internal class EnvCommand : AbstractCommand
    {
        public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            var values = String.Empty;
            foreach (var valuePair in env.GetEnvironment())
            {
                values += @$"{valuePair.Key} = {valuePair.Value}\n";
            }
            return CommandResult<string>.Success(values, this.OutputFormat);
        }

        public override IResult<string> HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            var values = String.Empty;
            foreach (var valuePair in env.GetEnvironment())
            {
                values += @$"{valuePair.Key} = {valuePair.Value}\n";
            }
            return CommandResult<string>.Success(values, this.OutputFormat);
        }
    }
}
