using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command.Core
{
    public abstract class AbstractCommand : ICommandDelegate
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
        /// <param name="parameters"></param>
        public virtual string Help(string[] parameters, IEnvironmentContext env)
        {
            return BuildHelpString(parameters, env);
        }
        /// <summary>
        /// single line help command description, used for listing all commands
        /// </summary>
        /// <param name="parameters"></param>
        public virtual string OneLineHelp(string[] parameters)
        {
            var thisType = GetType();
            var baseCommand = Attribute.GetCustomAttribute(thisType, typeof(CommandRegisterAttribute)) as CommandRegisterAttribute;
            if (baseCommand == null)
            {
                throw new Exception("CommandRegisterAttribute is required for all commands");
            }

            if (Attribute.GetCustomAttribute(thisType, typeof(CommandRootAttribute)) is CommandRootAttribute)
            {
                return $"-\t{baseCommand.Command,-12} {baseCommand.Description}";
            }
            else
            {
                return $"{baseCommand.Command,-12} {baseCommand.Description}";
            }


        }
        /// <summary>
        /// create a nicely formated
        /// </summary>
        /// <returns></returns>
        protected virtual string BuildHelpString(string[] parameters, IEnvironmentContext environment)
        {
            var thisType = GetType();
            var baseCommand = Attribute.GetCustomAttribute(thisType, typeof(CommandRegisterAttribute)) as CommandRegisterAttribute;
            var commandParametersOrdered = GetOrderedParameters(false);
            var commandParametersFlag = GetFlagParameters();
            var commandParametersNamed = GetNamedParameters(false);
            var commandParametersSuffix = GetSuffixParameters(false);
            var helpRemarks = Attribute.GetCustomAttributes(thisType, typeof(CommandHelpRemarksAttribute)) as CommandHelpRemarksAttribute[];

            // TODO: extract a help formatter so it can be customized
            var builder = new StringBuilder();
            if (Attribute.GetCustomAttribute(thisType, typeof(CommandRootAttribute)) is CommandRootAttribute rootCommand)
                builder.Append($"{rootCommand.Command} ");            
            builder.AppendLine($"{baseCommand?.Command}:");
            builder.AppendLine($"  {baseCommand?.Description}");
            builder.AppendLine();
            builder.AppendLine($"Usage:");
            
            if (commandParametersOrdered.Length + commandParametersNamed.Length + commandParametersSuffix.Length + commandParametersFlag.Length > 0)
            {

                // examne the parameters
                var parameterBuilder = new StringBuilder();
                var prototypeBuilder = new StringBuilder();

                foreach (var parameter in commandParametersOrdered)
                {
                    parameterBuilder.AppendLine($"  {parameter.ToString()}");
                    prototypeBuilder.Append($"{parameter.GetIndicator()} ");
                }

                foreach (var parameter in commandParametersFlag)
                {
                    parameterBuilder.AppendLine($"  {parameter.ToString()}");
                    prototypeBuilder.Append($"{parameter.GetIndicator()} ");
                }

                foreach (var parameter in commandParametersNamed)
                {
                    parameterBuilder.AppendLine($"  {parameter.ToString()}");
                    string valueIndicator = (parameter.AllowedValues.Length > 0) ?
                        $"[{string.Join("|", parameter.AllowedValues)}]" :
                        parameter.Name;

                    prototypeBuilder.Append($"{parameter.GetIndicator()} <{valueIndicator}> ");
                }

                foreach (var parameter in commandParametersSuffix)
                {
                    parameterBuilder.AppendLine($"  {parameter.ToString()}");
                    prototypeBuilder.Append($"{parameter.GetIndicator()} ");
                }

                // append parameter help
                if (baseCommand?.Prototype.Equals("todo", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    builder.AppendLine(prototypeBuilder.ToString());
                }
                else
                {
                    builder.AppendLine($"  {baseCommand?.Prototype}");
                }

                builder.AppendLine();
                builder.AppendLine("Options:");
                builder.AppendLine(parameterBuilder.ToString());
            }

            // append remarks
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
        /// <param name="io"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<string> Main(IIoContext io, IEnvironmentContext environment)
        {
            if (io.HasPipedInput)
            {
                await foreach (var p in io.ReadInputPipeChunks())
                {
                    if (string.IsNullOrEmpty(p)) continue;
                    yield return HandlePipedChunk(p, io.Parameters, environment);
                }
            }
            else
            {
                if (io.Parameters.Length > 0 && io.Parameters[0].Equals("--HELP", StringComparison.CurrentCultureIgnoreCase))
                    yield return BuildHelpString(io.Parameters, environment);
                else
                    yield return HandleExecution(io.Parameters, environment);
            }
        }
        /// <summary>
        /// Use the parameter attributest to process the parameters into a dictionary
        /// </summary>
        /// <param name="parameters"></param>
        protected Dictionary<string, string> ProcessParameters(string[] parameters, bool hasPipedInput = false)
        {
            if (parameters.Length == 0) return new Dictionary<string, string>();
            var parameterList = parameters.ToList();

            var parameterLookup = new Dictionary<string, string>();
            Type thisType = GetType();
            CommandParameters.
                        ProcessOrderedParameters(parameterList, parameterLookup, GetOrderedParameters(hasPipedInput));
            CommandParameters.ProcessFlags(parameterList, parameterLookup, GetFlagParameters());
            CommandParameters.ProcessNamedParameters(parameterList, parameterLookup, GetNamedParameters(hasPipedInput));
            CommandParameters.ProcessSuffixParameters(parameterList, parameterLookup, GetSuffixParameters(hasPipedInput));

            return parameterLookup;
        }

        /// <summary>
        /// reads parameter description from the instance
        /// </summary>
        /// <returns></returns>
        protected CommandParameterOrderedAttribute[] GetOrderedParameters(bool hasPipedInput)
        {
            var thisType = GetType();
            var ordered = Attribute.GetCustomAttributes(thisType, typeof(CommandParameterOrderedAttribute)) as CommandParameterOrderedAttribute[] ?? ([]);
            if (hasPipedInput)
            {
                ordered = ordered.Where(x => !x.UsePipe).ToArray();
            }

            return ordered;
        }
        /// <summary>
        /// reads parameter description from the instance
        /// </summary>
        /// <returns></returns>
        protected CommandParameterNamedAttribute[] GetNamedParameters(bool hasPipedInput)
        {
            var thisType = GetType();
            var named = Attribute.GetCustomAttributes(thisType, typeof(CommandParameterNamedAttribute)) as CommandParameterNamedAttribute[] ?? ([]);
            if (hasPipedInput)
            {
                named = named.Where(x => !x.UsePipe).ToArray();
            }
            return named;
        }
        /// <summary>
        /// reads parameter description from the instance
        /// </summary>
        /// <returns></returns>
        protected CommandFlagAttribute[] GetFlagParameters()
        {
            var thisType = GetType();
            var flags = Attribute.GetCustomAttributes(thisType, typeof(CommandFlagAttribute)) as CommandFlagAttribute[] ?? ([]);
            return flags;
        }
        /// <summary>
        /// reads parameter description from the instance
        /// </summary>
        /// <returns></returns>
        protected CommandParameterSuffixAttribute[] GetSuffixParameters(bool hasPipedInput)
        {
            var thisType = GetType();
            var flags = Attribute.GetCustomAttributes(thisType, typeof(CommandParameterSuffixAttribute)) as CommandParameterSuffixAttribute[] ?? ([]);
            if (hasPipedInput)
            {
                flags = flags.Where(x => !x.UsePipe).ToArray();
            }
            return flags;
        }

        public abstract string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironmentContext env);


        public abstract string HandleExecution(string[] parameters, IEnvironmentContext env);

    }
}
