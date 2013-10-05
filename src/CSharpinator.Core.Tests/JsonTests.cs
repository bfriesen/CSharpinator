using NUnit.Framework;
using System;
using System.Linq;

namespace CSharpinator.Core.Tests
{
    public class JsonTests : CompileTestBase
    {
        [Test]
        public void SinglePrimitiveProperty()
        {
            var document = "{\"foo\":\"bar\"}";

            var instance = CreateFromJson(document);

            Assert.That(instance.Foo, Is.EqualTo("bar"));
        }

        [Test]
        public void MultiplePrimitiveProperties()
        {
            var document = "{\"foo\":\"bar\",\"baz\":123}";

            var instance = CreateFromJson(document);

            Assert.That(instance.Foo, Is.EqualTo("bar"));
            Assert.That(instance.Baz, Is.EqualTo(123));
        }

        [Test]
        public void AllPrimitiveProperties()
        {
            var document = StripWhitespace(
@"{
    ""p1"":""abc"",
    ""p2"":123,
    ""p3"":1.23,
    ""p4"":true,
    ""p5"":""2013-10-05T12:45:08.4601194-04:00"",
    ""p6"":""0784e84e-f772-4c06-a51c-ecde31abfd2d""
}");

            var instance = CreateFromJson(document);

            Assert.That(instance.P1, Is.EqualTo("abc"));
            Assert.That(instance.P2, Is.EqualTo(123));
            Assert.That(instance.P3, Is.EqualTo(1.23));
            Assert.That(instance.P4, Is.EqualTo(true));
            Assert.That(instance.P5, Is.EqualTo(DateTime.Parse("2013-10-05T12:45:08.4601194-04:00")));
            Assert.That(instance.P6, Is.EqualTo(Guid.Parse("0784e84e-f772-4c06-a51c-ecde31abfd2d")));
        }

        [Test]
        public void PrimitiveArrayProperty()
        {
            var document = @"{""foos"":[1,2,3]}";

            var instance = CreateFromJson(document);

            Assert.That(instance.Foos, Is.EquivalentTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void AllPrimitiveArrayProperties()
        {
            var document = StripWhitespace(
@"{
    ""alphas"":[""abc""],
    ""betas"":[123],
    ""gammas"":[1.23],
    ""deltas"":[true],
    ""epsilons"":[""2013-10-05T12:45:08.4601194-04:00""],
    ""zetas"":[""0784e84e-f772-4c06-a51c-ecde31abfd2d""]
}");

            var instance = CreateFromJson(document);
            
            Assert.That(instance.Alphas, Is.EquivalentTo(new[] { "abc" }));
            Assert.That(instance.Betas, Is.EquivalentTo(new[] { 123 }));
            Assert.That(instance.Gammas, Is.EquivalentTo(new[] { 1.23 }));
            Assert.That(instance.Deltas, Is.EquivalentTo(new[] { true }));
            Assert.That(instance.Epsilons, Is.EquivalentTo(new[] { DateTime.Parse("2013-10-05T12:45:08.4601194-04:00") }));
            Assert.That(instance.Zetas, Is.EquivalentTo(new[] { Guid.Parse("0784e84e-f772-4c06-a51c-ecde31abfd2d") }));
        }
    }
}
