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
        public ActionResult<IEnumerable<DownloadItem>> Index()
        {
            return new ActionResult<IEnumerable<DownloadItem>>(DownloadManager.Instance.DownloadQueue);
        }

        [HttpGet("{guid}")]
        public ActionResult<DownloadItem> Get(Guid guid)
        {
            var dl = DownloadManager.Instance.GetById(guid);
            if (dl == null)
                return BadRequest($"Download was not found with specified guid {guid}");
            return dl;
        }

        [HttpDelete("{guid}")]
        public ActionResult Delete(Guid guid)
        {
            var dl = DownloadManager.Instance.GetById(guid);
            if (dl == null)
                return BadRequest($"Download was not found with specified guid {guid}");
            DownloadManager.Instance.Cancel(dl);
            return Ok();
        }

        [HttpPost]
        public ActionResult<DownloadItem> Post(ContentItem item)
        {
            var dl = new DownloadItem(item, DownloaderFactory.GetBestMatch(item.DownloadLinks));
            DownloadManager.Instance.Enqueue(dl);

            return CreatedAtAction(nameof(Get), new { guid = dl.Id }, dl);
        }

        [HttpPatch("{guid}")]
        public IActionResult JsonPatchWithModelState(Guid guid, [FromBody] JsonPatchDocument<DownloadItem> patchDoc)
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
