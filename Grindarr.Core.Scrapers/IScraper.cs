using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grindarr.Core.Scrapers
{
    public interface IScraper
    {
        /// <summary>
        /// Perform a search and return the matching results
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IAsyncEnumerable<ContentItem> SearchAsync(string text);

        /// <summary>
        /// Used to preserve constructor arguments, for example if the scraper is generic for a type of website, 
        /// and is instantiated with different websites (e.g. subreddits). The arguments must be plain objects
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSerializableConstructorArguments();
    }
}
