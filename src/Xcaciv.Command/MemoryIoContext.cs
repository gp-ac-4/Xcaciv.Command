using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command
{
    public class MemoryIoContext : AbstractTextIo
    {
        public MemoryIoContext(string name = "MemoryIo", Guid? parentId = default) : base(name, parentId)
        {
        }

        public ConcurrentBag<MemoryIoContext> Children { get; private set; } = new ConcurrentBag<MemoryIoContext>();
        public ConcurrentBag<string> Output { get; private set; } = new ConcurrentBag<string>();
        public ConcurrentDictionary<string, string> PromptAnswers { get; private set; } = new ConcurrentDictionary<string, string>();

        public override Task<IIoContext> GetChild(string[]? childArguments = null)
        {
            var child = new MemoryIoContext(this.Name + "Child", Id);

            Children.Add(child);

            if (this.HasPipedInput && this.inputPipe != null) child.SetInputPipe(this.inputPipe);

            if (this.outputPipe != null) child.SetOutputPipe(this.outputPipe);

            return Task.FromResult<IIoContext>(child);
        }

        public override Task HandleOutputChunk(string chunk)
        {
            Output.Add(chunk);
            return Task.CompletedTask;
        }

        public override Task<string> PromptForCommand(string prompt)
        {
            Output.Add($"PROMPT> {prompt}:");

            var answer = PromptAnswers.ContainsKey(prompt) ?
                    PromptAnswers[prompt] :
                    prompt;

            return Task.FromResult(answer);
        }

        public override Task<int> SetProgress(int total, int step)
        {
            Output.Add($"Progress: {step} of {total}");
            return Task.FromResult(step);
        }

        public override Task SetStatusMessage(string message)
        {
            Output.Add($"Status: {message}");
            return Task.CompletedTask;
        }
    }
}
