using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command.Commands
{
    [BaseCommand("Say", "Like echo but more valley.", Prototype ="SAY <thing to print>")]
    [CommandParameter("text", "Text to output")]
    [CommandHelpRemarks("Use double quotes to include environment variables in the format %var%.")]
    [CommandHelpRemarks("Piped input will be evalueated for env vars before being passed out.")]
    public class SayCommand : AbstractCommand
    {
        public override string HandleExecution(string[] parameters, IEnvironmentContext env)
        {
            var builder = new StringBuilder();
            foreach (var parameter in parameters) // zero position contains command
            {
                var value = parameter.ToString();
                if (value.Contains('%')) value = ProcessEnvValues(value, env);
                builder.Append(value);
                builder.Append(' ');
            }
            var result = builder.ToString();
            return result[..^1];
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

        public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env)
        {
            return ProcessEnvValues(pipedChunk, env);
        }

    }
}
