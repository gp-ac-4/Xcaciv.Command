using Xunit;
using Moq;
using Xcaciv.Command.Commands;
using Xcaciv.Command.Interface;
using System;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command.Tests
{
    public class PrameterParsingTests
    {
        [Fact]
        public void HandleExecution_ShouldReturnCorrectResult()
        {
            // Arrange
            var parameters = new string[] { "param1", "-flag", "--name", "John", "suffix" };
            var env = new Mock<IEnvironmentContext>().Object;
            var command = new Commands.ParameterTestCommand(
                [new CommandParameterOrderedAttribute("first", "a param")], 
                [new CommandFlagAttribute("flag", "a flag value")],
                [new CommandParameterNamedAttribute("--name", "a name value")], 
                [new CommandParameterSuffixAttribute("unnamed", "a unnamed value")]);

            // Act
            var result = command.HandleExecution(parameters, env);

            // Assert
            Assert.Contains("first = param1", result);
            Assert.Contains("flag = True", result);
            Assert.Contains("name = John", result);
            Assert.Contains("unnamed = suffix", result);
        }

        [Fact]
        public void HandlePipedChunk_ShouldThrowNotImplementedException()
        {
            // Arrange
            var pipedChunk = "piped chunk";
            var parameters = new string[] { "param1", "-flag", "value", "--name", "John", "suffix" };
            var env = new Mock<IEnvironmentContext>().Object;
            var command = new Commands.ParameterTestCommand([], [], [], []);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => command.HandlePipedChunk(pipedChunk, parameters, env));
        }
    }
}
