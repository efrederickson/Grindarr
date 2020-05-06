using System;
using System.Collections.Generic;
using System.Linq;

namespace Grindarr.Core
{
    public class ContentItem
    {
        /// <summary>
        /// The title or name of the file / content
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The DateTime the content was posted
        /// </summary>
        public DateTime? DatePosted { get; set; }

        /// <summary>
        /// The source web page (not the download link)
        /// </summary>
        public Uri Source { get; set; }

        /// <summary>
        /// A list of available download links (for example, if the source has
        /// links for Mega, Dropbox, or direct). 
        /// </summary>
        public HashSet<Uri> DownloadLinks { get; set; } = new HashSet<Uri>();

        /// <summary>
        /// The reported size of the content from the source. May not be an accurate size.
        /// </summary>
        public ulong ReportedSizeInBytes { get; set; }
    }
}
