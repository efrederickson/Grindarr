using Grindarr.Core;
using Grindarr.Core.Scrapers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Grindarr.Web.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class FeedController : ControllerBase
    {
        /// <summary>
        /// Returns <code>count</code> latest items as an RSS/Atom feed
        /// </summary>
        /// <param name="count"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        [HttpGet] // Index action
        public async Task<IActionResult> GetLatestItemsRssFeed(int count = 100, string format = "rss")
        {
            var feed = ScraperManager.Instance.CreateSyndicationFeedFromLatestItemsAsync(count);

            var settings = new XmlWriterSettings
            {
                Async = true,
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.Entitize,
                NewLineOnAttributes = true,
                Indent = true
            };
            var contentType = "rss";

            using var stream = new MemoryStream();
            using var xmlWriter = XmlWriter.Create(stream, settings);
            SyndicationFeedFormatter rssFormatter = null;

            switch (format)
            {
                case "atom":
                    rssFormatter = new Atom10FeedFormatter(await feed);
                    contentType = "atom";
                    break;
                default:
                    rssFormatter = new Rss20FeedFormatter(await feed, true);
                    break;
            }
            rssFormatter.WriteTo(xmlWriter);
            xmlWriter.Flush();

            return File(stream.ToArray(), $"application/{contentType}+xml; charset=utf-8");
        }
    }
}
