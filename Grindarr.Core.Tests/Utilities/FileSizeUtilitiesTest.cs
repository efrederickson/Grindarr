using Grindarr.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grindarr.Core.Tests.Utilities
{
    [TestClass]
    public class FileSizeUtilitiesTest
    {
        [TestMethod]
        public void TestSizing()
        {
            Assert.AreEqual(FileSizeUtilities.ParseFromSuffixedString("1MB"), (ulong)1024 * 1024);
            Assert.AreEqual(FileSizeUtilities.ParseFromSuffixedString("4k"), (ulong)1024 * 4);
            Assert.AreEqual(FileSizeUtilities.ParseFromSuffixedString("23gb"), (ulong)1024 * 1024 * 1024 * 23);
            Assert.AreEqual(FileSizeUtilities.ParseFromSuffixedString("7.2MB"), (ulong)Math.Floor(1024 * 1024 * 7.2));
            Assert.AreEqual(FileSizeUtilities.ParseFromSuffixedString("14b"), (ulong)1 * 14);
        }
    }
}
