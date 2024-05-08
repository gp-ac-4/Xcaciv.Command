using Xunit;
using zTestCommandPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assert = Xunit.Assert;

namespace zTestCommandPackage.Tests
{
    public class EchoCommandTests
    {
        [Fact()]
        public async void EchoTest()
        {
            var context = new TestImplementations.TestTextIo(["just", "a", "test"]);
            var echo = new EchoCommand();
            await foreach(var result  in echo.Main(context, context))
            {
                await context.OutputChunk(result);
            }

            Assert.Equal(5, context.Output.Count);
        }

        [Fact()]
        public async void EchoEncodedTest()
        {
            var context = new TestImplementations.TestTextIo(["just", "a", "test"]);
            var echo = new EchoEncodeCommand();
            await foreach (var result in echo.Main(context, context))
            {
                await context.OutputChunk(result);
            }

            Assert.Equal(":anVzdA==:", context.Output.Skip(1).First());
        }

        [Fact()]
        public async void EchoEnChamberTest()
        {
            var context = new TestImplementations.TestTextIo(["just", "a", "test"]);
            var echo = new EchoChamberCommand();
            await foreach (var result in echo.Main(context, context))
            {
                await context.OutputChunk(result);
            }

            Assert.Equal("just-just", context.Output.Skip(1).First());
        }
    }
}