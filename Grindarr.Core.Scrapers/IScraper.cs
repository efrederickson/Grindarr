using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grindarr.Core.Scrapers
{
    public interface IScraper
    {
        /// <summary>
        /// Used to preserve constructor arguments, for example if the scraper is generic for a type of website, 
        /// and is instantiated with different websites (e.g. subreddits). The arguments must be plain objects
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSerializableConstructorArguments();

        /// <summary>
        /// Perform a search and return the matching results
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IAsyncEnumerable<ContentItem> SearchAsync(string text);

        /// <summary>
        /// Return <code>count</code> latest items from the source. 
        /// </summary>
        /// <param name="count">Return up to this many results</param>
        /// <returns></returns>
        public IAsyncEnumerable<ContentItem> GetLatestItemsAsync(int count);
    }
}
