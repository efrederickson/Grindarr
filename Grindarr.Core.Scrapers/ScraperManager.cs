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
        public async IAsyncEnumerable<IContentItem> SearchAsync(string text, int count = 100)
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

        /// <summary>
        /// Add an instance of a scraper to the list of tracked ones
        /// </summary>
        /// <param name="scraper"></param>
        public void Register(IScraper scraper)
        {
            if (!scrapers.Contains(scraper))
                scrapers.Add(scraper);
            SaveScrapers();
        }

        /// <summary>
        /// Remove an instance of a scraper from the list of tracked ones
        /// </summary>
        /// <param name="scraper"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Provides the logic for instantiating an instance of a scraper.
        /// </summary>
        /// <param name="t">The type to create, it will be checked to ensure it implements <code>IScraper</code></param>
        /// <param name="arguments">List of arguments that will be passed to the constructor. This list will be filtered to remove empty strings</param>
        /// <returns></returns>
        private IScraper CreateScraperWithoutRegistering(Type t, IEnumerable<string> arguments)
        {
            if (t.GetInterfaces().Contains(typeof(IScraper)) == false)
                throw new InvalidOperationException($"Type {t.FullName} does not implement {typeof(IScraper).FullName}");

            var instance = (IScraper)Activator.CreateInstance(t, arguments.Where(arg => !string.IsNullOrEmpty(arg)).ToArray());
            return instance;
        }

        /// <summary>
        /// Initializes a new instance of a scraper for use. It will be persisted into the config
        /// </summary>
        /// <param name="t"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public IScraper CreateAndRegisterScraper(Type t, IEnumerable<string> arguments)
        {
            var scraper = CreateScraperWithoutRegistering(t, arguments);
            Register(scraper);
            return scraper;
        }

        /// <summary>
        /// Loads scrapers from config
        /// </summary>
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

        /// <summary>
        /// Saves configured scrapers to config
        /// </summary>
        private void SaveScrapers() 
            => Config.Instance.SetValue(CONFIG_SECTION, 
                scrapers.Select(s => ScraperModel.CreateFromScraper(s)).ToList());

        /// <summary>
        /// Searches all loaded assemblies for non-abstract classes implementing <code>IScraper</code> and returns a list of them
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllScraperClasses() 
            => AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(asm => asm.GetTypes())
            .Where(t => t.GetInterfaces().Contains(typeof(IScraper)))
            .Where(t => !t.IsAbstract);
    }
}
