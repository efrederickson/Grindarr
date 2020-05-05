using System;
using System.Collections.Generic;
using System.Text;

namespace Grindarr.Core
{
    // Shame the "extension everything" proposal for C# 8 didn't make it
    public static class CoreConfigExtensions
    {
        private const string IN_PROGRESS_DL_FOLDER_CONFIGPATH = "grindarr.core.config.inProgressDownloadsFolder";
        private const string IN_PROGRESS_DL_FOLDER_DEFAULT = "in-progress";

        private const string COMPLETED_DL_FOLDER_CONFIGPATH = "grindarr.core.config.completedDownloadFolder";
        private const string COMPLETED_DL_FOLDER_DEFAULT = "complete";

        private const string IGNORE_STALLED_DOWNLOADS_CONFIGPATH = "grindarr.core.config.ignoreStalledDownloads";
        private const bool IGNORE_STALLED_DOWNLOADS_DEFAULT = true;

        private const string STALLED_DOWNLOAD_CUTOFF_CONFIGPATH = "grindarr.core.config.stalledDownloadCutoff";
        private const double STALLED_DOWNLOAD_CUTOFF_DEFAULT = 50; // 50 kb/s
        public static string GetInProgressDownloadsFolder(this Config config) => config.GetValue(IN_PROGRESS_DL_FOLDER_CONFIGPATH, IN_PROGRESS_DL_FOLDER_DEFAULT);
        public static void SetInProgressDownloadsFolder(this Config config, string value) => config.SetValue(IN_PROGRESS_DL_FOLDER_CONFIGPATH, value);

        public static string GetCompleteDownloadsFolder(this Config config) => config.GetValue(COMPLETED_DL_FOLDER_CONFIGPATH, COMPLETED_DL_FOLDER_DEFAULT);
        public static void SetCompleteDownloadsFolder(this Config config, string value) => config.SetValue(COMPLETED_DL_FOLDER_CONFIGPATH, value);

        public static bool GetIgnoreStalledDownloads(this Config config) => config.GetValue(IGNORE_STALLED_DOWNLOADS_CONFIGPATH, IGNORE_STALLED_DOWNLOADS_DEFAULT);
        public static void SetIgnoreStalledDownloads(this Config config, bool value) => config.SetValue(IGNORE_STALLED_DOWNLOADS_CONFIGPATH, value);

        public static double GetStalledDownloadsCutoff(this Config config) => config.GetValue(STALLED_DOWNLOAD_CUTOFF_CONFIGPATH, STALLED_DOWNLOAD_CUTOFF_DEFAULT);
        public static void SetStalledDownloadsCutoff(this Config config, double value) => config.SetValue(STALLED_DOWNLOAD_CUTOFF_CONFIGPATH, value);

        public static void RegisterDefaultCoreConfigurationValues(this Config config)
        {
            config.GetInProgressDownloadsFolder();
            config.GetCompleteDownloadsFolder();
            config.GetIgnoreStalledDownloads();
            config.GetStalledDownloadsCutoff();
        }
    }
}
