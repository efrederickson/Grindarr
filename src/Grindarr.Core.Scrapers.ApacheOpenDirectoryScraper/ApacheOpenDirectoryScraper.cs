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

        protected override async IAsyncEnumerable<IContentItem> ListDirectoryAsync(Uri dir)
        {
            var httpResponse = await httpClient.GetAsync(dir);
            var responseBodyText = httpResponse.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(await responseBodyText);

            var tables = document.DocumentNode.Descendants("table");
            foreach (var table in tables)
            {
                foreach (var tableRow in table.Descendants("tr").Skip(1)) // Skip header
                {
                    var linkNode = tableRow.ChildNodes.Descendants("a").FirstOrDefault();
                    if (linkNode == null) 
                        continue;
                    var relativeLink = linkNode.Attributes["href"].Value;
                    var completeUri = new Uri(dir, relativeLink);
                    var dateNode = linkNode.ParentNode.NextSibling;
                    var sizeNode = dateNode?.NextSibling;

                    IContentItem item = relativeLink.EndsWith("/")
                        ? (IContentItem)new FolderContentItem()
                        : ContentItemStore.GetOrCreateByDownloadUrl<ContentItem>(completeUri);

                    item.Source = dir;
                    item.Title = relativeLink;
                    item.DownloadLinks.Add(completeUri);
                    item.ReportedSizeInBytes = FileSizeUtilities.ParseFromSuffixedString(sizeNode?.InnerText);

                    if (DateTime.TryParse(dateNode?.InnerText, out DateTime dateTimeParsed))
                        item.DatePosted = dateTimeParsed;

                    yield return item;
                }
            }
        }
    }
}
