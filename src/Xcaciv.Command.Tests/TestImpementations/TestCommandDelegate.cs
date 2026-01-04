using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Parameters;

namespace Xcaciv.Command.Tests.TestImpementations
{
    /// <summary>
    /// Direct implementation of ICommandDelegate for tests that need full control
    /// without using AbstractCommand's parameter processing logic.
    /// </summary>
    public class TestCommandDelegate : ICommandDelegate
    {
        private readonly Func<IIoContext, IEnvironmentContext, IAsyncEnumerable<IResult<string>>>? _mainFunc;
        private readonly Func<string[], IEnvironmentContext, string>? _helpFunc;
        private readonly Func<string[], string>? _oneLineHelpFunc;

        public TestCommandDelegate(
            Func<IIoContext, IEnvironmentContext, IAsyncEnumerable<IResult<string>>>? mainFunc = null,
            Func<string[], IEnvironmentContext, string>? helpFunc = null,
            Func<string[], string>? oneLineHelpFunc = null)
        {
            _mainFunc = mainFunc;
            _helpFunc = helpFunc;
            _oneLineHelpFunc = oneLineHelpFunc;
        }

        public async IAsyncEnumerable<IResult<string>> Main(IIoContext io, IEnvironmentContext environment)
        {
            if (_mainFunc != null)
            {
                await foreach (var result in _mainFunc(io, environment))
                {
                    yield return result;
                }
            }
            else
            {
                yield return CommandResult<string>.Success("TestCommandDelegate executed");
            }
        }

        public string Help(string[] parameters, IEnvironmentContext env)
        {
            return _helpFunc?.Invoke(parameters, env) ?? "Test command help";
        }

        public string OneLineHelp(string[] parameters)
        {
            return _oneLineHelpFunc?.Invoke(parameters) ?? "Test command one-line help";
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Simple echo command delegate for testing pipelines.
    /// </summary>
    public class EchoCommandDelegate : ICommandDelegate
    {
        private readonly string _prefix;

        public EchoCommandDelegate(string prefix = "")
        {
            _prefix = prefix;
        }

        public async IAsyncEnumerable<IResult<string>> Main(IIoContext io, IEnvironmentContext environment)
        {
            if (io.HasPipedInput)
            {
                await foreach (var chunk in io.ReadInputPipeChunks())
                {
                    yield return CommandResult<string>.Success(_prefix + chunk);
                }
            }
            else if (io.Parameters != null && io.Parameters.Length > 0)
            {
                foreach (var param in io.Parameters)
                {
                    yield return CommandResult<string>.Success(_prefix + param);
                }
            }
        }

        public string Help(string[] parameters, IEnvironmentContext env)
        {
            return "Echo command - outputs input";
        }

        public string OneLineHelp(string[] parameters)
        {
            return "ECHO         Echo input";
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
