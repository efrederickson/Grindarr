using Grindarr.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grindarr.Web.Api.Authorization
{
    public static class ApiKeyWrapper
    {
        private const string CONFIG_SECTION = "webBackendApiKey";
        private const string CONFIG_APIKEY_FIELD = "apiKey";
        private const string CONFIG_ENFORCE_APIKEY_FIELD = "enforceApiKey";

        private static Dictionary<string, dynamic> GetConfigurationSettings()
        {
            var section = Config.Instance.GetCustomSection<Dictionary<string, dynamic>>(CONFIG_SECTION);
            if (section == null)
            {
                // Generate defaults
                section = new Dictionary<string, dynamic>
                {
                    [CONFIG_ENFORCE_APIKEY_FIELD] = false
                };

            }
            return section;
        }
        
        private static void UpdateConfigurationSetting(string field, dynamic value)
        {
            var section = Config.Instance.GetCustomSection<Dictionary<string, dynamic>>(CONFIG_SECTION);
            section[field] = value;
            Config.Instance.SetCustomSection(CONFIG_SECTION, section);
        }

        public static string ApiKey
        {
            get
            {
                return GetConfigurationSettings()[CONFIG_APIKEY_FIELD] ?? GenerateApiKey();
            }
            set
            {
                UpdateConfigurationSetting(CONFIG_APIKEY_FIELD, value);
            }
        }

        public static bool EnforceApiKey
        {
            get => GetConfigurationSettings()[CONFIG_ENFORCE_APIKEY_FIELD];
            set => UpdateConfigurationSetting(CONFIG_ENFORCE_APIKEY_FIELD, value);
        }

        public static string GenerateApiKey()
        {
            var newKey = Guid.NewGuid().ToString().Replace("-", "");
            Config.Instance.SetCustomSection(CONFIG_SECTION, newKey);
            return newKey;
        }

        public static bool ValidateApiKey(string apiKey)
        {
            if (!EnforceApiKey)
                return true; // If not enforcing, any key is "valid"
            return ApiKey == apiKey;
        }
    }
}
