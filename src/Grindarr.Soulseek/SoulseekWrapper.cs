using Grindarr.Core;
using Soulseek;
using System;

namespace Grindarr.Soulseek
{
    /// <summary>
    /// Provide a single shared instance of a soulseek client
    /// </summary>
    public class SoulseekWrapper
    {
        private const string CONFIG_USERNAME = "grindarr.soulseek.username";
        private const string CONFIG_PASSWORD = "grindarr.soulseek.password";

        private static SoulseekWrapper _instance;
        public static SoulseekWrapper Instance => _instance ??= new SoulseekWrapper();

        private static SoulseekClient client;

        private SoulseekWrapper()
        {

        }

        /// <summary>
        /// Returns the existing soulseek client, or creates a new one if the configured username and password are not empty.
        /// </summary>
        /// <returns></returns>
        public SoulseekClient GetClient() => GetOrInitializeClient();

        /// <summary>
        /// Returns the existing soulseek client, or creates a new one if the configured username and password are not empty.
        /// </summary>
        /// <returns></returns>
        private SoulseekClient GetOrInitializeClient()
        {
            client ??= new SoulseekClient(new SoulseekClientOptions());

            if (client.State != SoulseekClientStates.LoggedIn)
            {
                var username = GetUsername();
                var password = GetPassword();

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                    client.ConnectAsync(username, password).Wait();
            }

            return client;
        }

        /// <summary>
        /// Updates the username used for soulseek authentication
        /// </summary>
        /// <param name="username"></param>
        public void SetUsername(string username)
        {
            Config.Instance.SetValue<string>(CONFIG_USERNAME, username);
            if (client != null)
            {
                client.Disconnect();
                client = null;
            }
        }

        /// <summary>
        /// Updates the password used for soulseek authentication
        /// </summary>
        /// <param name="password"></param>
        public void SetPassword(string password)
        {
            Config.Instance.SetValue<string>(CONFIG_PASSWORD, password);
            if (client != null)
            {
                client.Disconnect();
                client = null;
            }
        }

        /// <summary>
        /// Gets the username used for soulseek authentication
        /// </summary>
        /// <returns></returns>
        public string GetUsername() => Config.Instance.GetValue<string>(CONFIG_USERNAME, default);

        /// <summary>
        /// Gets the password used for soulseek authentication
        /// </summary>
        /// <returns></returns>
        public string GetPassword() => Config.Instance.GetValue<string>(CONFIG_PASSWORD, default);
    }
}
