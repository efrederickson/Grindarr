using System;

namespace Grindarr.Core
{
    public class DownloadProgress
    {
        public long BytesDownloaded { get; set; }
        public long BytesTotal { get; set; }
        public double Percentage => BytesTotal == 0 ? 0 : BytesDownloaded / (double)BytesTotal;

        public DownloadSpeedTracker SpeedTracker { get; protected set; } = new DownloadSpeedTracker(10, TimeSpan.FromMilliseconds(250));

        public double UnformattedDownloadSpeed => SpeedTracker.GetBytesPerSecond();
        public string DownloadSpeed => SpeedTracker.GetBytesPerSecondString();

        public DownloadStatus Status { get; set; }
    }
}
