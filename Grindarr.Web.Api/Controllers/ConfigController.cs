using Grindarr.Core;
using Grindarr.Core.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Grindarr.Web.Api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        [HttpGet]
        public Dictionary<string, dynamic> Index()
        {
            var cfg = Config.Instance.GetRawDictionary();
            return cfg;
        }
        
        [HttpPut]
        public IActionResult Put(Dictionary<string, JsonElement> newPartialConfig)
        {
            // Map json elements to underlying type
            var newPartialConfig2 = newPartialConfig.ToDictionary(entry => entry.Key, entry => JsonElementUnwrapper.Unwrap(entry.Value));
            // Update config
            Config.Instance.Merge(newPartialConfig2);
            // Return success
            return Ok();
        }
    }
}
