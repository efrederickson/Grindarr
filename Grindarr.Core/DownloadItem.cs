using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Grindarr.Core
{
    public class DownloadItem
    {
        public ContentItem Content { get; protected set; }
        public Uri DownloadUri { get; }

        /// <summary>
        /// Unique ID to identify this download
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Filename (without path) to store the in-progress download
        /// </summary>
        public string DownloadingFilename { get; set; }

        /// <summary>
        /// Filename (without path) to store the completed download
        /// </summary>
        public string CompletedFilename { get; set; }

        public DownloadProgress Progress { get; set; }

        public DownloadItem(ContentItem item, Uri dlUri)
        {
            Content = item;
            DownloadUri = dlUri;

            DownloadingFilename = dlUri.Segments.Last();
            CompletedFilename = dlUri.Segments.Last();

            Id = Guid.NewGuid();
        }

        public string GetDownloadingPath()
        {
            return Path.Join(Config.Instance.InProgressDownloadsFolder, DownloadingFilename);
        }

        public string GetCompletedPath()
        {
            return Path.Join(Config.Instance.CompletedDownloadsFolder, CompletedFilename);
        }
    }
}
