using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.FileLoader;
using Xcaciv.Command.Commands;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command.Tests.Commands
{
    public class SayCommandTests
    {

        [Fact()]
        public async Task HandleExecutionTest()
        {
            var commands = new CommandController(new Crawler(), AppContext.BaseDirectory);
            commands.RegisterBuiltInCommands();

            var env = new EnvironmentContext();
            var textio = new TestImpementations.TestTextIo();
            // simulate user input
            await commands.Run("say what is up", textio, env);

            // verify the output of the first run
            // by looking at the output of the second output line
            Assert.Equal("what is up", textio.Children.First().Output.First());
        }

        [Fact()]
        public void ProcessEnvValuesTest()
        {
            var env = new EnvironmentContext();
            var textio = new TestImpementations.TestTextIo();
            env.SetValue("direction", "up");

            var actual = SayCommand.ProcessEnvValues("what is %direction%!", env);

            Assert.Equal("what is up!", actual);
        }

        [Fact()]
        public async Task HandleExecutionWithEnvTest()
        {
            var commands = new CommandController(new Crawler(), AppContext.BaseDirectory);
            commands.RegisterBuiltInCommands();

            var env = new EnvironmentContext();
            var textio = new TestImpementations.TestTextIo();
            env.SetValue("direction", "up");
            // simulate user input
            await commands.Run(@"say ""what is %direction%!""", textio, env);

            // verify the output of the first run
            // by looking at the output of the second output line
            Assert.Equal("what is up!", textio.Children.First().Output.First());
        }

        [Fact()]
        public void BaseAttributeTest()
        {
            var attributes = Attribute.GetCustomAttribute(typeof(SayCommand), typeof(CommandRegisterAttribute)) as CommandRegisterAttribute;

            Assert.NotNull(attributes);
            Assert.Equal("Like echo but more valley.", attributes.Description);
        }

        [Fact()]
        public void ParameterAttributeTest()
        {

            var attributes = Attribute.GetCustomAttributes(typeof(SayCommand), typeof(CommandParameterOrderedAttribute)) as CommandParameterOrderedAttribute[];

            Assert.NotNull(attributes);
            Assert.NotEmpty(attributes);
            Assert.Equal("text", attributes.First().Name);
        }

        [Fact()]
        public void MultipleParameterAttributeTest()
        {

            var attributes = Attribute.GetCustomAttributes(typeof(SayCommand), typeof(CommandHelpRemarksAttribute)) as CommandHelpRemarksAttribute[];

            Assert.NotNull(attributes);
            Assert.NotEmpty(attributes);
            Assert.Equal(2, attributes.Length);
        }

        // test the one line help string
        [Fact()]
        public void OneLineHelpTest()
        {
            var textio = new TestImpementations.TestTextIo();
            var command = new SayCommand();
            
            var result = command.OneLineHelp(textio.Parameters);

            Assert.Equal("SAY          Like echo but more valley.", result);
        }
    }
}
