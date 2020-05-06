using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Grindarr.Core.Scrapers
{
    public partial class ScraperManager
    {
        public Task<SyndicationFeed> CreateSyndicationFeedFromLatestItemsAsync(int count) => CreateSyndicationFeedAsync(GetLatestItems(count));

        public async Task<SyndicationFeed> CreateSyndicationFeedAsync(IAsyncEnumerable<ContentItem> sourceItems)
        {
            var items = new List<SyndicationItem>();
            await foreach (var contentItem in sourceItems)
            {
                var feedItem = new SyndicationItem(contentItem.Title, contentItem.Title, contentItem.Source)
                {
                    PublishDate = contentItem.DatePosted ?? DateTime.Now
                };
                contentItem.DownloadLinks.ToList().ForEach(link => feedItem.ElementExtensions.Add("downloadLink", "", link));
                items.Add(feedItem);
            }

            return new SyndicationFeed("Grindarr Feed", "Aggregates Grindarr scraper feeds", null, null, DateTime.Now)
            {
                Items = items
            };
        }
    }
}
