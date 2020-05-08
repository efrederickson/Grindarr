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

        protected override async IAsyncEnumerable<ContentItem> ListDirectoryAsync(Uri dir)
        {
            var httpResponse = await httpClient.GetAsync(dir);
            var document = new HtmlDocument();
            document.LoadHtml(await httpResponse.Content.ReadAsStringAsync());
            bool first = true;

            var linkNodes = document.DocumentNode.Descendants("a");
            foreach (var linkNode in linkNodes)
            {
                // Skip header
                if (first)
                {
                    first = false;
                    continue;
                }
                var relativeLink = linkNode.Attributes["href"].Value;
                var completeUri = dir.ToString().EndsWith("/") || relativeLink.StartsWith("/")
                    ? new Uri(dir + relativeLink)
                    : new Uri(dir + "/" + relativeLink);
                var dateAndSizeText = linkNode.NextSibling.InnerText.Trim();
                var split = dateAndSizeText.Split(" ");
                var dateText = string.Join(" ", split.Take(split.Length - 1));
                var sizeText = split.Last();

                var title = linkNode.InnerText.EndsWith(">") || linkNode.InnerText.EndsWith("&gt;") // Not url-decoded
                    ? relativeLink
                    : linkNode.InnerText;

                ContentItem item =
                    relativeLink.EndsWith("/")
                    ? new FolderContentItem()
                    : ContentItemStore.GetOrCreateByDownloadUrl(completeUri);

                item.Source = dir;
                item.Title = title;
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
