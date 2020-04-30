﻿using Grindarr.Core;
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
        public async IAsyncEnumerable<ContentItem> SearchAction(string query)
        {
            await foreach (var result in ScraperManager.Instance.SearchAsync(query))
                yield return result;
        }
    }
}
