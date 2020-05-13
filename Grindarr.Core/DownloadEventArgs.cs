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
        public IDownloadItem Target { get; }

        public DownloadEventArgs(IDownloadItem item) => Target = item;
    }
}
