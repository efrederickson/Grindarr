using Grindarr.Core.Net;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Grindarr.Core.Downloaders.Implementations
{
    public class GenericDownloader : IDownloader
    {
        protected PausableEventedDownloader downloader;

        public IDownloadItem CurrentDownloadItem { get; private set; }

        public event EventHandler<DownloadEventArgs> DownloadComplete;
        public event EventHandler<DownloadEventArgs> DownloadFailed;
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;

        public virtual void Cancel()
        {
            downloader.Stop();
            CurrentDownloadItem.Progress.Status = DownloadStatus.Canceled;
            DownloadFailed?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
        }

        public virtual void Pause()
        {
            if (CurrentDownloadItem.Progress.Status != DownloadStatus.Downloading && CurrentDownloadItem.Progress.Status != DownloadStatus.Pending)
                throw new InvalidOperationException();

            downloader.Pause();
            CurrentDownloadItem.Progress.Status = DownloadStatus.Paused;
            DownloadProgressChanged?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
        }

        public virtual void Resume()
        {
            if (CurrentDownloadItem.Progress.Status != DownloadStatus.Paused)
                throw new InvalidOperationException();

            downloader.Resume();
            CurrentDownloadItem.Progress.Status = DownloadStatus.Downloading;
            DownloadProgressChanged?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
        }

        public virtual void SetItem(IDownloadItem item) => SetItem(item, item.DownloadUri);

        protected virtual void SetItem(IDownloadItem item, Uri actualDownloadUri)
        {
            CurrentDownloadItem = item;
            item.DownloadingFilename = HttpUtility.UrlDecode(actualDownloadUri.Segments.Last());
            item.CompletedFilename = item.DownloadingFilename;

            downloader = new PausableEventedDownloader(actualDownloadUri, item.GetDownloadingPath());
            downloader.ProgressChanged += Downloader_ProgressChanged;
            downloader.StatusChanged += Downloader_StatusChanged;
            downloader.ReceivedResponseFilename += Downloader_ReceivedResponseFilename;
            item.Progress = new DownloadProgress
            {
                BytesTotal = 0,
                BytesDownloaded = 0,
                Status = DownloadStatus.Pending
            };

            // Now it's ready, invoke the handler to get download manager to start (or update)
            DownloadProgressChanged?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
        }

        protected void Downloader_ReceivedResponseFilename(object sender, ResponseFilenameEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Filename))
                CurrentDownloadItem.CompletedFilename = HttpUtility.UrlDecode(e.Filename);
        }

        protected void Downloader_StatusChanged(object sender, EventArgs e)
        {
            if (downloader.IsDone())
            {
                CurrentDownloadItem.Progress.Status = DownloadStatus.Completed;
                DownloadComplete?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
            }
            if (downloader.HasFailed())
            {
                CurrentDownloadItem.Progress.Status = DownloadStatus.Failed;
                DownloadFailed?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
            }
        }

        protected void Downloader_ProgressChanged(object sender, EventArgs e)
        {
            CurrentDownloadItem.Progress.BytesDownloaded = downloader.Progress;
            CurrentDownloadItem.Progress.BytesTotal = downloader.Size;
            CurrentDownloadItem.Progress.SpeedTracker.SetProgress(downloader.Progress);

            DownloadProgressChanged?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
        }

        public virtual void Start()
        {
            if (CurrentDownloadItem.Progress.Status != DownloadStatus.Pending)
                throw new InvalidOperationException();

            downloader.StartDownloadAsync();
            CurrentDownloadItem.Progress.Status = DownloadStatus.Downloading;
        }
    }
}
