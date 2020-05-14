namespace Grindarr.Core
{
    /// <summary>
    /// Provides the different states a download can be in
    /// </summary>
    public enum DownloadStatus
    {
        /// <summary>
        /// The download is queued, but has not yet begun
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The download has been paused
        /// </summary>
        Paused = 1,

        /// <summary>
        /// The download is currently downloading, 
        /// whether that means actively receiving data or stalled.
        /// </summary>
        Downloading = 2,

        /// <summary>
        /// The download has successfully completed
        /// </summary>
        Completed = 3,

        /// <summary>
        /// The download has failed
        /// </summary>
        Failed = 4,

        /// <summary>
        /// The download was canceled (generally by the user or an API consumer)
        /// </summary>
        Canceled = 5
    }
}
