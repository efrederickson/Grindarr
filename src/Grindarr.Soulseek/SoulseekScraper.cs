using Grindarr.Core;
using Grindarr.Core.Downloaders;
using Grindarr.Core.Scrapers;
using Soulseek;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Grindarr.Soulseek
{
    public class SoulseekScraper : IScraper
    {
        /// <summary>
        /// Soulseek does not support this type of content finding, due to the nature of how it works
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<IContentItem> GetLatestItemsAsync(int count)
        {
            yield break;
        }

        /// <summary>
        /// This will return the username and password if configured, or an empty enumerable
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSerializableConstructorArguments()
        {
            var username = SoulseekWrapper.Instance.GetUsername();
            var password = SoulseekWrapper.Instance.GetPassword();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return default;
            return new[] { username, password };
        }

        public SoulseekScraper()
        {

        }

        /// <summary>
        /// Initialize the soulseek client with the given username and password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public SoulseekScraper(string username, string password)
        {
            SoulseekWrapper.Instance.SetUsername(username);
            SoulseekWrapper.Instance.SetPassword(password);
        }

        public async IAsyncEnumerable<IContentItem> SearchAsync(string text, int count)
        {
            var query = new SearchQuery(text);
            IEnumerable<SearchResponse> results = await SoulseekWrapper.Instance.GetClient().SearchAsync(query, options: new SearchOptions(fileLimit: count));
            results = results
                .OrderByDescending(res => res.FreeUploadSlots)
                .ThenBy(res => res.QueueLength)
                .ThenByDescending(res => res.UploadSpeed);
            foreach (var result in results)
            {
                foreach (var resultItem in result.Files.Where(i => i.Size > 0))
                {
                    var fakeUri = FakeSoulseekUriBuilder.BuildFrom(result.Username, resultItem);
                    if (fakeUri == null)
                        continue;

                    var ssItem = ContentItemStore.GetOrCreateByDownloadUrl<SoulseekContentItem>(fakeUri);
                    ssItem.SoulseekUsername = result.Username;
                    ssItem.ReportedSizeInBytes = (ulong)resultItem.Size;
                    ssItem.SoulseekFilename = resultItem.Filename;
                    ssItem.Title = resultItem.Filename;
                    ssItem.Source = fakeUri;
                    ssItem.DownloadLinks.Add(fakeUri);

                    yield return ssItem;
                }
            }
        }
    }
}
