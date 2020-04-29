using Grindarr.Core;
using Grindarr.Core.Scrapers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grindarr.Web.Api.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ActionsController
    {
        [HttpPost("search/{query}")]
        public ActionResult<IEnumerable<ContentItem>> SearchAction(string query)
        {
            return new ActionResult<IEnumerable<ContentItem>>(ScraperManager.Instance.Search(query));
        }
    }
}
