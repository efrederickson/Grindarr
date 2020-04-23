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
            return new ActionResult<Config.BareConfig>(Config.Instance.GetBareConfig());
        }
        
        [HttpPut]
        public ActionResult Put(Config.BareConfig newPartialConfig)
        {
            Config.Instance.Merge(newPartialConfig);
            return Ok();
        }
    }
}
