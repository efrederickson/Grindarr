using System;

namespace Grindarr.Core
{
    /// <summary>
    /// Event args used to pass information about an <code>IDownloadItem</code> that has changed in some way
    /// </summary>
    public class DownloadEventArgs : EventArgs
    {
        /// <summary>
        /// The download item in question for this object
        /// </summary>
        public IDownloadItem Target { get; }

        public DownloadEventArgs(IDownloadItem item) => Target = item;
    }
}
