using NUnit.Framework;

namespace CSharpinator.Core.Tests
{
    public class JsonTests : CompileTestBase
    {
        [Test]
        public void ExampleRoundTripTest()
        {
            var document = "{\"foo\":\"bar\"}";

            var instance = CreateObjectFromJson(document);

            Assert.That(instance.Foo, Is.EqualTo("bar"));
            Assert.That(SerializeJson(instance), Is.EqualTo(document));
        }
    }
}
