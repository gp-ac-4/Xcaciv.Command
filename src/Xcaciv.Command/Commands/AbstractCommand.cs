using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command.Commands
{
    public abstract class AbstractCommand : Xcaciv.Command.Interface.ICommandDelegate
    { 

        /// <summary>
        /// this should be overwritten to dispose of any unmanaged items
        /// </summary>
        /// <returns></returns>
        public virtual ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public virtual void Help(IOutputContext outputContext)
        {
            outputContext.OutputChunk(this.BuildHelpString());
        }
        /// <summary>
        /// create a nicely formated
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildHelpString()
        {            
            var thisType = this.GetType();
            var baseCommand = Attribute.GetCustomAttribute(thisType, typeof(BaseCommandAttribute)) as BaseCommandAttribute;
            var commandParameters = Attribute.GetCustomAttributes(thisType, typeof(CommandParameterAttribute)) as CommandParameterAttribute[];
            var helpRemarks = Attribute.GetCustomAttributes(thisType, typeof(CommandHelpRemarksAttribute)) as CommandHelpRemarksAttribute[];

            // TODO: extract a help formatter so it can be customized
            var builder = new StringBuilder();
            builder.AppendLine($"- {baseCommand?.Command} HELP ------------------------------");
            builder.AppendLine(baseCommand?.Description);
            builder.AppendLine();
            builder.AppendLine(baseCommand?.Prototype);
            builder.AppendLine();

            if (commandParameters != null && commandParameters.Length > 0)
            {
                builder.AppendLine("---- PARAMETERS");
                foreach (var parameter in commandParameters)
                {
                    builder.AppendLine(parameter.ToString());
                }
            }

            if (helpRemarks != null && helpRemarks.Length > 0)
            {
                builder.AppendLine("---- REMARKS");
                foreach (var rem in helpRemarks)
                {
                    builder.AppendLine();
                    builder.AppendLine(rem.Remarks);
                }
            }

            builder.AppendLine($"----------------------------------------");
            builder.AppendLine($"----------------------------------------");

            return builder.ToString();
        }
        /// <summary>
        /// execute pipe and single input the same
        /// </summary>
        /// <param name="input"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<string> Main(IInputContext input, IEnvironment environment)
        {
            if (input.HasPipedInput)
            {
                await foreach (var p in input.ReadInputPipeChunks())
                {
                    if (string.IsNullOrEmpty(p)) continue;
                    yield return this.HandlePipedChunk(p, input.Parameters, environment);
                }
            }
            else
            {
                if (input.Parameters.Length > 0 && input.Parameters[0].ToUpper() == "--HELP")
                    yield return BuildHelpString();
                else
                    yield return HandleExecution(input.Parameters, environment);
            }
        }

        public abstract string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironment env);

        public abstract string HandleExecution(string[] parameters, IEnvironment env);

    }
}
