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

namespace Xcaciv.CommandTests.TestImpementations
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

        public Dictionary<string, string> PromptAnswers { get; private set; } = new Dictionary<string, string>();

        public TestTextIo(string[]? arguments = null) : base("TestTextIo", null)
        {
            this.Parameters = arguments ?? string.Empty.Split(' ');
        }

        public override Task<ITextIoContext> GetChild(string[]? childArguments = null)
        {
            var child = new TestTextIo(childArguments)
            {
                Parent = this.Id
            };
            this.Children.Add(child);
            return Task.FromResult<ITextIoContext>(child);
        }

        public override Task<string> PromptForCommand(string prompt)
        {
            this.Output.Add($"PROMPT> {prompt}");

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
            this.Output.Add(message);
            return Task.CompletedTask;
        }

        public override Task HandleOutputChunk(string chunk)
        {
            this.Output.Add(chunk);
            return Task.CompletedTask;
        }

        public override string ToString()
        {
            string output = string.Empty;
            if (this.HasPipedInput)
            {
                // combine output into one string seperated by new lines
                // and then add the children output
                output = String.Join(Environment.NewLine, this.Output);
                foreach (var chidl in this.Children)
                {
                    output += chidl.ToString() + Environment.NewLine;
                }
            }



            output += String.Join('-', this.Output);

            return output;
        }

    }
}
