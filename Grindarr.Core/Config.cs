using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Grindarr.Core
{
    /// <summary>
    /// Provides management of the Grindarr configuration
    /// </summary>
    public class Config
    {
        /// <summary>
        /// This class actually stores the config info
        /// </summary>
        public class BareConfig
        {
            /// <summary>
            /// The full path to the downloads folder for in-progress downloads
            /// </summary>
            public string InProgressDownloadsFolder { get; set; }

            /// <summary>
            /// The full path to the downloads folder for complete downloads
            /// </summary>
            public string CompletedDownloadsFolder { get; set; }

            /// <summary>
            /// Whether or not slow/stalled downloads should be ignored when counting active downloads
            /// </summary>
            public bool? IgnoreStalledDownloads { get; set; }

            /// <summary>
            /// The speed cutoff for stalled downloads
            /// </summary>
            public double? StalledDownloadCutoff { get; set; }

            public Dictionary<string, dynamic> CustomSections { get; set; }

            /// <summary>
            /// Provides a shallow clone of this BareConfig
            /// </summary>
            /// <returns></returns>
            public BareConfig Clone()
            {
                var res = new BareConfig
                {
                    InProgressDownloadsFolder = InProgressDownloadsFolder,
                    CompletedDownloadsFolder = CompletedDownloadsFolder,
                    IgnoreStalledDownloads = IgnoreStalledDownloads,
                    StalledDownloadCutoff = StalledDownloadCutoff,
                    CustomSections = CustomSections.ToDictionary(entry => entry.Key, entry => entry.Value) // Shallow clone
                };
                return res;
            }
        }

        /// <summary>
        /// The filename for persistent configuratino
        /// </summary>
        private const string CONFIG_FILENAME = "config.json";

        #region Settings Properties

        BareConfig _config = null;

        /// <summary>
        /// Provides access into custom sections for storing configuration information
        /// </summary>
        public Dictionary<string, dynamic> CustomSections
        {
            get => _config.CustomSections ??= new Dictionary<string, dynamic>();
            private set
            {
                _config.CustomSections = value;
                Save();
            }
        }

        /// <summary>
        /// The full path to the downloads folder for in-progress downloads
        /// </summary>
        public string InProgressDownloadsFolder 
        {
            get => _config.InProgressDownloadsFolder;
            set
            {
                _config.InProgressDownloadsFolder = value;
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);
                Save();
            }
        }

        /// <summary>
        /// The full path to the downloads folder for completed downloads
        /// </summary>
        public string CompletedDownloadsFolder 
        {
            get => _config.CompletedDownloadsFolder;
            set
            {
                _config.CompletedDownloadsFolder = value;
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);
                Save();
            }
        }

        /// <summary>
        /// Whether or not slow/stalled downloads should be ignored when counting active downloads
        /// </summary>
        public bool? IgnoreStalledDownloads 
        {
            get => _config.IgnoreStalledDownloads;
            set
            {
                _config.IgnoreStalledDownloads = value;
                Save();
            }
        }

        /// <summary>
        /// The speed cutoff for stalled downloads
        /// </summary>
        public double? StalledDownloadCutoff 
        {
            get => _config.StalledDownloadCutoff; 
            set
            {
                if (value < 0)
                    value = 0;
                _config.StalledDownloadCutoff = value;
                Save();
            }
        }

        #endregion

        #region Singleton
        private static Config _instance = null;
        public static Config Instance => _instance ??= new Config();
        #endregion

        private Config()
        {
            LoadOrSetDefaults();
        }

        private void LoadOrSetDefaults()
        {
            if (File.Exists(CONFIG_FILENAME))
                Load();
            else
                SetDefaults();
        }

        private void SetDefaults()
        {
            // These ought to be reasonable defaults
            _config = new BareConfig();

            CustomSections = new Dictionary<string, dynamic>();

            InProgressDownloadsFolder = Environment.GetEnvironmentVariable("DOWNLOADING_FOLDER") ?? "in-progress";
            CompletedDownloadsFolder = Environment.GetEnvironmentVariable("COMPLETED_FOLDER") ?? "complete";

            IgnoreStalledDownloads = true;
            StalledDownloadCutoff = 50; // 50 Kb/s
        }

        private void Load()
        {
            _config = JsonConvert.DeserializeObject<BareConfig>(File.ReadAllText(CONFIG_FILENAME));
        }

        private void Save()
        {
            File.WriteAllText(CONFIG_FILENAME, JsonConvert.SerializeObject(_config));
        }

        /// <summary>
        /// Merges the set values from newConfig into this config
        /// </summary>
        /// <param name="newConfig"></param>
        public void Merge(BareConfig newConfig)
        {
            if (!string.IsNullOrEmpty(newConfig.InProgressDownloadsFolder))
                InProgressDownloadsFolder = newConfig.InProgressDownloadsFolder;
            if (!string.IsNullOrEmpty(newConfig.CompletedDownloadsFolder))
                CompletedDownloadsFolder = newConfig.CompletedDownloadsFolder;
            if (newConfig.IgnoreStalledDownloads.HasValue)
                IgnoreStalledDownloads = newConfig.IgnoreStalledDownloads;
            if (newConfig.StalledDownloadCutoff.HasValue)
                StalledDownloadCutoff = newConfig.StalledDownloadCutoff;

            if (newConfig.CustomSections != null && newConfig.CustomSections.Count > 0)
                Console.WriteLine("Ignoring merge for custom option section with values set");
        }

        public BareConfig GetBareConfig() => _config;

        /// <summary>
        /// Returns the custom section with the specified name, as the specified type.
        /// </summary>
        /// <typeparam name="T">Type the custom section is cast to</typeparam>
        /// <param name="key">The name of the custom section</param>
        /// <returns>The custom section</returns>
        public T GetCustomSection<T>(string key)
        {
            return GetCustomSection<T>(key, default);
        }

        /// <summary>
        /// Returns the custom section with the specified name, as the specified type.
        /// </summary>
        /// <typeparam name="T">Type the custom section is cast to</typeparam>
        /// <param name="key">The name of the custom section</param>
        /// <param name="defaultObj">The default object created if the custom section does not exist, default to <code>default(T)</code></param>
        /// <returns>The custom section</returns>
        public T GetCustomSection<T>(string key, T defaultObj)
        {
            if (!CustomSections.ContainsKey(key))
                return defaultObj;

            var plainObj = CustomSections[key];
            
            if (plainObj is JObject)
                return ((JObject)plainObj).ToObject<T>();
            return (T)plainObj;
        }

        /// <summary>
        /// Sets the custom section to this value. 
        /// Because it is dynamic, the value is not constrained to managed objects - it can be an int, bool, etc.
        /// </summary>
        /// <param name="key">The custom section key</param>
        /// <param name="value">The value</param>
        public void SetCustomSection(string key, dynamic value)
        {
            CustomSections[key] = value;
            Save();
        }
    }
}
