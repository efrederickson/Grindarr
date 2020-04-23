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
        public bool IgnoreStalledDownloads { get; set; } = true;
        public double StalledDownloadCutoff { get; set; } = 50; // 50 bytes per second
        public IEnumerable<DownloadItem> DownloadQueue
        {
            get => downloads.Keys;
        }

        private readonly Dictionary<DownloadItem, IDownloader> downloads = new Dictionary<DownloadItem, IDownloader>();

        public event EventHandler<DownloadEventArgs> DownloadCompleted;
        public event EventHandler<DownloadEventArgs> DownloadFailed;
        public event EventHandler<DownloadEventArgs> DownloadAdded;
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;

        private DownloadManager() { }

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
            PostProcessors.PostProcessorManager.Instance.Run(e.Target); // TODO: move this somewhere better
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
                .Where((val) => val.GetProgress().Status == DownloadStatus.Downloading);

            // Filter stalled downloads if requested
            if (IgnoreStalledDownloads)
                res = res.Where((p) => p.GetProgress().SpeedTracker.GetBytesPerSecond() > StalledDownloadCutoff);

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

        public DownloadItem GetById(Guid id)
        {
            var res = downloads.Keys.Where(dl => dl.Id == id);
            Console.WriteLine($"downloads: {downloads.Count}, matches: {res.Count()}");
            foreach (var k in downloads.Keys)
            {
                Console.WriteLine(k.Id);
                Console.WriteLine(k.Id.Equals(id));
            }
            Console.WriteLine(id);

                
                return res.FirstOrDefault();
        }

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
            return GetExistingDownload(item).GetProgress();
        }

        public void Pause(DownloadItem item)
        {
            GetExistingDownload(item).Pause();
        }

        public void PauseAll()
        {
            foreach (var item in downloads)
                Pause(item.Key);
        }

        public void Resume(DownloadItem item)
        {
            GetExistingDownload(item).Resume();
        }

        public void ResumeAll()
        {
            foreach (var item in downloads)
                Resume(item.Key);
        }
    }
}
