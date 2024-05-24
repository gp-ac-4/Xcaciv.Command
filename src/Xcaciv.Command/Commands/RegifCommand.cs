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
    [BaseCommand("REGIF", "Regular expression filter. Outputs the string if it matches", Prototype = @"<some command> | regif ""<regex expression>"" ""<string to check>""")]
    [CommandParameter("Regex", "Regular Expression")]
    [CommandParameter("String", "String to match")]
    public class RegifCommand : AbstractCommand
    {
        /// <summary>
        /// regex object for reuse
        /// </summary>
        protected Regex? expression { get; set; } = null;

        protected string regex { get; set; } = string.Empty;

        public override string HandleExecution(string[] parameters, IEnvironment status)
        {
            var output = new StringBuilder();
            setRegexExpression(parameters);
            foreach (var stringToCheck in parameters.Skip(1))
            {
                if (this.expression?.IsMatch(stringToCheck) ?? false)
                {
                    output.Append(" ");
                    output.Append(parameters[1]);
                }
            }
            return output.ToString().Trim();
        }

        public override string HandlePipedChunk(string stringToCheck, string[] parameters, IEnvironment status)
        {
            if (parameters.Length > 0)
            {
                setRegexExpression(parameters);
            }

            return (this.expression?.IsMatch(stringToCheck) ?? false) ? stringToCheck : string.Empty;
        }
        /// <summary>
        /// setup the regex object
        /// </summary>
        /// <param name="parameters"></param>
        private void setRegexExpression(string[] parameters)
        {
            if (this.expression == null && parameters.Length >0)
            {
                this.expression = new Regex(parameters[0]);
            }
        }


    }
}
