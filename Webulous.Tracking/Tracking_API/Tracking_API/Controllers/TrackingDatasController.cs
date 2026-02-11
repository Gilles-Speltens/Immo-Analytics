using Microsoft.AspNetCore.Mvc;
using System.Text;
using Tracking_API.Model;
using Tracking_API.Model.Dto;

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
        public ActionResult PostRequest([FromBody] RequestLogDto log)
        {
            _logService.AddEntryToQueue(log);

            return NoContent();
        }
    }
}
