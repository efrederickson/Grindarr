using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Grindarr.Core.Scrapers.Implementations
{
    /// <summary>
    /// Provides a base for open directory scrapers such as Apache. 
    /// Simply implement the one method that parsed the directory and enjoy an easier life. 
    /// </summary>
    public abstract class BaseOpenDirectoryScraper : IScraper
    {
        protected static readonly HttpClient httpClient = new HttpClient();
        protected readonly Uri rootFolderUri;

        protected BaseOpenDirectoryScraper(Uri rootFolderUri) => this.rootFolderUri = rootFolderUri;
        protected BaseOpenDirectoryScraper(String rootFolderUriStr) : this(new Uri(rootFolderUriStr)) { }

        public IEnumerable<string> GetSerializableConstructorArguments() => new[] { rootFolderUri.ToString() };

        public async IAsyncEnumerable<IContentItem> SearchAsync(string text, int count)
        {
            await foreach (var item in RecursivelySearchDirectoriesAsync(rootFolderUri, text).Take(count))
                yield return item;
        }

        public async IAsyncEnumerable<IContentItem> GetLatestItemsAsync(int count)
        {
            //var results = ListDirectoryAsync(rootFolderUri).OrderByDescending(ci => ci.DatePosted).Take(count);
            //await foreach (var item in results)
            //    yield return item;
            await foreach (var item in GetLatestItemsFlattenedAsync(ListDirectoryAsync(rootFolderUri), count).OrderByDescending(ci => ci.DatePosted).Take(count))
                yield return item;
        }

        protected async IAsyncEnumerable<IContentItem> GetLatestItemsFlattenedAsync(IAsyncEnumerable<IContentItem> items, int count)
        {
            await foreach (var item in items.OrderByDescending(ci => ci.DatePosted).Take(count))
                if (item is FolderContentItem fci)
                    await foreach (var childItem in GetLatestItemsFlattenedAsync(ListDirectoryAsync(fci.DownloadLinks.First()), count).OrderByDescending(ci => ci.DatePosted))
                        yield return childItem;
                else
                    yield return item;
        }

        protected async IAsyncEnumerable<IContentItem> RecursivelySearchDirectoriesAsync(Uri dir, string query)
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

        /// <summary>
        /// This method does the actual loading and enumerating of the open directory (or a subfolder)
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        protected abstract IAsyncEnumerable<IContentItem> ListDirectoryAsync(Uri dir);
    }
}
