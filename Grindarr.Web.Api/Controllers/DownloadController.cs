using Grindarr.Core;
using Grindarr.Core.Downloaders;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grindarr.Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<IDownloadItem> Index()
        {
            foreach (var dl in DownloadManager.Instance.DownloadQueue)
                yield return dl;
        }

        [HttpGet("{guid}")]
        public IActionResult Get(Guid guid)
        {
            var dl = DownloadManager.Instance.GetById(guid);
            if (dl == null)
                return BadRequest($"Download was not found with specified guid {guid}");
            return new ObjectResult(dl);
        }

        [HttpDelete("{guid}")]
        public IActionResult Delete(Guid guid)
        {
            var dl = DownloadManager.Instance.GetById(guid);
            if (dl == null)
                return BadRequest($"Download was not found with specified guid {guid}");
            DownloadManager.Instance.Cancel(dl);
            return Ok();
        }

        [HttpPost]
        public ActionResult<IDownloadItem> Post(ContentItem item)
        {
            var dl = new DownloadItem(item, DownloaderFactory.GetBestMatch(item.DownloadLinks));
            DownloadManager.Instance.Enqueue(dl);

            return CreatedAtAction(nameof(Get), new { guid = dl.Id }, dl);
        }

        [HttpPatch("{guid}")]
        public IActionResult JsonPatchWithModelState(Guid guid, [FromBody] JsonPatchDocument<IDownloadItem> patchDoc)
        {
            var dl = DownloadManager.Instance.GetById(guid);
            if (dl == null)
                return BadRequest($"Download was not found with specified guid {guid}");

            if (patchDoc != null)
            {
                patchDoc.ApplyTo(dl);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return new ObjectResult(dl);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost("{guid}/pause")]
        public IActionResult PauseDownload(Guid guid)
        {
            var dl = DownloadManager.Instance.GetById(guid);
            if (dl == null)
                return BadRequest($"Download was not found with specified guid {guid}");

            DownloadManager.Instance.Pause(dl);
            return Ok();
        }

        [HttpPost("{guid}/resume")]
        public IActionResult ResumeDownload(Guid guid)
        {
            var dl = DownloadManager.Instance.GetById(guid);
            if (dl == null)
                return BadRequest($"Download was not found with specified guid {guid}");

            DownloadManager.Instance.Resume(dl);
            return Ok();
        }
    }
}
