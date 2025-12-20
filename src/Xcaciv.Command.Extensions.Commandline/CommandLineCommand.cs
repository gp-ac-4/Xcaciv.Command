using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using SystemCommand = System.CommandLine.Command;

namespace Xcaciv.Command.Extensions.Commandline
{
    public class CommandLineCommand<T> : ICommandDelegate where T : SystemCommand
    {
        private T? command;

        protected T? WrappedCommand => command;

        public virtual void SetCommand(T commandToWrap)
        {
            command = commandToWrap ?? throw new ArgumentNullException(nameof(commandToWrap));
            EnsureHelpOption(command);
        }

        public virtual async IAsyncEnumerable<IResult<string>> Main(IIoContext ioContext, IEnvironmentContext env)
        {
            if (command == null)
            {
                yield return CommandResult<string>.Failure("Command has not been initialized. Call SetCommand before execution.");
                yield break;
            }

            var pipedInput = await CollectPipedInput(ioContext).ConfigureAwait(false);

            using var standardOutWriter = new StringWriter();
            using var standardErrorWriter = new StringWriter();

            var originalOut = Console.Out;
            var originalError = Console.Error;
            var originalIn = Console.In;

            try
            {
                Console.SetOut(standardOutWriter);
                Console.SetError(standardErrorWriter);

                if (!string.IsNullOrEmpty(pipedInput))
                {
                    Console.SetIn(new StringReader(pipedInput));
                }

                var parseResult = command.Parse(ioContext.Parameters ?? Array.Empty<string>());
                var exitCode = await parseResult.InvokeAsync().ConfigureAwait(false);

                var output = standardOutWriter.ToString();
                var errorOutput = standardErrorWriter.ToString();

                if (exitCode == 0)
                {
                    yield return CommandResult<string>.Success(output);
                }
                else
                {
                    var failureMessage = string.IsNullOrWhiteSpace(errorOutput)
                        ? $"Command '{command.Name}' exited with code {exitCode}."
                        : errorOutput;
                    yield return CommandResult<string>.Failure(failureMessage);
                }
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
                Console.SetIn(originalIn);
            }
        }

        public virtual string Help(string[] parameters, IEnvironmentContext env)
        {
            if (command == null)
            {
                throw new InvalidOperationException("Command has not been initialized. Call SetCommand before requesting help.");
            }

            var helpArguments = (parameters ?? Array.Empty<string>()).Concat(["--help"]).ToArray();

            using var standardOutWriter = new StringWriter();
            using var standardErrorWriter = new StringWriter();

            var originalOut = Console.Out;
            var originalError = Console.Error;

            try
            {
                Console.SetOut(standardOutWriter);
                Console.SetError(standardErrorWriter);

                var parseResult = command.Parse(helpArguments);
                parseResult.Invoke();

                var output = standardOutWriter.ToString();
                var errors = standardErrorWriter.ToString();

                return string.IsNullOrWhiteSpace(errors) ? output : output + errors;
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }

        public virtual string OneLineHelp(string[] parameters)
        {
            if (command == null)
            {
                throw new InvalidOperationException("Command has not been initialized. Call SetCommand before requesting help text.");
            }

            var description = string.IsNullOrWhiteSpace(command.Description)
                ? "No description provided."
                : command.Description;

            return $"{command.Name,-12} {description}";
        }

        public virtual ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        private static async Task<string> CollectPipedInput(IIoContext ioContext)
        {
            if (!ioContext.HasPipedInput)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();

            await foreach (var chunk in ioContext.ReadInputPipeChunks().ConfigureAwait(false))
            {
                if (string.IsNullOrEmpty(chunk))
                {
                    continue;
                }

                builder.AppendLine(chunk);
            }

            return builder.ToString();
        }

        private static void EnsureHelpOption(SystemCommand commandToWrap)
        {
            var hasHelp = commandToWrap.Options.Any(option => option is HelpOption || option.Aliases.Any(alias => alias.Equals("--help", StringComparison.OrdinalIgnoreCase)));
            if (!hasHelp)
            {
                commandToWrap.Add(new HelpOption());
            }
        }
    }
}
