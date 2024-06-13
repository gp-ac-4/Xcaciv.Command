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
using Xcaciv.Command.Tests.TestImpementations;

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
            var controller = new CommandController(new Crawler(), @"..\..\..\..\..\");
            controller.AddPackageDirectory(commandPackageDir);

            controller.LoadCommands(string.Empty);
            var env = new EnvironmentContext();
            var textio = new TestImpementations.TestTextIo();
            // simulate user input
            await controller.Run("echo what is up", textio, env);

            // verify the output of the first run
            // by looking at the output of the second output line
            Assert.Equal("is", textio.Children.First().Output[1]);
        }
        [Fact()]
        public async Task RunSubCommandsTestAsync()
        {
            var controller = new CommandController(new Crawler(), @"..\..\..\..\..\");
            controller.AddPackageDirectory(commandPackageDir);

            controller.LoadCommands(string.Empty);
            var env = new EnvironmentContext();
            var textio = new TestImpementations.TestTextIo();
            // simulate user input
            await controller.Run("do echo what is up", textio, env);

            // verify the output of the first run
            // by looking at the output of the second output line
            Assert.Equal("what is up", textio.Children.First().Output.First());
        }
        [Fact()]
        public async Task PipeCommandsTestAsync()
        {
            var controller = new CommandController(new Crawler(), @"..\..\..\..\..\");
            controller.AddPackageDirectory(commandPackageDir);

            controller.LoadCommands(string.Empty);
            var env = new EnvironmentContext();
            var textio = new TestImpementations.TestTextIo();
            // simulate user input
            await controller.Run("echo what is up | echo2 | echoe ", textio, env);

            // verify the output of the first run
            // by looking at the output of the second output line
            Assert.Equal(":d2hhdC13aGF0:\r\n:aXMtaXM=:\r\n:dXAtdXA=:", textio.ToString());
        }
        [Fact()]
        public void LoadCommandsTest()
        {
            var controller = new CommandControllerTestHarness(new Crawler(), @"..\..\..\..\..\");
            controller.AddPackageDirectory(commandPackageDir);
            controller.EnableDefaultCommands();
            controller.LoadCommands(string.Empty);

            var commands = controller.GetCommands();

            Assert.Equal(2, commands["DO"]?.SubCommands.Count);
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        [Fact()]
        public void LoadDefaultCommandsTest()
        {
            var controller = new CommandController(new Crawler(), @"..\..\..\..\..\") as Interface.ICommandController;
            controller.EnableDefaultCommands();

            var textio = new TestImpementations.TestTextIo();
            controller.GetHelp(string.Empty, textio);
            
            // Note: currently Loader is not unloading assemblies for performance reasons
            Assert.Contains("REGIF", textio.ToString());
        }
        [Fact()]
        public void HelpCommandsTestAsync()
        {
            var controller = new CommandControllerTestHarness(new Crawler(), @"..\..\..\..\..\") as Interface.ICommandController;
            controller.EnableDefaultCommands();

            controller.AddPackageDirectory(commandPackageDir);
            controller.LoadCommands(string.Empty);

            var textio = new TestImpementations.TestTextIo();
            //var env = new EnvironmentContext();
            controller.GetHelp(string.Empty, textio);
            var output = textio.ToString();

            //BROKEN

            // Note: currently Loader is not unloading assemblies for performance reasons
            Assert.Contains("SUB DO say", output);
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}