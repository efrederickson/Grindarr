using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grindarr.Core
{
    public static class ContentItemStore
    {
        private struct StoredContentItem
        {
            public ContentItem Item { get; set; }
            public DateTime CachedDateTime { get; set; }
            public TimeSpan Timeout { get; set; }

            public bool Expired() => DateTime.Now - CachedDateTime >= Timeout;

            public StoredContentItem(ContentItem item, DateTime cached, TimeSpan timeout)
            {
                Item = item;
                CachedDateTime = cached;
                Timeout = timeout;
            }
        }

        private static readonly TimeSpan DEFAULT_ITEM_TIMEOUT = TimeSpan.FromMinutes(15);
        //private static HashSet<Tuple<ContentItem, DateTime>> items = new HashSet<Tuple<ContentItem, DateTime>>(new ContentItemComparer());
        private static List<StoredContentItem> items = new List<StoredContentItem>();

        public static ContentItem GetBySourceUrl(Uri source) => FilterExpiredItems().Where(i => i.Source == source).FirstOrDefault();

        /// <summary>
        /// Gets or creates a <code>ContentItem</code> by source <code>Uri</code>.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ContentItem GetOrCreateBySourceUrl(Uri source) => GetOrCreateBySourceUrl(source, DEFAULT_ITEM_TIMEOUT);

        /// <summary>
        /// Gets or creates a <code>ContentItem</code> by source <code>Uri</code>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static ContentItem GetOrCreateBySourceUrl(Uri source, TimeSpan timeout)
        {
            var res = GetBySourceUrl(source);
            if (res == null)
            {
                res = new ContentItem()
                {
                    Source = source
                };
                items.Add(new StoredContentItem(item: res, cached: DateTime.Now, timeout: timeout));
            }
            return res;
        }

        public static ContentItem GetByDownloadUrl(Uri dlUri) => FilterExpiredItems().Where(i => i.DownloadLinks.Contains(dlUri)).FirstOrDefault();

        /// <summary>
        /// Gets or creates a tracked <code>ContentItem</code> by download <code>Uri</code>.
        /// </summary>
        /// <param name="dlUri">Uri to find a ContentItem by</param>
        /// <returns></returns>
        public static ContentItem GetOrCreateByDownloadUrl(Uri dlUri) => GetOrCreateByDownloadUrl(dlUri, DEFAULT_ITEM_TIMEOUT);

        /// <summary>
        /// Gets or creates a tracked <code>ContentItem</code> by download <code>Uri</code>.
        /// </summary>
        /// <param name="dlUri"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static ContentItem GetOrCreateByDownloadUrl(Uri dlUri, TimeSpan timeout)
        {
            var res = GetByDownloadUrl(dlUri);
            if (res == null)
            {
                res = new ContentItem()
                {
                    DownloadLinks = { dlUri }
                };
                items.Add(new StoredContentItem(item: res, cached: DateTime.Now, timeout: timeout));
            }
            return res;
        }

        private static IEnumerable<ContentItem> FilterExpiredItems()
        {
            // Look at this unholy abomination of a line
            // While i admire the conciseness, it's not very readable
            // Equivalent to: 
            // items = items.Where(i => !i.Expired()).ToList();
            // return items.Select(i => i.Item);
            return (items = items.Where(i => !i.Expired()).ToList()).Select(i => i.Item);
        }
    }
}
