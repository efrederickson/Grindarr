using System;
using System.IO;
using System.Linq;
using System.Threading;
using Grindarr.Core;
using Grindarr.Core.Downloaders;
using Grindarr.Core.Scrapers;
using Grindarr.Core.Scrapers.ApacheOpenDirectoryScraper;
using Grindarr.Core.Scrapers.GetComicsDotInfo;
using Grindarr.Core.Scrapers.NginxOpenDirectoryScraper;

namespace Grindarr.ConsoleApp
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("start");
            var sm = ScraperManager.Instance;
            //sm.Register(new ApacheOpenDirectoryScraper(new Uri("http://www.modders-heaven.net/2000GB%201/")));
            //sm.Register(new ApacheOpenDirectoryScraper(new Uri("https://dynamics.cs.washington.edu/nobackup/reddit/")));
            //sm.Register(new NginxOpenDirectoryScraper(new Uri("http://167.99.206.36/Movie/2020/")));
            if (sm.GetRegisteredScrapers().Select(s => s.GetType() == typeof(GetComicsScraper)).Count() == 0)
                sm.Register(new GetComicsScraper());
            Console.WriteLine("scraper done");
            await foreach (var obj in sm.SearchAsync("bloodseed"))
                Console.WriteLine("Search result: " + obj.Title + ": " + obj.DatePosted + ", " + obj.ReportedSizeInBytes + ", " + obj.DownloadLinks.FirstOrDefault());
            Console.WriteLine("results done");

            Console.ReadLine();
            Environment.Exit(0);

            var ci = new ContentItem
            {
                Source = new Uri("https://file-examples.com"),
                Title = "Sample MP4 Video",
                DownloadLinks = { new Uri("https://file-examples.com/wp-content/uploads/2017/04/file_example_MP4_1920_18MG.mp4") }
            };

            var di = new DownloadItem(ci, ci.DownloadLinks.First());
            di.DownloadingFilename = di.DownloadUri.Segments.Last();
            di.CompletedFilename = di.DownloadingFilename;
            
            DownloadManager.Instance.DownloadAdded += Instance_DownloadAdded;
            DownloadManager.Instance.DownloadCompleted += Instance_DownloadCompleted;
            DownloadManager.Instance.DownloadFailed += Instance_DownloadFailed;
            DownloadManager.Instance.DownloadProgressChanged += Instance_DownloadProgressChanged;

            DownloadManager.Instance.Enqueue(di);

            while (DownloadManager.Instance.DownloadQueue.Count() > 0)
                Thread.Sleep(1000);
        }

        private static void Instance_DownloadProgressChanged(object sender, DownloadEventArgs e)
        {
            Console.WriteLine("Progress Changed: " + (e.Progress.Percentage * 100) + "% at " + e.Progress.SpeedTracker.GetBytesPerSecondString() +
                 "fn: " + e.Target.DownloadingFilename + " -> " + e.Target.CompletedFilename);
        }

        private static void Instance_DownloadFailed(object sender, DownloadEventArgs e)
        {
            Console.WriteLine("Download failed: " + e.Target.DownloadingFilename);
        }

        private static void Instance_DownloadCompleted(object sender, DownloadEventArgs e)
        {
            Console.WriteLine("Download completed: " + e.Target.DownloadingFilename + " -> " + e.Target.CompletedFilename);
        }

        private static void Instance_DownloadAdded(object sender, DownloadEventArgs e)
        {
            Console.WriteLine("Download added: " + e.Target.DownloadingFilename);
        }
    }
}
