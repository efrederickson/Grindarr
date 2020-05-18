using System;
using System.Collections.Generic;
using System.Linq;

namespace Grindarr.Core.Collections
{
    /// <summary>
    /// This is a class to provide domain to T mappings, for example domain to IDownloader
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TDefault"></typeparam>
    public abstract class UrlMapFactory<T, TDefault> where TDefault : T
    {
        protected static readonly Dictionary<string, Type> typeMap = new Dictionary<string, Type>();

        public static T CreateFrom(Uri url)
        {
            string domain = url.DnsSafeHost;

            var filtered = GetFilteredDomains(domain);
            if (filtered.Count() > 0)
                return (T)Activator.CreateInstance(filtered.First());

            return (T)Activator.CreateInstance(typeof(TDefault));
        }

        public static Uri GetBestMatch(IEnumerable<Uri> urls)
        {
            var matches = urls.Where(url => GetFilteredDomains(url.DnsSafeHost).Count() > 0);
            return matches.FirstOrDefault() ?? urls.FirstOrDefault();
        }

        private static IEnumerable<Type> GetFilteredDomains(string domain)
        {
            foreach (var match in typeMap.Keys.Where(key => domain.EndsWith(key, StringComparison.OrdinalIgnoreCase)))
                yield return typeMap[match];
        }

        public static void Register(string domain, T type)
        {
            typeMap[domain] = type.GetType();
        }

        public static void Unregister(string domain)
        {
            if (typeMap.ContainsKey(domain))
                typeMap.Remove(domain);
        }
    }
}
