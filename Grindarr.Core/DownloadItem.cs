using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace Grindarr.Core
{
    public class DownloadItem : DownloadItemBase, IDownloadItem
    {
        public DownloadItem(IContentItem item, Uri dlUri) : base(item, dlUri) { }
        public DownloadItem() : base() { }
    }
}
