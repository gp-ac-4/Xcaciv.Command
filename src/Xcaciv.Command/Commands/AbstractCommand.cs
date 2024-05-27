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
            var commandParametersOrdered = GetOrderedParameters();
            var commandParametersNamed = GetNamedParameters();
            var commandParametersSuffix = GetSuffixParameters();
            var commandParametersFlag = GetFlagParameters();
            var helpRemarks = Attribute.GetCustomAttributes(thisType, typeof(CommandHelpRemarksAttribute)) as CommandHelpRemarksAttribute[];

            // TODO: extract a help formatter so it can be customized
            var builder = new StringBuilder();
            builder.AppendLine($"{baseCommand?.Command}:");
            builder.AppendLine($"  {baseCommand?.Description}");
            builder.AppendLine("Usage:");
            builder.AppendLine($"  {baseCommand?.Prototype}");
            builder.AppendLine();

            if (commandParametersOrdered.Length + commandParametersNamed.Length + commandParametersSuffix.Length + commandParametersFlag.Length > 0)
                builder.AppendLine("Options:");

            foreach (var parameter in commandParametersOrdered)
            {
                builder.AppendLine($"  {parameter.ToString()}");
            }

            foreach (var parameter in commandParametersFlag)
            {
                builder.AppendLine($"  {parameter.ToString()}");
            }

            foreach (var parameter in commandParametersNamed)
            {
                builder.AppendLine($"  {parameter.ToString()}");
            }

            foreach (var parameter in commandParametersSuffix)
            {
                builder.AppendLine($"  {parameter.ToString()}");
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
        /// <summary>
        /// Use the parameter attributest to process the parameters into a dictionary
        /// </summary>
        /// <param name="parameters"></param>
        protected Dictionary<string, string> ProcessParameters(string[] parameters)
        {
            if (parameters.Length == 0) return new Dictionary<string, string>();
            var parameterList = parameters.ToList();

            var parameterLookup = new Dictionary<string, string>();
            Type thisType = this.GetType();
            CommandParameters.
                        ProcessOrderedParameters(parameterList, parameterLookup, GetOrderedParameters());
            CommandParameters.ProcessFlags(parameterList, parameterLookup, GetFlagParameters());
            CommandParameters.ProcessNamedParameters(parameterList, parameterLookup, GetNamedParameters());
            CommandParameters.ProcessSuffixParameters(parameterList, parameterLookup, GetSuffixParameters());

            return parameterLookup;
        }

        /// <summary>
        /// reads parameter description from the instance
        /// </summary>
        /// <returns></returns>
        protected CommandParameterOrderedAttribute[] GetOrderedParameters()
        {
            var thisType = this.GetType();
            var ordered = Attribute.GetCustomAttributes(thisType, typeof(CommandParameterOrderedAttribute)) as CommandParameterOrderedAttribute[];
            return ordered ?? ([]);
        }
        /// <summary>
        /// reads parameter description from the instance
        /// </summary>
        /// <returns></returns>
        protected CommandParameterNamedAttribute[] GetNamedParameters()
        {
            var thisType = this.GetType();
            var named = Attribute.GetCustomAttributes(thisType, typeof(CommandParameterNamedAttribute)) as CommandParameterNamedAttribute[];
            return named ?? ([]);
        }
        /// <summary>
        /// reads parameter description from the instance
        /// </summary>
        /// <returns></returns>
        protected CommandFlagAttribute[] GetFlagParameters()
        {
            var thisType = this.GetType();
            var flags = Attribute.GetCustomAttributes(thisType, typeof(CommandFlagAttribute)) as CommandFlagAttribute[];
            return flags ?? ([]);
        }
        /// <summary>
        /// reads parameter description from the instance
        /// </summary>
        /// <returns></returns>
        protected CommandParameterSuffixAttribute[] GetSuffixParameters()
        {
            var thisType = this.GetType();
            var flags = Attribute.GetCustomAttributes(thisType, typeof(CommandParameterSuffixAttribute)) as CommandParameterSuffixAttribute[];
            return flags ?? ([]);
        }

        public abstract string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env);


        public abstract string HandleExecution(string[] parameters, IEnvironmentContext env);

    }
}
