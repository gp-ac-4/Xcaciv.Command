using Xunit;
using Xcaciv.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using System.IO.Abstractions;
using Xcaciv.Command.FileLoader;
using System.IO.Abstractions.TestingHelpers;
using Moq;

namespace Xcaciv.Command.Tests
{
    public class CommandControllerTests
    {
        private ITestOutputHelper _testOutput;
        private string commandPackageDir = @"..\..\..\..\zTestCommandPackage\bin\{1}\";
        public CommandControllerTests(ITestOutputHelper output)
        {
            _testOutput = output;
#if DEBUG
            _testOutput.WriteLine("Tests in Debug mode");
            commandPackageDir = commandPackageDir.Replace("{1}", "Debug");
#else
            this._testOutput.WriteLine("Tests in Release mode??");
            this.commandPackageDir = commandPackageDir.Replace("{1}", "Release");
#endif
        }
        [Fact()]
        public async Task RunCommandsTestAsync()
        {
            var commands = new CommandController(new Crawler(), @"..\..\..\..\..\");
            commands.AddPackageDirectory(commandPackageDir);

            commands.LoadCommands(string.Empty);
            var textio = new TestImpementations.TestTextIo();
            // simulate user input
            await commands.Run("echo what is up", textio);

            // verify the output of the first run
            // by looking at the output of the second output line
            Assert.Equal("> what", textio.Children.First().Output[1]);
        }
        [Fact()]
        public async Task PipeCommandsTestAsync()
        {
            var commands = new CommandController(new Crawler(), @"..\..\..\..\..\");
            commands.AddPackageDirectory(commandPackageDir);

            commands.LoadCommands(string.Empty);
            var textio = new TestImpementations.TestTextIo();
            // simulate user input
            await commands.Run("echo what is up | echo2 | echoe ", textio);

            // verify the output of the first run
            // by looking at the output of the second output line
            Assert.Equal("> :d2hhdC13aGF0:-:d2hhdC13aGF0:-> :aXMtaXM=:-:aXMtaXM=:-> :dXAtdXA=:-:dXAtdXA=:", textio.ToString());
        }

        [Fact()]
        public void LoadDefaultCommandsTest()
        {
            var commands = new CommandController(new Crawler(), @"..\..\..\..\..\") as ICommandController;
            commands.LoadDefaultCommands();

            var textio = new TestImpementations.TestTextIo();
            commands.GetHelp(string.Empty, textio);
            
            // Note: currently Loader is not unloading assemblies for performance reasons
            Assert.Contains("REGIF", textio.ToString());
        }
    }
}