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
        /// Given that download items are presently made as a generic DownloadItem from the API POST, this method
        /// will take that and parse the soulseek data from the DownloadUri
        /// </summary>
        /// <param name="item">Any download item with a fake soulseek-like Uri</param>
        /// <returns>The parsed download information</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="item"/>does not contain a valid faked soulseek uri</exception>
        public static SoulseekDownloadItem ParseFrom(IDownloadItem item)
        {
            if (item is SoulseekDownloadItem ssItem)
                return ssItem;

            return FakeSoulseekUriBuilder.DeconstructFrom(item);
        }
    }
}
