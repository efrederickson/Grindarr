using Grindarr.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grindarr.Soulseek
{
    /// <summary>
    /// Download item with specific fields for soulseek data
    /// </summary>
    public class SoulseekDownloadItem : DownloadItemBase, IDownloadItem
    {
        /// <summary>
        /// The username that the file belongs to
        /// </summary>
        public string SoulseekUsername { get; set; }

        /// <summary>
        /// The soulseek full filename, which may be different than the generated URI
        /// </summary>
        public string SoulseekFilename { get; set; }

        /// <summary>
        /// Given that download items are presently made as a generic <code>DownloadItem</code> from the API POST, this method
        /// will take that <code>DownloadItem</code> and parse the soulseek data from the <code>DownloadUri</code>
        /// </summary>
        /// <param name="item">Any download item with a soulseek-like Uri</param>
        /// <returns>The parsed download information</returns>
        public static SoulseekDownloadItem ParseFrom(IDownloadItem item)
        {
            if (item is SoulseekDownloadItem ssItem)
                return ssItem;

            if (item.DownloadUri.DnsSafeHost != "soulseek")
                throw new ArgumentException("download item is not a valid soulseek download item");

            var ssFn = System.Web.HttpUtility.UrlDecode(string.Join("", item.DownloadUri.Segments)).Replace("/", "\\");
            if (ssFn.StartsWith("\\"))
                ssFn = ssFn.Substring(1);

            SoulseekDownloadItem newItem = new SoulseekDownloadItem
            {
                CompletedFilename = item.CompletedFilename,
                Content = item.Content,
                DownloadingFilename = item.DownloadingFilename,
                DownloadUri = item.DownloadUri,
                Id = item.Id,
                Progress = item.Progress,
                SoulseekFilename = ssFn,
                SoulseekUsername = item.DownloadUri.UserInfo
            };
            return newItem;
        }
    }
}
