using System;

namespace Grindarr.Core
{
    public class DownloadProgress
    {
        /// <summary>
        /// Current amount of bytes downloaded
        /// </summary>
        public long BytesDownloaded { get; set; }

        /// <summary>
        /// Total size of this download, as reported by the actual, in progress download
        /// </summary>
        public long BytesTotal { get; set; }

        /// <summary>
        /// Dynamically calculated from the <code>BytesTotal</code> and <code>BytesDownloaded</code> values.
        /// </summary>
        public double Percentage => BytesTotal == 0 ? 0 : BytesDownloaded / (double)BytesTotal;

        public DownloadSpeedTracker SpeedTracker { get; protected set; } = new DownloadSpeedTracker(10, TimeSpan.FromMilliseconds(250));

        /// <summary>
        /// Helper property for the <code>SpeedTracker</code> bytes per second
        /// </summary>
        public double UnformattedDownloadSpeed => SpeedTracker.GetBytesPerSecond();

        /// <summary>
        /// Helper property for the <code>SpeedTracker</code> bytes per second as a formatted string
        /// </summary>
        public string DownloadSpeed => SpeedTracker.GetBytesPerSecondString();

        /// <summary>
        /// Current status of this download
        /// </summary>
        public DownloadStatus Status { get; set; }
    }
}
