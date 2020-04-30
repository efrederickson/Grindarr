using System;

namespace Grindarr.Core
{
    public class DownloadEventArgs : EventArgs
    {
        /// <summary>
        /// Returns the download progress of the <code>Target</code> of this object
        /// </summary>
        public DownloadProgress Progress => Target.Progress;

        /// <summary>
        /// The download item in question for this object
        /// </summary>
        public DownloadItem Target { get; }

        public DownloadEventArgs(DownloadItem item) => Target = item;
    }
}
