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
            public IContentItem Item { get; set; }
            public DateTime CachedDateTime { get; set; }
            public TimeSpan Timeout { get; set; }

            public bool Expired() => DateTime.Now - CachedDateTime >= Timeout;

            public StoredContentItem(IContentItem item, DateTime cached, TimeSpan timeout)
            {
                Item = item;
                CachedDateTime = cached;
                Timeout = timeout;
            }
        }

        private static readonly TimeSpan DEFAULT_ITEM_TIMEOUT = TimeSpan.FromMinutes(15);
        //private static HashSet<Tuple<ContentItem, DateTime>> items = new HashSet<Tuple<ContentItem, DateTime>>(new ContentItemComparer());
        private static List<StoredContentItem> items = new List<StoredContentItem>();

        public static IContentItem GetBySourceUrl(Uri source) => FilterExpiredItems().Where(i => i.Source == source).FirstOrDefault();

        /// <summary>
        /// Gets or creates a <code>ContentItem</code> by source <code>Uri</code>.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T GetOrCreateBySourceUrl<T>(Uri source) where T: IContentItem => GetOrCreateBySourceUrl<T>(source, DEFAULT_ITEM_TIMEOUT);

        /// <summary>
        /// Gets or creates a <code>ContentItem</code> by source <code>Uri</code>.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static T GetOrCreateBySourceUrl<T>(Uri source, TimeSpan timeout) where T: IContentItem
        {
            T res = (T)GetBySourceUrl(source);
            if (res == null)
            {
                res = Activator.CreateInstance<T>();
                res.Source = source;
                items.Add(new StoredContentItem(item: res, cached: DateTime.Now, timeout: timeout));
            }
            return res;
        }

        public static IContentItem GetByDownloadUrl(Uri dlUri) => FilterExpiredItems().Where(i => i.DownloadLinks.Contains(dlUri)).FirstOrDefault();

        /// <summary>
        /// Gets or creates a tracked <code>ContentItem</code> by download <code>Uri</code>.
        /// </summary>
        /// <param name="dlUri">Uri to find a ContentItem by</param>
        /// <returns></returns>
        public static T GetOrCreateByDownloadUrl<T>(Uri dlUri) where T: IContentItem => GetOrCreateByDownloadUrl<T>(dlUri, DEFAULT_ITEM_TIMEOUT);

        /// <summary>
        /// Gets or creates a tracked <code>ContentItem</code> by download <code>Uri</code>.
        /// </summary>
        /// <param name="dlUri"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static T GetOrCreateByDownloadUrl<T>(Uri dlUri, TimeSpan timeout) where T: IContentItem
        {
            T res = (T)GetByDownloadUrl(dlUri);
            if (res == null)
            {
                res = Activator.CreateInstance<T>();
                res.DownloadLinks.Add(dlUri);
                items.Add(new StoredContentItem(item: res, cached: DateTime.Now, timeout: timeout));
            }
            return res;
        }

        /// <summary>
        /// Filters the list of stored items, mutating it and then returning it to remove expired items
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<IContentItem> FilterExpiredItems()
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
