namespace Grindarr.Core
{
    public enum DownloadStatus
    {
        Pending = 0,

        Paused = 1,

        Downloading = 2,

        Completed = 3,
        Failed = 4,
        Canceled = 5
    }
}
