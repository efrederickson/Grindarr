using Grindarr.Core.PostProcessors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Grindarr.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostProcessorController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<IPostProcessor>> Index()
        {
            return new ActionResult<IEnumerable<IPostProcessor>>(PostProcessorManager.Instance.PostProcessors);
        }

        [HttpGet("{id}")]
        public ActionResult<IPostProcessor> Get(int id)
        {
            var postProcessors = PostProcessorManager.Instance.PostProcessors;
            if (postProcessors.Count <= id)
                return BadRequest("Invalid scraper index");
            var target = postProcessors[id];
            return new ActionResult<IPostProcessor>(target);
        }

        [HttpPatch("{id}")]
        public ActionResult Patch(int id, System.Collections.Hashtable newOptions)
        {
            newOptions = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable(newOptions);
            object newStateObj = newOptions["enabled"];
            if (newStateObj == null)
                return BadRequest("No key 'enabled' was specified");

            bool newState = ((JsonElement)newStateObj).GetBoolean();

            var postProcessors = PostProcessorManager.Instance.PostProcessors;
            if (postProcessors.Count <= id)
                return BadRequest("Invalid scraper index");
            var target = postProcessors[id];

            try
            {
                PostProcessorManager.Instance.SetPostProcessorEnableState(target, newState);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest("Unable to reconfigure post processor: " + ex.Message);
            }
            return Ok();
        }
    }
}
