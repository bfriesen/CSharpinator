using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpinator.Core.Tests
{
    public class JsonTests : CompileTestBase
    {
        [Test]
        public void HiThere()
        {
            var instance = CreateObject("namespace n{public class c{public int i;}}", "n.c");
            instance.i = 100;
            Assert.That(instance.i, Is.EqualTo(100));
        }

        [Test]
        public void HiThereAgain()
        {
            var instance = CreateObject("namespace n{public class c{public int i;}}", "n.c");
            instance.i = -100;
            Assert.That(instance.i, Is.EqualTo(-100));
        }
    }
}
