using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Grindarr.Core
{
    /// <summary>
    /// Provides management of the Grindarr configuration
    /// </summary>
    public class Config
    {
        /// <summary>
        /// The filename for persistent configuratino
        /// </summary>
        private const string CONFIG_FILENAME = "config.json";

        private static Config _instance = null;

        Dictionary<string, dynamic> _config = null;

        public static Config Instance => _instance ??= new Config();

        private Config()
        {
            Load();
        }

        private void Load()
        {
            if (File.Exists(CONFIG_FILENAME))
                _config = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(File.ReadAllText(CONFIG_FILENAME));
            else
                _config = new Dictionary<string, dynamic>();
        }

        private void Save() => File.WriteAllText(CONFIG_FILENAME, JsonConvert.SerializeObject(_config));
        
        public Dictionary<string, dynamic> GetRawDictionary() => _config;

        public void Merge(IDictionary<string, dynamic> newConfig)
        {
            foreach (var key in newConfig.Keys)
                _config[key] = newConfig[key];
            Save();
        }

        public T GetValue<T>(string key, T defaultObj)
        {
            if (_config.ContainsKey(key))
                return (T)_config[key];

            // If it doesn't exist, seed it to the defaultObj
            SetValue(key, defaultObj);
            return defaultObj;
        }

        public IEnumerable<T> GetEnumerableValue<T>(string key)
        {
            if (!_config.ContainsKey(key))
                return Array.Empty<T>(); // Return an empty array here because a null would cause foreach's to fail
            var res = (JArray)_config[key];
            return res.ToObject<IEnumerable<T>>();
        }

        /// <summary>
        /// Sets the value for a given key to the requested value
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="key">full key path for the settings</param>
        /// <param name="value">New value, also returned to help chain methods</param>
        /// <returns>value</returns>
        public T SetValue<T>(string key, T value)
        {
            _config[key] = value;
            Save();

            return value;
        }
    }
}
