using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Soulseek;
using System.Web;

namespace Grindarr.Soulseek.Tests
{
    [TestClass]
    public class FakeSoulseekUriBuilderTest
    {
        [TestMethod]
        public void Test_BuildFrom()
        {
            var item = FakeSoulseekUriBuilder.BuildFrom("user", CreateFile("afile.txt"));
            Assert.AreEqual(item, new Uri("slsk://user@soulseek/afile.txt"));

            item = FakeSoulseekUriBuilder.BuildFrom("a user", CreateFile("afile.txt"));
            Assert.AreEqual(item, new Uri($"slsk://{HttpUtility.UrlEncode("a user")}@soulseek/afile.txt"));

            item = FakeSoulseekUriBuilder.BuildFrom("a user", CreateFile("a file.txt"));
            Assert.AreEqual(item, new Uri($"slsk://{HttpUtility.UrlEncode("a user")}@soulseek/{HttpUtility.UrlEncode("a file.txt")}"));

            item = FakeSoulseekUriBuilder.BuildFrom("a user", CreateFile("c:\\a file.txt"));
            Assert.AreEqual(item, new Uri($"slsk://{HttpUtility.UrlEncode("a user")}@soulseek/{HttpUtility.UrlEncode("c:\\a file.txt")}"));

            item = FakeSoulseekUriBuilder.BuildFrom("joe@schmoe", CreateFile("/media/nas/who/cares.txt"));
            Assert.AreEqual(item, new Uri($"slsk://{HttpUtility.UrlEncode("joe@schmoe")}@soulseek/{HttpUtility.UrlEncode("/media/nas/who/cares.txt")}"));

            item = FakeSoulseekUriBuilder.BuildFrom("Some > user", CreateFile(@"D:\Data Drive\This has Spaces\And CAPS\downloads\file.exe"));
            Assert.AreEqual(item, new Uri($"slsk://{HttpUtility.UrlEncode("Some > user")}@soulseek/{HttpUtility.UrlEncode(@"D:\Data Drive\This has Spaces\And CAPS\downloads\file.exe")}"));
        }

        [TestMethod]
        public void Test_DeconstructFrom()
        {
            var item = FakeSoulseekUriBuilder.DeconstructFrom(FakeSoulseekUriBuilder.BuildFrom("user", CreateFile("afile.txt")));
            Assert.AreEqual(item.SoulseekUsername, "user");
            Assert.AreEqual(item.SoulseekFilename, "afile.txt");

            item = FakeSoulseekUriBuilder.DeconstructFrom(FakeSoulseekUriBuilder.BuildFrom("a user", CreateFile("afile.txt")));
            Assert.AreEqual(item.SoulseekUsername, "a user");
            Assert.AreEqual(item.SoulseekFilename, "afile.txt");

            item = FakeSoulseekUriBuilder.DeconstructFrom(FakeSoulseekUriBuilder.BuildFrom("a user", CreateFile("a file.txt")));
            Assert.AreEqual(item.SoulseekUsername, "a user");
            Assert.AreEqual(item.SoulseekFilename, "a file.txt");

            item = FakeSoulseekUriBuilder.DeconstructFrom(FakeSoulseekUriBuilder.BuildFrom("a user", CreateFile("c:\\a file.txt")));
            Assert.AreEqual(item.SoulseekUsername, "a user");
            Assert.AreEqual(item.SoulseekFilename, "c:\\a file.txt");

            item = FakeSoulseekUriBuilder.DeconstructFrom(FakeSoulseekUriBuilder.BuildFrom("joe@schmoe", CreateFile("/media/nas/movies/the matrix.avi")));
            Assert.AreEqual(item.SoulseekUsername, "joe@schmoe");
            Assert.AreEqual(item.SoulseekFilename, "/media/nas/movies/the matrix.avi");

            item = FakeSoulseekUriBuilder.DeconstructFrom(FakeSoulseekUriBuilder.BuildFrom("Some > user", CreateFile(@"D:\Data Drive\This has Spaces\And CAPS\downloads\file.exe")));
            Assert.AreEqual(item.SoulseekUsername, "Some > user");
            Assert.AreEqual(item.SoulseekFilename, @"D:\Data Drive\This has Spaces\And CAPS\downloads\file.exe");
        }

        private static File CreateFile(string fn, long size = 0)
        {
            return new File(0, fn, size, System.IO.Path.GetExtension(fn), 0);
        }
    }
}
