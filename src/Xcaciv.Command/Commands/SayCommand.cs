using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;

namespace Xcaciv.Command.Commands
{
    [CommandRegister("Say", "Like echo but more valley.", Prototype ="SAY <thing to print>")]
    [CommandParameterSuffix("text", "Text to output")]
    [CommandHelpRemarks("Use double quotes to include environment variables in the format %var%.")]
    [CommandHelpRemarks("Piped input will be evalueated for env vars before being passed out.")]
    public class SayCommand : AbstractCommand
    {
        public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            // Get the text parameter which contains all arguments joined together
            if (parameters.TryGetValue("text", out var textParam) && textParam.IsValid)
            {
                var value = textParam.GetValue<string>();
                if (value.Contains('%')) value = ProcessEnvValues(value, env);
                return CommandResult<string>.Success(value, this.OutputFormat);
            }
            return CommandResult<string>.Success(string.Empty, this.OutputFormat);
        }

        public static string ProcessEnvValues(string value, IEnvironmentContext env)
        {
            Regex regex = new Regex(@"%(.\w*?)%");
            return regex.Replace(value, match =>
            {
                string variable = match.Groups[1].Value;
                string value = env.GetValue(variable);
                if (!String.IsNullOrEmpty(value))
                {
                    return value;
                }
                else
                {
                    return '%'+match.Value+'%';
                }
            });
        }

        public override IResult<string> HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            var processedValue = ProcessEnvValues(pipedChunk, env);
            return CommandResult<string>.Success(processedValue, this.OutputFormat);
        }
    }
}
