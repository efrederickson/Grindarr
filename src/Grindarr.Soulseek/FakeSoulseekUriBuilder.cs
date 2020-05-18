using Grindarr.Core;
using Grindarr.Core.Logging;
using Soulseek;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Grindarr.Soulseek
{
    public static class FakeSoulseekUriBuilder
    {
        /// <summary>
        /// Builds a "fake" uri with the required metadata that Grindarr needs to parse in the future to reconstruct a download
        /// </summary>
        /// <param name="soulseekUsername">The user on soulseek who is hosting the file</param>
        /// <param name="file">The soulseek file in question</param>
        /// <returns></returns>
        public static Uri BuildFrom(string soulseekUsername, File file)
        {
            var urlSafeUsername = HttpUtility.UrlEncode(soulseekUsername);
            var urlSafeFilename = HttpUtility.UrlEncode(file.Filename);
            var fakeLink = $"slsk://{urlSafeUsername}@soulseek/{urlSafeFilename}";
            if (!Uri.TryCreate(fakeLink, UriKind.Absolute, out Uri fakeUri))
            {
                Log.WriteLine($"Failed to create fake slsk url: {fakeLink}");
            }
            return fakeUri;
        }

        /// <summary>
        /// Does the opposite of BuildFrom - it takes a faked uri and creates a downloadable item
        /// </summary>
        /// <param name="fakedUri"></param>
        /// <returns></returns>
        public static SoulseekDownloadItem DeconstructFrom(Uri fakedUri)
        {
            // Do some validation checks
            if (fakedUri.DnsSafeHost.ToLower() != "soulseek" && fakedUri.Scheme.ToLower() != "slsk")
                throw new ArgumentException("download item is not a valid soulseek download item");

            var newItem = new SoulseekDownloadItem();

            var ssFn = string.Join("", fakedUri.Segments);
            if (ssFn.StartsWith("/")) // Trim the '/' at the start from the first segment
                ssFn = ssFn.Substring(1);

            newItem.SoulseekFilename = HttpUtility.UrlDecode(ssFn);
            newItem.SoulseekUsername = HttpUtility.UrlDecode(fakedUri.UserInfo);

            return newItem;
        }

        /// <summary>
        /// Does the opposite of BuildFrom - it takes a faked uri and creates a downloadable item.
        /// This also copies all of the other properties from the IDownloadItem into the new SoulseekDownloadItem
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static SoulseekDownloadItem DeconstructFrom(IDownloadItem item)
        {
            // Do the thing we're here for
            var newItem = DeconstructFrom(item.DownloadUri);

            // Copy all the other properties
            newItem.CompletedFilename = item.CompletedFilename;
            newItem.Content = item.Content;
            newItem.DownloadingFilename = item.DownloadingFilename;
            newItem.DownloadUri = item.DownloadUri;
            newItem.Id = item.Id;
            newItem.Progress = item.Progress;

            // Return the newly created item
            return newItem;
        }
    }
}
