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
        /// <summary>
        /// output full help
        /// </summary>
        /// <param name="outputContext"></param>
        public virtual void Help(IIoContext outputContext)
        {
            outputContext.OutputChunk(this.BuildHelpString());
        }
        /// <summary>
        /// single line help command description, used for listing all commands
        /// </summary>
        /// <param name="outputContext"></param>
        public virtual void OneLineHelp(IIoContext outputContext)
        {
            var baseCommand = Attribute.GetCustomAttribute(this.GetType(), typeof(CommandRegisterAttribute)) as CommandRegisterAttribute;
            if (baseCommand != null)
                outputContext.OutputChunk($"{baseCommand.Command,-12} {baseCommand.Description}"); 
        }
        /// <summary>
        /// create a nicely formated
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildHelpString()
        {
            var thisType = this.GetType();
            var baseCommand = Attribute.GetCustomAttribute(thisType, typeof(CommandRegisterAttribute)) as CommandRegisterAttribute;
            var commandParameters = Attribute.GetCustomAttributes(thisType, typeof(CommandParameterNamedAttribute)) as CommandParameterNamedAttribute[];
            var helpRemarks = Attribute.GetCustomAttributes(thisType, typeof(CommandHelpRemarksAttribute)) as CommandHelpRemarksAttribute[];

            // TODO: extract a help formatter so it can be customized
            var builder = new StringBuilder();
            builder.AppendLine($"{baseCommand?.Command}:");
            builder.AppendLine($"  {baseCommand?.Description}");
            builder.AppendLine("Usage:");
            builder.AppendLine($"  {baseCommand?.Prototype}");
            builder.AppendLine();

            if (commandParameters != null && commandParameters.Length > 0)
            {
                builder.AppendLine("Options:");
                foreach (var parameter in commandParameters)
                {
                    builder.AppendLine($"  {parameter.ToString()}");
                }
            }

            if (helpRemarks != null && helpRemarks.Length > 0)
            {
                builder.AppendLine("Remarks:");
                foreach (var rem in helpRemarks)
                {
                    builder.AppendLine();
                    builder.AppendLine(rem.Remarks);
                }
            }

            builder.AppendLine();
            return builder.ToString();
        }
        /// <summary>
        /// execute pipe and single input the same
        /// </summary>
        /// <param name="input"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<string> Main(IIoContext input, IEnvironmentContext environment)
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

        public abstract string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env);

        public abstract string HandleExecution(string[] parameters, IEnvironmentContext env);

    }
}
