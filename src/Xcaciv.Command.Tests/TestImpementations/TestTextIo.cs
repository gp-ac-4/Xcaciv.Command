using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xcaciv.Command;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command.Tests.TestImpementations
{
    public class TestTextIo : AbstractTextIo
    {
        /// <summary>
        /// test collection of children to verify commmand behavior
        /// </summary>
        public List<TestTextIo> Children { get; private set; } = new List<TestTextIo>();

        /// <summary>
        /// test collection of output chunks to verify behavior
        /// </summary>
        public List<string> Output { get; private set; } = new List<string>();

        public List<string> Trace { get; private set; } = new List<string>();

        public Dictionary<string, string> PromptAnswers { get; private set; } = new Dictionary<string, string>();

        public TestTextIo(string[]? arguments = null, Dictionary<string, string>? envVars = default) : base("TestTextIo", null)
        {
            this.Parameters = arguments ?? string.Empty.Split(' ');
            this.Verbose = true;
            if (envVars != null)
                this.EnvironmentVariables = new System.Collections.Concurrent.ConcurrentDictionary<string, string>(envVars);
        }

        public override Task<ITextIoContext> GetChild(string[]? childArguments = null)
        {
            var envVarsCopy = this.EnvironmentVariables.ToDictionary() ;
            var child = new TestTextIo(childArguments, envVarsCopy)
            {
                Parent = Id
            };
            Children.Add(child);
            return Task.FromResult<ITextIoContext>(child);
        }

        public override Task<string> PromptForCommand(string prompt)
        {
            Output.Add($"PROMPT> {prompt}");

            var answer = PromptAnswers.ContainsKey(prompt) ?
                    PromptAnswers[prompt] :
                    prompt;

            return Task.FromResult(answer);
        }

        public override Task<int> SetProgress(int total, int step)
        {
            return Task.FromResult(step);
        }

        public override Task SetStatusMessage(string message)
        {
            Output.Add(message);
            return Task.CompletedTask;
        }

        public override Task HandleOutputChunk(string chunk)
        {
            Output.Add(chunk);
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            string output = string.Empty;
            if (HasPipedInput)
            {
                // combine output into one string seperated by new lines
                // and then add the children output
                output = string.Join(Environment.NewLine, Output);
                foreach (var chidl in Children)
                {
                    output += chidl.ToString() + Environment.NewLine;
                }
            }

            output += string.Join('-', Output);

            return output;
        }

        public override Task AddTraceMessage(string message)
        {
            Trace.Add(message);  
            // if we are not verbose, send the output to DEBUG
            System.Diagnostics.Debug.WriteLine(message);
            return Task.CompletedTask;
        }

    }
}
