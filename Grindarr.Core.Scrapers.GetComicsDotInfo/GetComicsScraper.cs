using Grindarr.Core.Utilities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Grindarr.Core.Scrapers.GetComicsDotInfo
{
    public class GetComicsScraper : IScraper
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string searchUrlBase = "https://getcomics.info/?s={0}";
        public uint GetConstructorArgumentCount() => 0;

        public IEnumerable<string> GetSerializableConstructorArguments() => default;

        public async IAsyncEnumerable<ContentItem> SearchAsync(string text)
        {
            await foreach (var result in DoSearchAsync(text))
                yield return result;
        }

        private async IAsyncEnumerable<ContentItem> DoSearchAsync(string query)
        {
            var httpResponse = await httpClient.GetAsync(new Uri(string.Format(searchUrlBase, query)));
            var responseBodyText = httpResponse.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(await responseBodyText);

            var matches = document.DocumentNode.Descendants("article");
            foreach (var match in matches)
            {
                var link = match.Descendants("a").FirstOrDefault().GetAttributeValue("href", "<could not scrape link>");
                var linkUri = new Uri(link);
                var item = ParsePageContentsAsync(linkUri);
                yield return await item;
            }
        }

        private async Task<ContentItem> ParsePageContentsAsync(Uri url)
        {
            var contentItem = new ContentItem();

            var httpResponse = await httpClient.GetAsync(url);
            var responseBodyText = httpResponse.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(await responseBodyText);

            contentItem.Title = HttpUtility.HtmlDecode(document.DocumentNode.Descendants().Where(node => node.HasClass("post-title")).FirstOrDefault().InnerText);
            contentItem.DatePosted = DateTime.Parse(document.DocumentNode.Descendants()
                .Where(node => node.HasClass("post-date")).FirstOrDefault()
                .Descendants("time").FirstOrDefault()
                .GetAttributeValue("datetime", ""));
            contentItem.Source = url;
            contentItem.ReportedSizeInBytes = FileSizeUtilities.ParseFromSuffixedString(document.DocumentNode.Descendants()
                .Where(node => node.InnerText.StartsWith("Size", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault().NextSibling.InnerText);

            // Filter down to relevant links based on what I deduced from looking at the page source
            var linkNodes = document.DocumentNode.Descendants("section")
                .Where(child => child.HasClass("post-contents"))
                .SelectMany(node => node.Descendants("div"))
                .Where(node => node.HasClass("aio-pulse"))
                .SelectMany(node => node.Descendants("a"));
            foreach (var linkNode in linkNodes)
            {
                var link = linkNode.GetAttributeValue("href", "<could not scrape link>");
                var linkUri = new Uri(link);
                var resolvedUri = await UrlRedirectionResolverUtility.ResolveAsync(linkUri);
                contentItem.DownloadLinks.Add(resolvedUri);
            }
            return contentItem;
        }
    }
}
