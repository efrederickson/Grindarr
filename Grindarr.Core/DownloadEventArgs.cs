using System;

namespace Grindarr.Core
{
    public class DownloadEventArgs : EventArgs
    {
        public DownloadProgress Progress => Target.Progress;
        public DownloadItem Target { get; }

        public DownloadEventArgs(DownloadItem item)
        {
            Target = item;
        }
    }
}
