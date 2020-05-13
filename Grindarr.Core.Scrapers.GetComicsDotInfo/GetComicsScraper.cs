using Grindarr.Core.Utilities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Grindarr.Core.Scrapers.GetComicsDotInfo
{
    public class GetComicsScraper : IScraper
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string searchUrlBase = "https://getcomics.info/?s={0}";
        private const string feedUrlBase = "https://getcomics.info/feed/";

        public IEnumerable<string> GetSerializableConstructorArguments() => default;

        public async IAsyncEnumerable<IContentItem> SearchAsync(string text, int count)
        {
            await foreach (var result in DoSearchAsync(text).Take(count))
                yield return result;
        }

        public async IAsyncEnumerable<IContentItem> GetLatestItemsAsync(int count)
        {
            var feed = SyndicationFeed.Load(XmlReader.Create(feedUrlBase));
            var items = feed.Items.OrderByDescending(i => i.PublishDate).Take(count);
            foreach (var item in items)
            {
                var uri = item.Links.FirstOrDefault()?.Uri;
                if (uri == null)
                    continue;
                var ci = ContentItemStore.GetBySourceUrl(uri);
                if (ci == null)
                    ci = await ParsePageContentsAsync(uri);
                yield return ci;
            }
        }

        private async IAsyncEnumerable<IContentItem> DoSearchAsync(string query)
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

        private async Task<IContentItem> ParsePageContentsAsync(Uri url)
        {
            // The timeout here is 3 hours, why? Based on my monitoring of the website,
            // it doesn't update too often - the individual pages even less often - therefore a 3 hour timeout
            // Should be a good balance. Also reduces hammering the poor, useful website. 
            var contentItem = ContentItemStore.GetOrCreateBySourceUrl<ContentItem>(url, timeout: TimeSpan.FromHours(3));

            var httpResponse = await httpClient.GetAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(await httpResponse.Content.ReadAsStringAsync());

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
                var resolvedUri = linkUri;
                contentItem.DownloadLinks.Add(resolvedUri);
            }
            return contentItem;
        }
    }
}
