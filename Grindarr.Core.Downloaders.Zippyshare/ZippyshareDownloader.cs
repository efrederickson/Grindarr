using Grindarr.Core.Downloaders.Implementations;
using Grindarr.Core.Net;
using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grindarr.Core.Downloaders.Zippyshare
{
    public class ZippyshareDownloader : GenericDownloader, IDownloader
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async override void SetItem(DownloadItem item) => base.SetItem(item, await GetActualDownloadUri(item.DownloadUri));

        private async Task<Uri> GetActualDownloadUri(Uri zippysharePage)
        {
            var httpResponse = await httpClient.GetAsync(zippysharePage);
            var responseBodyText = httpResponse.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(await responseBodyText);

            // If unable to get value, will raise an exception when it tries to create the URI from the href
            var linkNode = document.GetElementbyId("dlButton").GetAttributeValue("href", "<unable to scrape zippyshare link>");
            return new Uri(zippysharePage, linkNode);
        }
    }
}
