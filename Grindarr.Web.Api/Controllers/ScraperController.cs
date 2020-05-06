using Grindarr.Core.Scrapers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grindarr.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScraperController : ControllerBase
    {
        [HttpGet("available")]
        public IEnumerable<string> GetAvailable() => ScraperManager.GetAllScraperClasses().Select(t => t.AssemblyQualifiedName);

        [HttpGet]
        public IEnumerable<ScraperModel> Index() => ScraperManager.Instance.GetRegisteredScrapers().Select(s => ScraperModel.CreateFromScraper(s));

        [HttpGet("{id}")]
        public ActionResult<ScraperModel> Get(int id)
        {
            var scrapers = ScraperManager.Instance.GetRegisteredScrapers().ToList();
            if (scrapers.Count <= id)
                return BadRequest("Invalid scraper index");
            var target = scrapers[id];
            return ScraperModel.CreateFromScraper(target);
        }

        [HttpPost]
        public IActionResult Post(ScraperModel arg)
        {
            var type = Type.GetType(arg.ClassName);
            if (type == null)
                return BadRequest($"Scraper class {arg.ClassName} does not exist. The argument is case sensitive.");
            IScraper scraper = ScraperManager.Instance.CreateAndRegisterScraper(type, arg.Arguments);

            var index = ScraperManager.Instance.GetRegisteredScrapers().ToList().IndexOf(scraper);
            return CreatedAtAction(nameof(Get), new { id = index }, scraper);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var scrapers = ScraperManager.Instance.GetRegisteredScrapers().ToList();
            if (scrapers.Count <= id)
                return BadRequest("Invalid scraper index");
            var target = scrapers[id];
            bool success = ScraperManager.Instance.Unregister(target);
            if (success)
                return Ok();
            return StatusCode(500, "Something went wrong removing the scraper");
        }
    }
}
