using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace Xcaciv.CommandTests.TestImpementations
{
    public class TestTextIo : ITextIoContext
    {
        public Guid Id => new Guid();

        public string Name => "TestTextIo";

        public Guid? Parent { get; set; } = null;
        /// <summary>
        /// test collection of children to verify commmand behavior
        /// </summary>
        public List<TestTextIo> Children { get; private set; } = new List<TestTextIo>();

        /// <summary>
        /// test collection of output chunks to verify behavior
        /// </summary>
        public List<string> Output { get; private set; } = new List<string>();

        public Task<ITextIoContext> GetChild(string Name)
        {
            var child = new TestTextIo();
            child.Parent = this.Id;
            this.Children.Add(child);
            return Task.FromResult<ITextIoContext>(child);
        }

        public Dictionary<string, string> PromptAnswers { get; private set; } = new Dictionary<string, string>();
        
        public Task<string> PromptForCommand(string prompt)
        {
            this.Output.Add($"PROMPT> {prompt}");

            var answer = PromptAnswers.ContainsKey(prompt) ? 
                    PromptAnswers[prompt] : 
                    prompt;

            return Task.FromResult(answer);
        }

        public Task<int> SetProgress(int total, int step)
        {
            return Task.FromResult(step);
        }

        public Task SetStatusMessage(string message)
        {
            this.Output.Add(message);
            return Task.CompletedTask;
        }

        public Task OutputLine(string message)
        {
            this.Output.Add(message);
            return Task.CompletedTask;
        }

        public Task OutputChunk(string message)
        {
            this.Output.Add(message);
            return Task.CompletedTask;
        }
    }
}
