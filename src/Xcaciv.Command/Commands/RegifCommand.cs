using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command.Commands
{
    public class RegifCommand : AbstractCommand
    {
        public override string BaseCommand { get; } = "REGIF";


        public override string FriendlyName { get; } = "regular expression filter";


        public override string HelpString { get; } = "<some command> | regif '<regex expression>'";
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
