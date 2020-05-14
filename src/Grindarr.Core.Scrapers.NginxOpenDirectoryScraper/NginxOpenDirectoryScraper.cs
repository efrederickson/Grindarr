using Grindarr.Core.Scrapers.Implementations;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grindarr.Core.Scrapers.NginxOpenDirectoryScraper
{
    public class NginxOpenDirectoryScraper : BaseOpenDirectoryScraper
    {
        public NginxOpenDirectoryScraper(Uri rootFolderUri) : base(rootFolderUri) { }
        public NginxOpenDirectoryScraper(string rootFolderUriStr) : base(rootFolderUriStr) { }

        protected override async IAsyncEnumerable<IContentItem> ListDirectoryAsync(Uri dir)
        {
            var httpResponse = await httpClient.GetAsync(dir);
            var document = new HtmlDocument();
            document.LoadHtml(await httpResponse.Content.ReadAsStringAsync());

            var linkNodes = document.DocumentNode.Descendants("a");
            foreach (var linkNode in linkNodes.Skip(1)) // Skip header
            {
                var relativeLink = linkNode.Attributes["href"].Value;
                var completeUri = new Uri(dir, relativeLink);
                var split = linkNode.NextSibling.InnerText.Trim().Split(" ");
                var dateText = string.Join(" ", split.Take(split.Length - 1));
                var sizeText = split.Last();

                IContentItem item = relativeLink.EndsWith("/")
                    ? (IContentItem)new FolderContentItem()
                    : ContentItemStore.GetOrCreateByDownloadUrl<ContentItem>(completeUri);

                item.Source = dir;
                item.Title = relativeLink;
                item.DownloadLinks.Add(completeUri);

                if (DateTime.TryParse(dateText, out DateTime dateTimeParsed))
                    item.DatePosted = dateTimeParsed;

                if (ulong.TryParse(sizeText, out ulong sizeParsed))
                    item.ReportedSizeInBytes = sizeParsed;

                yield return item;
            }
        }
    }
}
