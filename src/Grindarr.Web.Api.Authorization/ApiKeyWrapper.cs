using Grindarr.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grindarr.Web.Api.Authorization
{
    public static class ApiKeyWrapper
    {
        private const string CONFIGPATH_APIKEY = "grindarr.web.api.authorization.apiKey";
        private const string CONFIGPATH_ENFORCE_APIKEY = "grindarr.web.api.authorization.enforceApiKey";

        public static string ApiKey
        {
            get
            {
                return Config.Instance.GetValue<string>(CONFIGPATH_APIKEY, null) ?? GenerateApiKey();
            }
            set => Config.Instance.SetValue(CONFIGPATH_APIKEY, value);
        }

        public static bool EnforceApiKey
        {
            get => Config.Instance.GetValue(CONFIGPATH_ENFORCE_APIKEY, false);
            set => Config.Instance.SetValue(CONFIGPATH_ENFORCE_APIKEY, value);
        }

        private static string GenerateApiKey() => Config.Instance.SetValue(CONFIGPATH_APIKEY, Guid.NewGuid().ToString().Replace("-", ""));

        public static bool ValidateApiKey(string apiKey)
        {
            if (!EnforceApiKey)
                return true; // If not enforcing, any key is "valid"
            return ApiKey == apiKey;
        }

        public static void RegisterDefaultConfiguration()
        {
            // Ensure the defaults are generated or loaded
            _ = EnforceApiKey;
            _ = ApiKey;
        }
    }
}
