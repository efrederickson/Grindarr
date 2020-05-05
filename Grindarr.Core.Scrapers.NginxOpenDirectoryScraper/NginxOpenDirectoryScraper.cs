using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Grindarr.Core.Scrapers.NginxOpenDirectoryScraper
{
    public class NginxOpenDirectoryScraper : IScraper
    {

        private static readonly HttpClient httpClient = new HttpClient();
        private readonly Uri rootFolderUri;

        public NginxOpenDirectoryScraper(Uri rootFolderUri) => this.rootFolderUri = rootFolderUri;
        public NginxOpenDirectoryScraper(String rootFolderUriStr) : this(new Uri(rootFolderUriStr)) { }

        public IEnumerable<string> GetSerializableConstructorArguments() => new string[] { rootFolderUri.ToString() };

        public async IAsyncEnumerable<ContentItem> SearchAsync(string text)
        {
            await foreach (var item in RecursivelySearchDirectoriesAsync(rootFolderUri, text))
                yield return item;
        }

        private async IAsyncEnumerable<ContentItem> RecursivelySearchDirectoriesAsync(Uri dir, string query)
        {
            await foreach (var item in ListDirectoryAsync(dir))
            {
                if (item.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    if (item is FolderContentItem)
                        await foreach (var sub in RecursivelySearchDirectoriesAsync(item.DownloadLinks.First(), query))
                            yield return sub;
                    else
                        yield return item;
                }
            }
        }

        private async IAsyncEnumerable<ContentItem> ListDirectoryAsync(Uri dir)
        {
            var httpResponse = await httpClient.GetAsync(dir);
            var responseBodyText = httpResponse.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(await responseBodyText);
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
                    : new ContentItem();

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
