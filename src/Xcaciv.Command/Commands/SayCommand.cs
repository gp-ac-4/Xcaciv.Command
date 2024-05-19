using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command.Commands
{
    public class SayCommand : AbstractCommand
    {
        public override string BaseCommand { get; } = "SAY";


        public override string FriendlyName { get; } = "say something";


        public override string HelpString { get; } = @"SAY <thing to print> \n\tUse double quotes and %<env var name>% to embed values from the environment.";

        public override string HandleExecution(string[] parameters, IEnvironment env)
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

        public static string ProcessEnvValues(string value, IEnvironment env)
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

        public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironment env)
        {
            return ProcessEnvValues(pipedChunk, env);
        }

    }
}
