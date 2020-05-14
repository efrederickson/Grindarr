using System;
using System.Collections.Generic;

namespace Grindarr.Core.Downloaders
{
    /// <summary>
    /// Provides the interface for a download manager
    /// </summary>
    public interface IDownloadManager
    {
        /// <summary>
        /// Max simultaneous downloads
        /// </summary>
        public int MaxSimultaneousDownloads { get; set; }

        /// <summary>
        /// The queue of download items, in their preferered order
        /// </summary>
        public IEnumerable<IDownloadItem> DownloadQueue { get; }

        public event EventHandler<DownloadEventArgs> DownloadCompleted;
        public event EventHandler<DownloadEventArgs> DownloadFailed;
        public event EventHandler<DownloadEventArgs> DownloadAdded;

        /// <summary>
        /// Returns the specified download, if that item belongs to this download manager
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IDownloadItem GetById(Guid id);

        /// <summary>
        /// Returns the download progress for a specified item, if that item belongs to this download manager
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public DownloadProgress GetProgress(IDownloadItem item);

        /// <summary>
        /// Adds a new download to the queue
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(IDownloadItem item);

        /// <summary>
        /// Pause a specified download, if that download belongs to this download manager
        /// </summary>
        /// <param name="item"></param>
        public void Pause(IDownloadItem item);

        /// <summary>
        /// Pauses all downloads
        /// </summary>
        public void PauseAll();

        /// <summary>
        /// Resume a specified download, if that download belongs to this download manager
        /// </summary>
        /// <param name="item"></param>
        public void Resume(IDownloadItem item);

        /// <summary>
        /// Resumes all downloads
        /// </summary>
        public void ResumeAll();

        /// <summary>
        /// Cancels a specified download, if that download belongs to this download manager
        /// </summary>
        /// <param name="item"></param>
        public void Cancel(IDownloadItem item);

        /// <summary>
        /// Cancels all downloads
        /// </summary>
        public void CancelAll();
    }
}
