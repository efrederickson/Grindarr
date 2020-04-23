using System;
using System.Collections.Generic;

namespace Grindarr.Core.Downloaders
{
    public interface IDownloadManager
    {
        public int MaxSimultaneousDownloads { get; set; }
        public IEnumerable<DownloadItem> DownloadQueue { get; }

        public event EventHandler<DownloadEventArgs> DownloadCompleted;
        public event EventHandler<DownloadEventArgs> DownloadFailed;
        public event EventHandler<DownloadEventArgs> DownloadAdded;

        public DownloadItem GetById(Guid id);
        public DownloadProgress GetProgress(DownloadItem item);

        public void Enqueue(DownloadItem item);

        public void Pause(DownloadItem item);
        public void PauseAll();
        public void Resume(DownloadItem item);
        public void ResumeAll();
        public void Cancel(DownloadItem item);
        public void CancelAll();
    }
}
