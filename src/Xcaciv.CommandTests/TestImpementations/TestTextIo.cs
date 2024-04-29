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

        public Guid? Parent => null;

        public Task<ITextIoContext> GetChild(string Name)
        {
            return Task.FromResult<ITextIoContext>(new TestTextIo());
        }

        private List<string> _output = new List<string>()
        {
            "say what is up",
            "say this is a test"
        };

        public Task<string> PromptForCommand(string prompt)
        {
            return Task.FromResult(_output.First());
        }

        public Task<int> SetProgress(int total, int step)
        {
            return Task.FromResult(step);
        }

        public Task SetStatusMessage(string message)
        {
            return Task.CompletedTask;
        }

        public Task WriteLine(string message)
        {
            return Task.CompletedTask;
        }
    }
}
