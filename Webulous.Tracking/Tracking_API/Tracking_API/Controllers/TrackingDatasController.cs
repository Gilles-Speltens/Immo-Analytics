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
        private IPManager _ipManager;

        public TrackingDatasController(FileLogService logService, IPManager ipManager)
        {
            _logService = logService;
            _ipManager = ipManager;
        }

        [HttpPost]
        public ActionResult PostRequest([FromBody] RequestLogDto log)
        {
            _logService.AddEntryToQueue(log);

            return NoContent();
        }

        [HttpGet]
        public string[] GetIps()
        {
            
            return _ipManager.getSafeList();
        }

        [HttpPost("AddIp")]
        public ActionResult AddIp([FromBody] string ip)
        {
            Console.WriteLine("Add : " + ip);
            _ipManager.AddIpToSafeList(ip);
            return Ok(_ipManager.getSafeList());
        }

        [HttpPost("DeleteIp")]
        public IActionResult DeleteIp([FromBody] string ip)
        {
            Console.WriteLine("Delete : " + ip);
            _ipManager.RemoveIpToSafeList(ip);
            return Ok(_ipManager.getSafeList());
        }
    }
}
