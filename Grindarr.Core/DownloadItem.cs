﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace Grindarr.Core
{
    public class DownloadItem
    {
        /// <summary>
        /// The <code>ContentItem</code> that this object was created for
        /// </summary>
        public ContentItem Content { get; protected set; }

        /// <summary>
        /// The selected download uri for this download item - it may not be one in the <code>Content</code>, due to it being transformed by downloaders
        /// or the like into a "more correct" Uri
        /// </summary>
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

        /// <summary>
        /// The progress of the download, contains size, status, progress, speed, etc.
        /// </summary>
        public DownloadProgress Progress { get; set; }

        public DownloadItem(ContentItem item, Uri dlUri)
        {
            Content = item;
            DownloadUri = dlUri;

            DownloadingFilename = HttpUtility.UrlDecode(dlUri.Segments.Last());
            CompletedFilename = DownloadingFilename;

            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Returns the full path where this item will download to while in progress
        /// </summary>
        /// <returns></returns>
        public string GetDownloadingPath()
        {
            return Path.Join(Config.Instance.GetInProgressDownloadsFolder(), DownloadingFilename);
        }

        /// <summary>
        /// Returns the full path where the completed download will be (whether it be moved here upon completion or reside upon completion for post processors)
        /// </summary>
        /// <returns></returns>
        public string GetCompletedPath()
        {
            return Path.Join(Config.Instance.GetCompleteDownloadsFolder(), CompletedFilename);
        }
    }
}
