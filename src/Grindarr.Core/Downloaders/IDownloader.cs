using System;

namespace Grindarr.Core.Downloaders
{
    /// <summary>
    /// Interface for classes that are used to download a Uri. Single-use for a specific download
    /// </summary>
    public interface IDownloader
    {
        /// <summary>
        /// This event is raised when the download status changes
        /// </summary>
        public event EventHandler<DownloadEventArgs> DownloadStatusChanged;

        /// <summary>
        /// This event is raised when the download progress changes
        /// </summary>
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;

        /// <summary>
        /// The item currently being downloaded by this downloader
        /// </summary>
        public IDownloadItem CurrentDownloadItem { get; }

        /// <summary>
        /// Configure the downloader to prepare to download the specified item
        /// </summary>
        /// <param name="item">The new item to download</param>
        public void SetItem(IDownloadItem item);

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
