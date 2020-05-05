using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Grindarr.Core.Downloaders
{
    public class DownloadManager : IDownloadManager
    {
        private static DownloadManager _instance = null;
        public static DownloadManager Instance => _instance ??= new DownloadManager();

        public int MaxSimultaneousDownloads { get; set; } = 5;
        public bool? IgnoreStalledDownloads => Config.Instance.GetIgnoreStalledDownloads();
        public double? StalledDownloadCutoff => Config.Instance.GetStalledDownloadsCutoff();
        public IEnumerable<DownloadItem> DownloadQueue => downloads.Keys;

        private readonly Dictionary<DownloadItem, IDownloader> downloads = new Dictionary<DownloadItem, IDownloader>();

        public event EventHandler<DownloadEventArgs> DownloadCompleted;
        public event EventHandler<DownloadEventArgs> DownloadFailed;
        public event EventHandler<DownloadEventArgs> DownloadAdded;
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;

        private void UpdateActiveDownloads()
        {
            if (GetActiveDownloads().Count() < MaxSimultaneousDownloads)
            {
                var target = DownloadQueue.Where((di) => di.Progress.Status == DownloadStatus.Pending).FirstOrDefault();
                if (target != null)
                    GetExistingDownload(target).Start();
            }
            else if (GetActiveDownloads().Count() > MaxSimultaneousDownloads)
            {
                var target = DownloadQueue.Where((di) => di.Progress.Status == DownloadStatus.Downloading).FirstOrDefault();
                if (target != null)
                    GetExistingDownload(target).Pause();
            }
        }

        private void IDownloader_DownloadProgressChanged(object sender, DownloadEventArgs e)
        {
            UpdateActiveDownloads();
            DownloadProgressChanged?.Invoke(sender, e);
        }

        private void IDownloader_DownloadCompleted(object sender, DownloadEventArgs e)
        {
            UpdateActiveDownloads();
            downloads.Remove(e.Target);
            PostProcessors.PostProcessorManager.Instance.Run(e.Target); // TODO: move this somewhere better?
            DownloadCompleted?.Invoke(sender, e);
        }

        private void IDownloader_DownloadFailed(object sender, DownloadEventArgs e)
        {
            UpdateActiveDownloads();
            downloads.Remove(e.Target);
            File.Delete(e.Target.GetDownloadingPath());
            DownloadFailed?.Invoke(sender, e);
        }

        private void InternalAddDownload(DownloadItem item)
        {
            // Create and store downloader
            IDownloader dl = DownloaderFactory.CreateFrom(item.DownloadUri);
            dl.SetItem(item);
            downloads[item] = dl;

            // Register events
            dl.DownloadProgressChanged += IDownloader_DownloadProgressChanged;
            dl.DownloadComplete += IDownloader_DownloadCompleted;
            dl.DownloadFailed += IDownloader_DownloadFailed;

            // Fire event
            DownloadAdded?.Invoke(this, new DownloadEventArgs(item));

            UpdateActiveDownloads();
        }

        public IEnumerable<DownloadItem> GetActiveDownloads()
        {
            var res = downloads.Values
                .Where((val) => val.CurrentDownloadItem.Progress.Status == DownloadStatus.Downloading);

            // Filter stalled downloads if requested
            if (IgnoreStalledDownloads.HasValue && IgnoreStalledDownloads.Value)
                res = res.Where((p) => p.CurrentDownloadItem.Progress.SpeedTracker.GetBytesPerSecond() > StalledDownloadCutoff);

            return res.Select((s) => s.CurrentDownloadItem);
        }

        public void Cancel(DownloadItem item)
        {
            downloads[item].Cancel();
        }

        public void CancelAll()
        {
            foreach (var val in downloads)
                Cancel(val.Key);
        }

        public DownloadItem GetById(Guid id) => downloads.Keys.Where(dl => dl.Id == id).FirstOrDefault();

        private IDownloader GetExistingDownload(DownloadItem item)
        {
            if (downloads.ContainsKey(item))
                return downloads[item];
            throw new KeyNotFoundException();
        }

        public void Enqueue(DownloadItem item)
        {
            InternalAddDownload(item);
        }

        public DownloadProgress GetProgress(DownloadItem item)
        {
            return GetExistingDownload(item).CurrentDownloadItem.Progress;
        }

        public void Pause(DownloadItem item)
        {
            GetExistingDownload(item).Pause();
        }

        public void PauseAll()
        {
            foreach (var item in downloads)
                if (item.Key.Progress.Status == DownloadStatus.Pending || item.Key.Progress.Status == DownloadStatus.Downloading)
                    Pause(item.Key);
        }

        public void Resume(DownloadItem item)
        {
            GetExistingDownload(item).Resume();
        }

        public void ResumeAll()
        {
            foreach (var item in downloads)
                if (item.Key.Progress.Status == DownloadStatus.Paused)
                    Resume(item.Key);
        }
    }
}
