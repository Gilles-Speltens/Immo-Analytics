using Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using Tracking_API.Model;

namespace Tracking_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrackingDatasController : ControllerBase
    {
        private FileLogService _logService;

        public TrackingDatasController(FileLogService logService)
        {
            _logService = logService;
        }

        [HttpPost]
        public ActionResult PostRequest()
        {
            var body = HttpContext.Request.Body;
            if (body == null)
                return BadRequest("null body");

            _logService.AddEntryToQueue(body);

            return NoContent();
        }

    }
}
