using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using SystemCommand = System.CommandLine.Command;

namespace Xcaciv.Command.Extensions.Commandline
{
    /// <summary>
    /// Wraps System.CommandLine.Command instances to work within the Xcaciv.Command pipeline.
    /// Console redirection is synchronized across concurrent instances to prevent interference.
    /// </summary>
    public class CommandLineCommand<T> : ICommandDelegate where T : SystemCommand
    {
        private static readonly SemaphoreSlim ConsoleRedirectionSemaphore = new(1, 1);
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

            var standardOutWriter = new StringWriter();
            var standardErrorWriter = new StringWriter();
            StringReader? standardInReader = null;

            var originalOut = Console.Out;
            var originalError = Console.Error;
            var originalIn = Console.In;

            try
            {
                Console.SetOut(standardOutWriter);
                Console.SetError(standardErrorWriter);

                if (!string.IsNullOrEmpty(pipedInput))
                {
                    standardInReader = new StringReader(pipedInput);
                    Console.SetIn(standardInReader);
                }

                // ioContext.Parameters are pre-tokenized strings from Xcaciv.Command framework.
                // System.CommandLine.Parse expects command-line arguments as they would appear
                // on the command line. The framework tokenizes the input, so values with spaces
                // are already separated into individual array elements, making them compatible
                // with System.CommandLine's parser expectations.
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
                standardOutWriter.Dispose();
                standardErrorWriter.Dispose();
                standardInReader?.Dispose();
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

            // Using synchronous Wait() here because Help() is a synchronous method.
            // The ICommandDelegate interface defines Help as string (not Task<string>),
            // so we cannot use await. Synchronous blocking is acceptable here since
            // help generation is a fast, non-I/O-bound operation.
            ConsoleRedirectionSemaphore.Wait();
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
                ConsoleRedirectionSemaphore.Release();
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

                builder.Append(chunk);
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
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
