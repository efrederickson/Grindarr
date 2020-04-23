using System;
using System.Collections.Generic;
using System.Linq;

namespace Grindarr.Core.Collections
{
    public abstract class DomainMapFactory<T, TDefault> where TDefault : T
    {
        protected static readonly Dictionary<string, Type> typeMap = new Dictionary<string, Type>();

        public static T CreateFrom(Uri url)
        {
            string domain = url.DnsSafeHost;

            if (typeMap.ContainsKey(domain)) // TODO: better way to do this
                return (T)Activator.CreateInstance(typeMap[domain]);

            return (T)Activator.CreateInstance(typeof(TDefault));
        }

        public static Uri GetBestMatch(IEnumerable<Uri> urls)
        {
            var matches = urls.Where(url => typeMap.Keys.Contains(url.DnsSafeHost));
            return matches.FirstOrDefault() ?? urls.FirstOrDefault();
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
