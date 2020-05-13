using Grindarr.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grindarr.Soulseek
{
    public class SoulseekDownloadItem : DownloadItemBase, IDownloadItem
    {
        public string SoulseekUsername { get; set; }
        public string SoulseekFilename { get; set; }

        public static SoulseekDownloadItem ParseFrom(IDownloadItem item)
        {
            if (item is SoulseekDownloadItem ssItem)
                return ssItem;

            if (item.DownloadUri.DnsSafeHost != "soulseek")
                throw new ArgumentException("download item is not a valid soulseek download item");

            var ssFn = System.Web.HttpUtility.UrlDecode(string.Join("", item.DownloadUri.Segments)).Replace("/", "\\");
            if (ssFn.StartsWith("\\"))
                ssFn = ssFn.Substring(1);

            SoulseekDownloadItem newItem = new SoulseekDownloadItem
            {
                CompletedFilename = item.CompletedFilename,
                Content = item.Content,
                DownloadingFilename = item.DownloadingFilename,
                DownloadUri = item.DownloadUri,
                Id = item.Id,
                Progress = item.Progress,
                SoulseekFilename = ssFn,
                SoulseekUsername = item.DownloadUri.UserInfo
            };
            return newItem;
        }
    }
}
