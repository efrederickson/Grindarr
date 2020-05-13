using Grindarr.Core.Collections;
using Grindarr.Core.Downloaders.Implementations;

namespace Grindarr.Core.Downloaders
{
    public class DownloaderFactory : UrlMapFactory<IDownloader, GenericDownloader>
    {
        private DownloaderFactory() { }
    }
}
