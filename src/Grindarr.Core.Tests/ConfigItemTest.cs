using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Grindarr.Core.Tests
{
    [TestClass]
    public class ConfigItemTest
    {
        [TestMethod]
        public void TestConstructors()
        {
            var ci = RandomFixtureFactory.CreateContentItem();

            Assert.IsTrue(ci.DownloadLinks.Count > 0);
        }
    }
}
