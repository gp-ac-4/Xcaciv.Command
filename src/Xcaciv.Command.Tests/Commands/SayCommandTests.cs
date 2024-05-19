using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.FileLoader;
using Xcaciv.Command.Commands;

namespace Xcaciv.Command.Tests.Commands
{
    public class SayCommandTests
    {

        [Fact()]
        public async Task HandleExecutionTest()
        {
            var commands = new CommandController(new Crawler(), "");
            commands.EnableDefaultCommands();

            var textio = new TestImpementations.TestTextIo();
            // simulate user input
            await commands.Run("say what is up", textio);

            // verify the output of the first run
            // by looking at the output of the second output line
            Assert.Equal("what is up", textio.Children.First().Output.First());
        }

        [Fact()]
        public void ProcessEnvValuesTest()
        {
            var textio = new TestImpementations.TestTextIo();
            textio.SetValue("direction", "up");

            var actual = SayCommand.ProcessEnvValues("what is %direction%!", textio);

            Assert.Equal("what is up!", actual);
        }

        [Fact()]
        public async Task HandleExecutionWithEnvTest()
        {
            var commands = new CommandController(new Crawler(), "");
            commands.EnableDefaultCommands();

            var textio = new TestImpementations.TestTextIo();
            textio.SetValue("direction", "up");
            // simulate user input
            await commands.Run(@"say ""what is %direction%!""", textio);

            // verify the output of the first run
            // by looking at the output of the second output line
            Assert.Equal("what is up!", textio.Children.First().Output.First());
        }
    }
}