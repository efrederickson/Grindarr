using HtmlAgilityPack;
using Grindarr.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Grindarr.Core.Scrapers.Implementations;

namespace Grindarr.Core.Scrapers.ApacheOpenDirectoryScraper
{
    public class ApacheOpenDirectoryScraper : BaseOpenDirectoryScraper
    {
        public ApacheOpenDirectoryScraper(Uri rootFolderUri) : base(rootFolderUri) { }
        public ApacheOpenDirectoryScraper(string rootFolderUriStr) : base(rootFolderUriStr) { }

        protected override async IAsyncEnumerable<ContentItem> ListDirectoryAsync(Uri dir)
        {
            var httpResponse = await httpClient.GetAsync(dir);
            var responseBodyText = httpResponse.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(await responseBodyText);

            bool first = true;

            var tables = document.DocumentNode.Descendants("table");
            foreach (var table in tables)
            {
                foreach (var tableRow in table.Descendants("tr"))
                {
                    // If it's the header
                    if (first)
                    {
                        first = false;
                        continue;
                    }

                    var linkNode = tableRow.ChildNodes.Descendants("a").FirstOrDefault();
                    if (linkNode == null)
                        continue;
                    var relativeLink = linkNode.Attributes["href"].Value;
                    var completeUri = dir.ToString().EndsWith("/") || relativeLink.StartsWith("/")
                        ? new Uri(dir + relativeLink)
                        : new Uri(dir + "/" + relativeLink);
                    var dateNode = linkNode.ParentNode.NextSibling;
                    var sizeNode = dateNode.NextSibling;

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
                    item.ReportedSizeInBytes = FileSizeUtilities.ParseFromSuffixedString(sizeNode.InnerText);

                    if (DateTime.TryParse(dateNode.InnerText, out DateTime dateTimeParsed))
                        item.DatePosted = dateTimeParsed;

                    yield return item;
                }
            }
        }
    }
}
