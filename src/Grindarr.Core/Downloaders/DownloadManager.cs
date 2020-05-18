using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grindarr.Core.Logging;
using Newtonsoft.Json;

namespace Grindarr.Core.Downloaders
{
    public class DownloadManager : IDownloadManager
    {
        private const string DLSTATE_PATH = "downloads.json";

        private static DownloadManager _instance = null;
        public static DownloadManager Instance => _instance ??= new DownloadManager();

        public int MaxSimultaneousDownloads { get; set; } = 5;
        public bool? IgnoreStalledDownloads => Config.Instance.GetIgnoreStalledDownloads();
        public double? StalledDownloadCutoff => Config.Instance.GetStalledDownloadsCutoff();
        public IEnumerable<IDownloadItem> DownloadQueue => downloads.Keys;

        private readonly Dictionary<IDownloadItem, IDownloader> downloads = new Dictionary<IDownloadItem, IDownloader>();

        public event EventHandler<DownloadEventArgs> DownloadCompleted;
        public event EventHandler<DownloadEventArgs> DownloadFailed;
        public event EventHandler<DownloadEventArgs> DownloadAdded;
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;

        public DownloadManager() => LoadDownloads();

        /// <summary>
        /// This method will start or stop downloads, depending on the number of currently active downloads compared to the desired number
        /// </summary>
        private void UpdateActiveDownloads()
        {
            if (GetActiveDownloads().Count() < MaxSimultaneousDownloads)
            {
                var target = DownloadQueue.Where((di) => di.Progress?.Status == DownloadStatus.Pending).FirstOrDefault();
                if (target != null)
                    GetExistingDownload(target).Start();
            }
            else if (GetActiveDownloads().Count() > MaxSimultaneousDownloads)
            {
                var target = DownloadQueue.Where((di) => di.Progress?.Status == DownloadStatus.Downloading).Reverse().FirstOrDefault();
                if (target != null)
                    Pause(target);
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
            SaveDownloads();
        }

        private void IDownloader_DownloadFailed(object sender, DownloadEventArgs e)
        {
            UpdateActiveDownloads();
            Log.WriteLine($"Download failed: {e.Target.DownloadUri}");
            if (e.Target.Progress.Status == DownloadStatus.Canceled)
                downloads.Remove(e.Target);
            if (File.Exists(e.Target.GetDownloadingPath()))
               File.Delete(e.Target.GetDownloadingPath());
            DownloadFailed?.Invoke(sender, e);
            SaveDownloads();
        }

        /// <summary>
        /// Private method that contains the actual logic for adding a download
        /// </summary>
        /// <param name="item"></param>
        private void InternalAddDownload(IDownloadItem item)
        {
            // Check dirs
            if (!Directory.Exists(Config.Instance.GetInProgressDownloadsFolder()))
                Directory.CreateDirectory(Config.Instance.GetInProgressDownloadsFolder());
            if (!Directory.Exists(Config.Instance.GetCompleteDownloadsFolder()))
                Directory.CreateDirectory(Config.Instance.GetCompleteDownloadsFolder());

            // Create and store downloader
            IDownloader dl = DownloaderFactory.CreateFrom(item.DownloadUri);
            dl.SetItem(item);

            // Register events
            dl.DownloadProgressChanged += IDownloader_DownloadProgressChanged;
            dl.DownloadComplete += IDownloader_DownloadCompleted;
            dl.DownloadFailed += IDownloader_DownloadFailed;

            downloads[dl.CurrentDownloadItem] = dl;
            // Fire event
            DownloadAdded?.Invoke(this, new DownloadEventArgs(dl.CurrentDownloadItem));

            UpdateActiveDownloads();
            SaveDownloads();
        }

        /// <summary>
        /// Returns the downloads that are currently downloading, and if desired, above the download cutoff
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IDownloadItem> GetActiveDownloads()
        {
            var res = downloads.Values
                .Where((val) => val.CurrentDownloadItem?.Progress?.Status == DownloadStatus.Downloading);

            // Filter stalled downloads if requested
            if (IgnoreStalledDownloads.HasValue && IgnoreStalledDownloads.Value)
                res = res.Where((p) => p.CurrentDownloadItem.Progress.SpeedTracker.GetBytesPerSecond() > StalledDownloadCutoff);

            return res.Select((s) => s.CurrentDownloadItem).ToList(); // Resolve promises now, prevent problems later. I hope.
        }

        public void Cancel(IDownloadItem item) => downloads[item].Cancel();

        public void CancelAll() => DownloadQueue.ToList().ForEach(item => Cancel(item));

        /// <summary>
        /// Returns a download with the specified guid, assuming it exists, otherwise null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IDownloadItem GetById(Guid id) => downloads.Keys.Where(dl => dl.Id == id).FirstOrDefault();

        /// <summary>
        /// Returns the downloader for a specified download item if it exists, otherwise it throws an exception
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private IDownloader GetExistingDownload(IDownloadItem item) => DownloadQueue.Contains(item) ? downloads[item] : throw new KeyNotFoundException();

        public void Enqueue(IDownloadItem item) => InternalAddDownload(item);

        public DownloadProgress GetProgress(IDownloadItem item) => GetExistingDownload(item).CurrentDownloadItem.Progress;

        public void Pause(IDownloadItem item) => GetExistingDownload(item).Pause();

        public void PauseAll()
            => DownloadQueue.Where(item => item.Progress?.Status == DownloadStatus.Pending || item.Progress?.Status == DownloadStatus.Downloading)
            .ToList().ForEach(item => Pause(item));

        public void Resume(IDownloadItem item) => GetExistingDownload(item).Resume();

        public void ResumeAll()
            => DownloadQueue.Where(item => item.Progress?.Status == DownloadStatus.Paused).ToList().ForEach(item => Resume(item));

        private void LoadDownloads()
        {
            if (File.Exists(DLSTATE_PATH))
            {
                var loadedDownloads = JsonConvert.DeserializeObject<IEnumerable<IDownloadItem>>(File.ReadAllText(DLSTATE_PATH), new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                });
                foreach (var dl in loadedDownloads)
                    Enqueue(dl);
            }
        }

        private void SaveDownloads()
        {
            File.WriteAllText(DLSTATE_PATH, JsonConvert.SerializeObject(downloads.Keys.ToArray(), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            }));
        }
    }
}
