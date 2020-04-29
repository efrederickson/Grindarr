using Grindarr.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grindarr.Web.Api.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        [HttpGet]
        public ActionResult<Config.BareConfig> Index()
        {
            var cfg = Config.Instance.GetBareConfig().Clone();
            cfg.CustomSections = null; // TODO
            return new ActionResult<Config.BareConfig>(cfg);
        }
        
        [HttpPut]
        public ActionResult Put(Config.BareConfig newPartialConfig)
        {
            Config.Instance.Merge(newPartialConfig);
            return Ok();
        }
    }
}
