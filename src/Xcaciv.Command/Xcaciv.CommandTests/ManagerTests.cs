using Xunit;
using Xcaciv.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.CommandTests
{
    public class ManagerTests
    {
        [Fact()]
        public void GetCommandTest()
        {
            var expected = "DIR";
            var commandLine = $"{expected} - some options here";
            var manager = new CommandManager();

            var actual = CommandManager.GetCommand(commandLine);

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public void PrepareArgsTest()
        {
            var command = "DIR";
            var expected = new[] { "-some", "options", "here" };
            var commandLine = $"{command} " + String.Join(' ', expected);
            var manager = new CommandManager();

            var actual = CommandManager.PrepareArgs(commandLine);
            Assert.Equal(expected, actual);
        }

        [Fact()]
        public void GetCommand_HostileInput_Filters()
        {
            var expected = "DIR";
            var commandLine = $"{expected}*'`%^! -some options here";
            
            var actual = CommandManager.GetCommand(commandLine);

            Assert.Equal(expected, actual);
        }

        [Fact()]
        public void PrepareArgs_HostileInput_Filters()
        {
            var command = "DIR";
            var expected = new[] { "-some", "options", "here" };
            var commandLine = $"{command} *'`%^!" + String.Join(' ', expected);
            
            var actual = CommandManager.PrepareArgs(commandLine);
            Assert.Equal(expected, actual);
        }

        [Fact()]
        public void PrepareArgs_Quotes_Groups()
        {
            var command = "DIR";
            var expected = new[] { "-some", "\"two word\"", "and_three_word", "options" };
            var commandLine = $"{command} " + String.Join(' ', expected);
            
            var actual = CommandManager.PrepareArgs(commandLine);
            Assert.Equal(expected, actual);
        }
    }
}