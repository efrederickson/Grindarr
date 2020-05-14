using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace Grindarr.Core
{
    /// <summary>
    /// Generic implementation of a download item
    /// </summary>
    public class DownloadItem : DownloadItemBase, IDownloadItem
    {
        public DownloadItem(IContentItem item, Uri dlUri) : base(item, dlUri) { }
        public DownloadItem() : base() { }
    }
}
