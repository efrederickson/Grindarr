using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grindarr.Core.Tests
{
    [TestClass]
    public class DownloadItemTest
    {
        [TestMethod]
        public void TestConstructors()
        {
            var di = RandomFixtureFactory.CreateDownloadItem();
            Assert.IsTrue(di.DownloadingFilename.Length > 0);
        }
    }
}
