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

        public int GetConstructorArgumentCount() => 1;

        public IEnumerable<string> GetSerializableConstructorArguments() => new string[] { rootFolderUri.ToString() };

        public IEnumerable<ContentItem> Search(string text)
        {
            var dirContentsTask = Task.Run(async () => await listDirectoryAsync(rootFolderUri));
            dirContentsTask.Wait();
            var dirContents = dirContentsTask.Result;

            var filtered = dirContents.Where(ci => ci.Title.Contains(text, StringComparison.OrdinalIgnoreCase));

            while (filtered.Any(ci => ci is FolderContentItem))
            {
                var folders = filtered.Where(ci => ci is FolderContentItem);
                var others = filtered.Where(ci => !(ci is FolderContentItem));
                List<ContentItem> filteredTemp = new List<ContentItem>(others);
                foreach (var folder in folders)
                {
                    var task = Task.Run(async () => await listDirectoryAsync(folder.DownloadLinks.FirstOrDefault()));
                    task.Wait();
                    filteredTemp.AddRange(task.Result);
                }
                filtered = filteredTemp.Where(ci => ci.Title.Contains(text, StringComparison.OrdinalIgnoreCase));
            }

            return filtered;
        }

        private async Task<IEnumerable<ContentItem>> listDirectoryAsync(Uri dir)
        {
            List<ContentItem> res = new List<ContentItem>();

            var httpResponse = await httpClient.GetAsync(dir);
            var responseBodyText = httpResponse.Content.ReadAsStringAsync();
            var document = new HtmlDocument();
            document.LoadHtml(await responseBodyText);

            var linkNodes = document.DocumentNode.Descendants("a");
            foreach (var linkNode in linkNodes)
            {
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

                if (long.TryParse(sizeText, out long sizeParsed))
                    item.ReportedSizeInBytes = sizeParsed;

                res.Add(item);
            }

            return res;
        }
    }
}
