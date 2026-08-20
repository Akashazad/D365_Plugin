using FakeXrmEasy;
using Xunit;

namespace D365Plugins.Tests
{
    public class SampleTests
    {
        [Fact]
        public void XrmFakedContext_CanBeInstantiated()
        {
            var context = new XrmFakedContext();

            Assert.NotNull(context);
        }
    }
}
