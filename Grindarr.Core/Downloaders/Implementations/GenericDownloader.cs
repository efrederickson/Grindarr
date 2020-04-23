using Grindarr.Core.Net;
using System;
using System.IO;

namespace Grindarr.Core.Downloaders.Implementations
{
    public class GenericDownloader : IDownloader
    {
        private PausableEventedDownloader downloader;

        public DownloadItem CurrentDownloadItem { get; private set; }

        public event EventHandler<DownloadEventArgs> DownloadComplete;
        public event EventHandler<DownloadEventArgs> DownloadFailed;
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;

        public void Cancel()
        {
            downloader.Stop();
            CurrentDownloadItem.Progress.Status = DownloadStatus.Canceled;
            DownloadFailed?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
        }

        public DownloadProgress GetProgress()
        {
            return CurrentDownloadItem.Progress;
        }

        public void Pause()
        {
            if (CurrentDownloadItem.Progress.Status != DownloadStatus.Downloading)
                throw new InvalidOperationException();

            downloader.Pause();
            CurrentDownloadItem.Progress.Status = DownloadStatus.Paused;
            DownloadProgressChanged?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
        }

        public void Resume()
        {
            if (CurrentDownloadItem.Progress.Status != DownloadStatus.Paused)
                throw new InvalidOperationException();

            downloader.Resume();
            CurrentDownloadItem.Progress.Status = DownloadStatus.Downloading;
            DownloadProgressChanged?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
        }

        public void SetItem(DownloadItem item)
        {
            CurrentDownloadItem = item;
            downloader = new PausableEventedDownloader(item.DownloadUri, item.GetDownloadingPath());
            downloader.ProgressChanged += Downloader_ProgressChanged;
            downloader.StatusChanged += Downloader_StatusChanged;
            downloader.ReceivedResponseFilename += Downloader_ReceivedResponseFilename;
            item.Progress = DownloadProgress.Create(0, 0, DownloadStatus.Pending);
        }

        private void Downloader_ReceivedResponseFilename(object sender, ResponseFilenameEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Filename))
                CurrentDownloadItem.CompletedFilename = e.Filename;
        }

        private void Downloader_StatusChanged(object sender, EventArgs e)
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

        private void Downloader_ProgressChanged(object sender, EventArgs e)
        {
            CurrentDownloadItem.Progress.BytesDownloaded = downloader.Progress;
            CurrentDownloadItem.Progress.BytesTotal = downloader.Size;
            CurrentDownloadItem.Progress.SpeedTracker.SetProgress(downloader.Progress);

            DownloadProgressChanged?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
        }

        public void Start()
        {
            if (CurrentDownloadItem.Progress.Status != DownloadStatus.Pending)
                throw new InvalidOperationException();

            downloader.StartDownloadAsync();
            CurrentDownloadItem.Progress.Status = DownloadStatus.Downloading;
        }
    }
}
