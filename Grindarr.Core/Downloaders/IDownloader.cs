using System;

namespace Grindarr.Core.Downloaders
{
    public interface IDownloader
    {
        public event EventHandler<DownloadEventArgs> DownloadComplete;
        public event EventHandler<DownloadEventArgs> DownloadFailed;
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;

        public DownloadItem CurrentDownloadItem { get; }

        public void SetItem(DownloadItem item);

        public void Start();
        public void Pause();
        public void Resume();
        public void Cancel();

        public DownloadProgress GetProgress();
    }
}
