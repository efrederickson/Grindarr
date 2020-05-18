using Grindarr.Core;
using Grindarr.Core.Downloaders;
using Soulseek;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Grindarr.Soulseek
{
    /// <summary>
    /// Implementation of a downloader to download a given soulseek file
    /// </summary>
    public class SoulSeekDownloader : IDownloader
    {
        public IDownloadItem CurrentDownloadItem => CurrentSSDownloadItem;

        public SoulseekDownloadItem CurrentSSDownloadItem { get; private set; }

        public event EventHandler<DownloadEventArgs> DownloadComplete;
        public event EventHandler<DownloadEventArgs> DownloadFailed;
        public event EventHandler<DownloadEventArgs> DownloadProgressChanged;

        private CancellationTokenSource cancellationToken = null;

        public void Cancel()
        {
            if (cancellationToken != null)
                cancellationToken.Cancel();
            CurrentDownloadItem.Progress.Status = DownloadStatus.Canceled;
            DownloadFailed?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
        }

        public void Pause()
        {
            if (cancellationToken != null)
            {
                if (CurrentDownloadItem.Progress.Status != DownloadStatus.Downloading && CurrentDownloadItem.Progress.Status != DownloadStatus.Pending)
                    throw new InvalidOperationException();

                cancellationToken.Cancel();

                CurrentDownloadItem.Progress.Status = DownloadStatus.Paused;
                DownloadProgressChanged?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
            }
        }

        public void Resume()
        {
            if (CurrentDownloadItem.Progress.Status != DownloadStatus.Paused)
                throw new InvalidOperationException();

            Start();
        }

        public void Start()
        {
            if (cancellationToken == null)
            {
                StartWorkerAsync();
                CurrentDownloadItem.Progress.Status = DownloadStatus.Downloading;
                DownloadProgressChanged?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
            }
        }

        public void SetItem(IDownloadItem item)
        {
            CurrentSSDownloadItem = SoulseekDownloadItem.ParseFrom(item);
            item.DownloadingFilename = HttpUtility.UrlDecode(item.DownloadUri.Segments.Last());
            item.CompletedFilename = item.DownloadingFilename;

            item.Progress = new DownloadProgress
            {
                BytesTotal = 0,
                BytesDownloaded = 0,
                Status = DownloadStatus.Pending
            };

            // Now it's ready, invoke the handler to get download manager to start (or update)
            DownloadProgressChanged?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
        }

        private async void StartWorkerAsync()
        {
            if (cancellationToken != null)
                throw new InvalidOperationException("Cannot start download when cancellation token already exists");

            cancellationToken = new CancellationTokenSource();
            CurrentDownloadItem.Progress.Status = DownloadStatus.Downloading;
            byte[] bytes = null;
            try
            {
                var task = SoulseekWrapper.Instance.GetClient()?.DownloadAsync(
                    username: CurrentSSDownloadItem.SoulseekUsername,
                    filename: CurrentSSDownloadItem.SoulseekFilename,
                    startOffset: 0,
                    cancellationToken: cancellationToken.Token,
                    options: new TransferOptions(
                        stateChanged: (e) =>
                        {
                            switch (e.Transfer.State)
                            {
                                case TransferStates.TimedOut:
                                case TransferStates.Rejected:
                                case TransferStates.Errored:
                                case TransferStates.Cancelled:
                                    CurrentDownloadItem.Progress.Status = DownloadStatus.Failed;
                                    DownloadFailed?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
                                    break;
                                case TransferStates.Completed:
                                    Console.WriteLine($"Finished {CurrentDownloadItem.DownloadUri}");
                                    break;
                                default:
                                    break;
                            }
                        },
                        progressUpdated: (e) =>
                        {
                            CurrentDownloadItem.Progress.BytesDownloaded = e.Transfer.BytesTransferred;
                            CurrentDownloadItem.Progress.BytesTotal = e.Transfer.Size;
                            CurrentDownloadItem.Progress.SpeedTracker.SetProgress(e.Transfer.BytesTransferred);
                            DownloadProgressChanged?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
                        }
                    )
                );
                if (task == null)
                {
                    // TODO... 
                    Console.WriteLine("Did not create task - assuming Soulseek authentication has not been set");
                    throw new InvalidOperationException("Unable to create download task");
                }
                bytes = await task;
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during soulseek download for {CurrentSSDownloadItem.SoulseekFilename}: {ex.Message}");
                CurrentDownloadItem.Progress.Status = DownloadStatus.Failed;
                DownloadFailed?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
                cancellationToken = null;
                return;
            }

            if (bytes != null)
            {
                System.IO.File.WriteAllBytes(CurrentSSDownloadItem.GetDownloadingPath(), bytes);
                CurrentDownloadItem.Progress.Status = DownloadStatus.Completed;
                DownloadComplete?.Invoke(this, new DownloadEventArgs(CurrentDownloadItem));
                cancellationToken = null;
            }
            else
            {

            }
        }
    }
}
