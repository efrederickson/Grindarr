using System;

namespace Grindarr.Core.Downloaders
{
    /// <summary>
    /// Interface for classes that are used to download a Uri
    /// </summary>
    public interface IDownloader
    {
        /// <summary>
        /// This event is raised when the download successfully completes
        /// </summary>
        public event EventHandler<DownloadEventArgs> DownloadComplete;

        /// <summary>
        /// This event is raised when the download fails or is cancelled
        /// </summary>
        public event EventHandler<DownloadEventArgs> DownloadFailed;

        /// <summary>
        /// This event is raised when the download progress (or state) changes
        /// </summary>
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;

        /// <summary>
        /// The item currently be downloaded by this downloader
        /// </summary>
        public DownloadItem CurrentDownloadItem { get; }

        /// <summary>
        /// Reset this downloader to download the specified item
        /// </summary>
        /// <param name="item">The new item to download</param>
        public void SetItem(DownloadItem item);

        /// <summary>
        /// Start the download
        /// </summary>
        public void Start();

        /// <summary>
        /// Pause the download
        /// </summary>
        public void Pause();

        /// <summary>
        /// Resume the download
        /// </summary>
        public void Resume();

        /// <summary>
        /// Cancel the download
        /// </summary>
        public void Cancel();
    }
}
