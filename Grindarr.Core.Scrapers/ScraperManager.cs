using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Grindarr.Core.Scrapers
{
    public partial class ScraperManager
    {
        private const string CONFIG_SECTION = "grindarr.core.scrapers.configuredScrapers";

        private static ScraperManager _instance = null;
        public static ScraperManager Instance => _instance ??= new ScraperManager();

        private readonly List<IScraper> scrapers = new List<IScraper>();

        private ScraperManager() => LoadScrapers();

        /// <summary>
        /// Searches all the registered scraper instances for the text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<IContentItem> SearchAsync(string text, int count)
        {
            await foreach (var result in scrapers.Select(s => s.SearchAsync(text, count)).Merge().Take(count))
                yield return result;
        }

        public async IAsyncEnumerable<IContentItem> GetLatestItems(int count)
        {
            var results = scrapers.Select(s => s.GetLatestItemsAsync(count)).Merge().OrderByDescending(ci => ci.DatePosted).Take(count);
            await foreach (var result in results)
                yield return result;
        }

        public void Register(IScraper scraper)
        {
            if (!scrapers.Contains(scraper))
                scrapers.Add(scraper);
            SaveScrapers();
        }

        public bool Unregister(IScraper scraper)
        {
            bool res = false;
            if (scrapers.Contains(scraper))
                if (scrapers.Remove(scraper))
                    res = true;
            SaveScrapers();
            return res;
        }

        public IEnumerable<IScraper> GetRegisteredScrapers() => scrapers;

        private IScraper CreateScraperWithoutRegistering(Type t, IEnumerable<string> arguments)
        {
            if (t.GetInterfaces().Contains(typeof(IScraper)) == false)
                throw new InvalidOperationException($"Type {t.FullName} does not implement {typeof(IScraper).FullName}");

            var instance = (IScraper)Activator.CreateInstance(t, arguments.Where(arg => !string.IsNullOrEmpty(arg)).ToArray());
            return instance;
        }

        public IScraper CreateAndRegisterScraper(Type t, IEnumerable<string> arguments)
        {
            var scraper = CreateScraperWithoutRegistering(t, arguments);
            Register(scraper);
            return scraper;
        }

        private void LoadScrapers()
        {
            foreach (var scraper in Config.Instance.GetEnumerableValue<ScraperModel>(CONFIG_SECTION))
            {
                var targetType = Type.GetType(scraper.ClassName);
                if (targetType == null)
                    throw new InvalidOperationException($"Type name {scraper.ClassName} does not exist");
                var instance = CreateScraperWithoutRegistering(targetType, scraper.Arguments);
                scrapers.Add(instance);
            }
        }

        private void SaveScrapers() 
            => Config.Instance.SetValue(CONFIG_SECTION, 
                scrapers.Select(s => ScraperModel.CreateFromScraper(s)).ToList());

        public static IEnumerable<Type> GetAllScraperClasses() 
            => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(t => t.GetInterfaces().Contains(typeof(IScraper)))
            .Where(t => !t.IsAbstract);
    }
}
