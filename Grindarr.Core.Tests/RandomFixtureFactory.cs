using System;

namespace Grindarr.Core.Tests
{
    internal class RandomFixtureFactory
    {
        private static Random Random { get; } = new Random();

        public static ContentItem CreateContentItem()
        {
            return new ContentItem()
            {
                DatePosted = DateTime.Now,
                DownloadLinks = { new Uri("https://some.site/download.iso") },
                ReportedSizeInBytes = (ulong)Random.Next(),
                Source = new Uri("https://site.site/downloads.html"),
                Title = $"Some Download {Random.Next()}",
            };
        }

        public static DownloadItem CreateDownloadItem()
        {
            var res = new DownloadItem(CreateContentItem(), new Uri($"https://site-other.com/alt-dl-{Random.Next()}.iso"));

            res.Progress = new DownloadProgress
            {
                BytesDownloaded = 0,
                BytesTotal = 0,
                Status = DownloadStatus.Pending
            };
            res.Progress.BytesTotal = Random.Next(1, int.MaxValue - 1);
            res.Progress.BytesDownloaded = Random.Next(1, (int)res.Progress.BytesTotal);
            res.Progress.Status = (DownloadStatus)Random.Next(0, (int)DownloadStatus.Canceled);
            res.Progress.SpeedTracker.SetProgress(0);
            res.Progress.SpeedTracker.SetProgress(Random.Next(1, (int)res.Progress.BytesTotal));

            return res;
        }
    }
}
