using Xunit;
using Xcaciv.Command;
using Xcaciv.Command.Tests.TestImpementations;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xcaciv.Command.Tests
{
    public class PipelineBackpressureTests
    {
        private ITestOutputHelper _testOutput;

        public PipelineBackpressureTests(ITestOutputHelper output)
        {
            _testOutput = output;
        }

        /// <summary>
        /// Test: Default pipeline configuration uses bounded channels
        /// </summary>
        [Fact]
        public void DefaultPipelineConfiguration_IsBounded()
        {
            // Arrange
            var config = new PipelineConfiguration();

            // Assert
            Assert.Equal(10_000, config.MaxChannelQueueSize);
            Assert.Equal(PipelineBackpressureMode.DropOldest, config.BackpressureMode);
            Assert.Equal(0, config.ExecutionTimeoutSeconds);
        }

        /// <summary>
        /// Test: Pipeline configuration can be customized
        /// </summary>
        [Fact]
        public void PipelineConfiguration_CanBeCustomized()
        {
            // Arrange & Act
            var config = new PipelineConfiguration
            {
                MaxChannelQueueSize = 5_000,
                BackpressureMode = PipelineBackpressureMode.DropNewest,
                ExecutionTimeoutSeconds = 30
            };

            // Assert
            Assert.Equal(5_000, config.MaxChannelQueueSize);
            Assert.Equal(PipelineBackpressureMode.DropNewest, config.BackpressureMode);
            Assert.Equal(30, config.ExecutionTimeoutSeconds);
        }

        /// <summary>
        /// Test: CommandController has pipeline configuration property
        /// </summary>
        [Fact]
        public void CommandController_HasPipelineConfiguration()
        {
            // Arrange
            var controller = new CommandController();

            // Act
            var config = controller.PipelineConfig;

            // Assert
            Assert.NotNull(config);
            Assert.Equal(10_000, config.MaxChannelQueueSize);
        }

        /// <summary>
        /// Test: Pipeline configuration can be set on CommandController
        /// </summary>
        [Fact]
        public void CommandController_PipelineConfiguration_CanBeSet()
        {
            // Arrange
            var controller = new CommandController();
            var customConfig = new PipelineConfiguration
            {
                MaxChannelQueueSize = 1_000,
                BackpressureMode = PipelineBackpressureMode.Block
            };

            // Act
            controller.PipelineConfig = customConfig;

            // Assert
            Assert.Same(customConfig, controller.PipelineConfig);
            Assert.Equal(1_000, controller.PipelineConfig.MaxChannelQueueSize);
            Assert.Equal(PipelineBackpressureMode.Block, controller.PipelineConfig.BackpressureMode);
        }

        /// <summary>
        /// Test: Pipeline execution with custom configuration still works
        /// </summary>
        [Fact]
        public async Task PipelineExecution_WithCustomConfiguration_WorksAsync()
        {
            // Arrange
            var controller = new CommandController();
            controller.EnableDefaultCommands();
            controller.PipelineConfig = new PipelineConfiguration
            {
                MaxChannelQueueSize = 5_000,
                BackpressureMode = PipelineBackpressureMode.DropOldest
            };
            var env = new EnvironmentContext();
            var ioContext = new TestTextIo(new[] { "hello" });

            // Act
            await controller.Run("say hello | say world", ioContext, env);

            // Assert - Should execute without error
            Assert.NotNull(ioContext.Output);
        }

        /// <summary>
        /// Test: DropOldest backpressure mode configuration
        /// </summary>
        [Fact]
        public void BackpressureMode_DropOldest_Configured()
        {
            // Arrange
            var mode = PipelineBackpressureMode.DropOldest;

            // Assert
            Assert.Equal(0, (int)mode);
        }

        /// <summary>
        /// Test: DropNewest backpressure mode configuration
        /// </summary>
        [Fact]
        public void BackpressureMode_DropNewest_Configured()
        {
            // Arrange
            var mode = PipelineBackpressureMode.DropNewest;

            // Assert
            Assert.Equal(1, (int)mode);
        }

        /// <summary>
        /// Test: Block backpressure mode configuration
        /// </summary>
        [Fact]
        public void BackpressureMode_Block_Configured()
        {
            // Arrange
            var mode = PipelineBackpressureMode.Block;

            // Assert
            Assert.Equal(2, (int)mode);
        }

        /// <summary>
        /// Test: Multiple pipeline configurations don't interfere
        /// </summary>
        [Fact]
        public async Task MultiplePipelineConfigurations_DontInterfereAsync()
        {
            // Arrange
            var controller1 = new CommandController();
            controller1.EnableDefaultCommands();
            controller1.PipelineConfig.MaxChannelQueueSize = 5_000;

            var controller2 = new CommandController();
            controller2.EnableDefaultCommands();
            controller2.PipelineConfig.MaxChannelQueueSize = 2_000;

            var env = new EnvironmentContext();
            var ioContext1 = new TestTextIo(new[] { "test1" });
            var ioContext2 = new TestTextIo(new[] { "test2" });

            // Act
            await controller1.Run("say test1", ioContext1, env);
            await controller2.Run("say test2", ioContext2, env);

            // Assert
            Assert.NotNull(ioContext1.Output);
            Assert.NotNull(ioContext2.Output);
            Assert.Equal(5_000, controller1.PipelineConfig.MaxChannelQueueSize);
            Assert.Equal(2_000, controller2.PipelineConfig.MaxChannelQueueSize);
        }

        /// <summary>
        /// Test: Pipeline configuration with complex pipeline still works
        /// </summary>
        [Fact]
        public async Task PipelineConfiguration_WithComplexPipeline_WorksAsync()
        {
            // Arrange
            var controller = new CommandController();
            controller.EnableDefaultCommands();
            controller.PipelineConfig = new PipelineConfiguration
            {
                MaxChannelQueueSize = 1_000,
                BackpressureMode = PipelineBackpressureMode.DropNewest
            };
            var env = new EnvironmentContext();
            var ioContext = new TestTextIo();

            // Act
            await controller.Run("say a | say b | say c | say d", ioContext, env);

            // Assert
            Assert.NotNull(ioContext.Output);
        }
    }
}
