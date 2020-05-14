﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Grindarr.Core
{
    /// <summary>
    /// Provides a bare, generic implementation of a download item to avoid duplicating effort
    /// </summary>
    public abstract class DownloadItemBase : IDownloadItem
    {
        /// <summary>
        /// The <code>ContentItem</code> that this object was created for
        /// </summary>
        public IContentItem Content { get; set; }

        /// <summary>
        /// The selected download uri for this download item - it may not be one in the <code>Content</code>, due to it being transformed by downloaders
        /// or the like into a "more correct" Uri
        /// </summary>
        public Uri DownloadUri { get; set; }

        /// <summary>
        /// Unique ID to identify this download
        /// </summary>
        public Guid Id { get; set; }

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

        public DownloadItemBase(IContentItem item, Uri dlUri)
        {
            Content = item;
            DownloadUri = dlUri;

            DownloadingFilename = HttpUtility.UrlDecode(dlUri.Segments.Last());
            CompletedFilename = DownloadingFilename;

            Id = Guid.NewGuid();
        }

        public DownloadItemBase()
        {

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
