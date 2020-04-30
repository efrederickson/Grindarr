using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Grindarr.Core.Scrapers
{
    public class ScraperManager
    {
        private const string CONFIG_SECTION = "scrapers";

        private static ScraperManager _instance = null;
        public static ScraperManager Instance => _instance ??= new ScraperManager();

        private readonly List<IScraper> scrapers = new List<IScraper>();

        private ScraperManager() 
        {
            LoadScrapers();
        }

        /// <summary>
        /// Searches all the registered scraper instances for the text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<ContentItem> SearchAsync(string text)
        {
            await foreach (var result in scrapers.Select(s => s.SearchAsync(text)).Merge())
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
            //var scraperSection = Config.Instance.GetCustomSection<List<Dictionary<string, string[]>>>(CONFIG_SECTION);
            var scraperSection = Config.Instance.GetCustomSection<Newtonsoft.Json.Linq.JArray>(CONFIG_SECTION);
            if (scraperSection == null)
                return;
            foreach (var scraper in scraperSection)
            {
                Dictionary<string, string[]> scraperCreator = ((Newtonsoft.Json.Linq.JObject)scraper).ToObject<Dictionary<string, string[]>>();
                foreach (var key in scraperCreator.Keys)
                {
                    var targetType = Type.GetType(key);
                    if (targetType == null)
                        throw new InvalidOperationException($"Type name {key} does not exist");
                    var instance = CreateScraperWithoutRegistering(targetType, scraperCreator[key]);
                    scrapers.Add(instance);
                }
            }
        }

        private void SaveScrapers()
        {
            var list = new List<Dictionary<string, string[]>>();
            foreach (var scraper in scrapers)
            {
                var expectedArgCount = scraper.GetConstructorArgumentCount();
                var args = scraper.GetSerializableConstructorArguments();
                var argCount = args == null ? 0 : args.Count();
                if (expectedArgCount != argCount)
                    throw new InvalidOperationException($"Argument counts do not match for {scraper.GetType().FullName}");
                list.Add(new Dictionary<string, string[]>()
                {
                    { scraper.GetType().AssemblyQualifiedName, args == null ? Array.Empty<string>() : args.ToArray() }
                });
            }
            Config.Instance.SetCustomSection(CONFIG_SECTION, list);
        }

        public static IEnumerable<Type> GetAllScraperClasses()
        {
            List<Type> ret = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in asm.GetTypes())
                    if (type.GetInterfaces().Contains(typeof(IScraper)))
                        ret.Add(type);
            return ret;
        }
    }
}
