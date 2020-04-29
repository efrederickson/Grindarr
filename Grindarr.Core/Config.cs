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
    public class Config
    {
        /// <summary>
        /// This class actually stores the config info
        /// </summary>
        public class BareConfig
        {
            public string InProgressDownloadsFolder { get; set; }
            public string CompletedDownloadsFolder { get; set; }
            public bool? IgnoreStalledDownloads { get; set; }
            public double? StalledDownloadCutoff { get; set; }

            public Dictionary<string, dynamic> CustomSections { get; set; }

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

        private const string CONFIG_FILENAME = "config.json";

        #region Settings Properties

        BareConfig _config = null;

        public Dictionary<string, dynamic> CustomSections
        {
            get => _config.CustomSections ??= new Dictionary<string, dynamic>();
            private set
            {
                _config.CustomSections = value;
                Save();
            }
        }

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

        public bool? IgnoreStalledDownloads 
        {
            get => _config.IgnoreStalledDownloads;
            set
            {
                _config.IgnoreStalledDownloads = value;
                Save();
            }
        }

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

        public T GetCustomSection<T>(string key)
        {
            return GetCustomSection<T>(key, default);
        }
        public T GetCustomSection<T>(string key, T defaultObj)
        {
            if (!CustomSections.ContainsKey(key))
                return defaultObj;

            var plainObj = CustomSections[key];
            
            if (plainObj is JObject)
                return ((JObject)plainObj).ToObject<T>();
            return (T)plainObj;
        }

        public void SetCustomSection(string key, dynamic value)
        {
            CustomSections[key] = value;
            Save();
        }
    }
}
