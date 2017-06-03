using Xunit;

namespace CakeTry.BusinessLayer.Unit.Tests
{
    public class GreeterTest
    {
        [Fact]
        public void Hello_test()
        {
            var greeting = new Greeter().Greet("World");
            Assert.Equal("Hello, World", greeting);
        }
    }
}
