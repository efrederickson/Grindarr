using Grindarr.Core.Collections;
using Grindarr.Core.Downloaders.Implementations;

namespace Grindarr.Core.Downloaders
{
    public class DownloaderFactory : DomainMapFactory<IDownloader, GenericDownloader>
    {
        private DownloaderFactory() { }
    }
}
