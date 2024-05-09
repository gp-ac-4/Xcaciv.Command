using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.FileLoader;

namespace Xcaciv.Command.Tests.Commands
{
    public class SayCommandTests
    {

        [Fact()]
        public async Task HandleExecutionTest()
        {
            var commands = new CommandController(new Crawler(), "");
            commands.LoadDefaultCommands();

            var textio = new TestImpementations.TestTextIo();
            // simulate user input
            await commands.Run("say what is up", textio);

            // verify the output of the first run
            // by looking at the output of the second output line
            Assert.Equal("> what is up", textio.Children.First().Output[1]);
        }
    }
}