using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;

namespace Xcaciv.Command.Core
{
    public abstract class AbstractCommand : ICommandDelegate
    {
        private static IHelpService? _helpService;

        public ResultFormat OutputFormat { get; protected set; } = ResultFormat.General;

        /// <summary>
        /// Sets the help service used by all AbstractCommand instances for help formatting.
        /// Should be set during application startup, typically by the CommandController.
        /// TODO: make this non-static !!!
        /// </summary>
        public static void SetHelpService(IHelpService helpService)
        {
            _helpService = helpService ?? throw new ArgumentNullException(nameof(helpService));
        }

        /// <summary>
        /// this should be overwritten to dispose of any unmanaged items
        /// </summary>
        /// <returns></returns>
        public virtual ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Generates full help output for the command using the configured help service.
        /// </summary>
        /// <param name="parameters">Command parameters for context</param>
        /// <param name="env">Environment context</param>
        /// <returns>Formatted help string</returns>
        public virtual string Help(string[] parameters, IEnvironmentContext env)
        {
            if (_helpService == null)
            {
                throw new InvalidOperationException(
                    "HelpService has not been configured. Call AbstractCommand.SetHelpService() during application startup.");
            }

            return _helpService.BuildHelp(this, parameters, env);
        }

        /// <summary>
        /// Generates single line help summary for command listing.
        /// </summary>
        /// <param name="parameters">Command parameters for context</param>
        /// <returns>Single line formatted help string</returns>
        public virtual string OneLineHelp(string[] parameters)
        {
            if (_helpService == null)
            {
                var thisType = GetType();
                var baseCommand = Attribute.GetCustomAttribute(thisType, typeof(CommandRegisterAttribute)) as CommandRegisterAttribute;
                if (baseCommand == null)
                {
                    throw new InvalidOperationException("CommandRegisterAttribute is required for all commands");
                }

                var isRoot = Attribute.GetCustomAttribute(thisType, typeof(CommandRootAttribute)) is CommandRootAttribute;
                var prefix = isRoot ? "-\t" : "";
                return $"{prefix}{baseCommand.Command,-12} {baseCommand.Description}";
            }

            var commandDesc = CommandParameters.CreatePackageDescription(GetType(), null!);
            return _helpService.BuildOneLineHelp(commandDesc);
        }

        public async IAsyncEnumerable<IResult<string>> Main(IIoContext io, IEnvironmentContext environment)
        {
            var processedParameters = ProcessParameters(io.Parameters, io.HasPipedInput);

            if (io.HasPipedInput)
            {
                OnStartPipe(processedParameters, environment);

                await foreach (var pipedChunk in io.ReadInputPipeChunks())
                {
                    if (string.IsNullOrEmpty(pipedChunk)) continue;
                    yield return CommandResult<string>.Success(HandlePipedChunk(pipedChunk, processedParameters, environment), this.OutputFormat);
                }

                OnEndPipe(processedParameters, environment);
            }
            else
            {
                var parameterArray = io.Parameters ?? Array.Empty<string>();
                var isHelp = _helpService?.IsHelpRequest(parameterArray) ?? false;
                if (isHelp)
                {
                    yield return CommandResult<string>.Success(Help(parameterArray, environment), ResultFormat.General);
                }
                else
                {
                    yield return CommandResult<string>.Success(HandleExecution(processedParameters, environment), this.OutputFormat);
                }
            }
        }

        /// <summary>
        /// Processes command parameters using attributes into a typed dictionary.
        /// </summary>
        /// <param name="parameters">Raw command parameters</param>
        /// <param name="hasPipedInput">Whether command is receiving piped input</param>
        /// <returns>Dictionary of processed parameter values</returns>
        public Dictionary<string, IParameterValue> ProcessParameters(string[] parameters, bool hasPipedInput = false)
        {
            if (parameters.Length == 0)
            {
                return new Dictionary<string, IParameterValue>(StringComparer.OrdinalIgnoreCase);
            }

            return CommandParameters.ProcessTypedParameters(
                parameters,
                GetOrderedParameters(hasPipedInput),
                GetFlagParameters(),
                GetNamedParameters(hasPipedInput),
                GetSuffixParameters(hasPipedInput));
        }

        /// <summary>
        /// Retrieves ordered parameter attributes from the command type.
        /// </summary>
        /// <param name="hasPipedInput">If true, excludes parameters marked with UsePipe</param>
        /// <returns>Array of ordered parameter attributes</returns>
        protected CommandParameterOrderedAttribute[] GetOrderedParameters(bool hasPipedInput)
        {
            var thisType = GetType();
            var attributes = Attribute.GetCustomAttributes(thisType, typeof(CommandParameterOrderedAttribute)) as CommandParameterOrderedAttribute[] ?? Array.Empty<CommandParameterOrderedAttribute>();
            
            return hasPipedInput 
                ? attributes.Where(x => !x.UsePipe).ToArray() 
                : attributes;
        }

        /// <summary>
        /// Retrieves named parameter attributes from the command type.
        /// </summary>
        /// <param name="hasPipedInput">If true, excludes parameters marked with UsePipe</param>
        /// <returns>Array of named parameter attributes</returns>
        protected CommandParameterNamedAttribute[] GetNamedParameters(bool hasPipedInput)
        {
            var thisType = GetType();
            var attributes = Attribute.GetCustomAttributes(thisType, typeof(CommandParameterNamedAttribute)) as CommandParameterNamedAttribute[] ?? Array.Empty<CommandParameterNamedAttribute>();
            
            return hasPipedInput 
                ? attributes.Where(x => !x.UsePipe).ToArray() 
                : attributes;
        }

        /// <summary>
        /// Retrieves flag parameter attributes from the command type.
        /// </summary>
        /// <returns>Array of flag attributes</returns>
        protected CommandFlagAttribute[] GetFlagParameters()
        {
            var thisType = GetType();
            return Attribute.GetCustomAttributes(thisType, typeof(CommandFlagAttribute)) as CommandFlagAttribute[] ?? Array.Empty<CommandFlagAttribute>();
        }

        /// <summary>
        /// Retrieves suffix parameter attributes from the command type.
        /// </summary>
        /// <param name="hasPipedInput">If true, excludes parameters marked with UsePipe</param>
        /// <returns>Array of suffix parameter attributes</returns>
        protected CommandParameterSuffixAttribute[] GetSuffixParameters(bool hasPipedInput)
        {
            var thisType = GetType();
            var attributes = Attribute.GetCustomAttributes(thisType, typeof(CommandParameterSuffixAttribute)) as CommandParameterSuffixAttribute[] ?? Array.Empty<CommandParameterSuffixAttribute>();
            
            return hasPipedInput 
                ? attributes.Where(x => !x.UsePipe).ToArray() 
                : attributes;
        }
        /// <summary>
        /// Executes the operation using the specified parameters and environment context, and returns the result as a
        /// string.
        /// </summary>
        /// <param name="parameters">A dictionary containing parameter names and their corresponding values to be used during execution. Cannot
        /// be null.</param>
        /// <param name="env">The environment context in which the operation is executed. Cannot be null.</param>
        /// <returns>A string representing the result of the execution. The format and content of the result depend on the
        /// specific implementation.</returns>
        public abstract string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env);
        /// <summary>
        /// Processes a chunk of piped input and returns the resulting string after applying the specified parameters
        /// and environment context.
        /// </summary>
        /// <param name="pipedChunk">The input string representing the chunk of data to process. Cannot be null.</param>
        /// <param name="parameters">A dictionary of parameter names and their corresponding values to be used during processing. Cannot be null.</param>
        /// <param name="env">The environment context that provides additional information or services required for processing. Cannot be
        /// null.</param>
        /// <returns>A string containing the processed result of the input chunk.</returns>
        public abstract string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env);

        /// <summary>
        /// Invoked when a pipe operation is starting, allowing derived classes to perform custom initialization.
        /// </summary>
        /// <param name="processedParameters">Dictionary containing processed parameter values</param>
        /// <param name="environment">Environment context</param>
        protected virtual void OnStartPipe(Dictionary<string, IParameterValue> processedParameters, IEnvironmentContext environment)
        {
        }

        /// <summary>
        /// Invoked when pipe processing has completed, allowing for custom post-processing or cleanup.
        /// </summary>
        /// <param name="processedParameters">Dictionary containing processed parameter values</param>
        /// <param name="environment">Environment context</param>
        protected virtual void OnEndPipe(Dictionary<string, IParameterValue> processedParameters, IEnvironmentContext environment)
        {
        }

    }
}
