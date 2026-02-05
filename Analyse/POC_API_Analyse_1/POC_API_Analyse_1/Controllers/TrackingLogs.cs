using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace POC_API_Analyse_1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrackingLogs : ControllerBase
    {
        [HttpPost]
        public IActionResult PostRequest(RequestLog log)
        {
            Log.Information(
                    "{DateTime} - {Path} - {UrlReferrer} - {Action} - {SessionId} - {UserAgent}",
                    log.Date,
                    log.Url,
                    log.UrlReferrer,
                    log.Action,
                    log.SessionId,
                    log.UserAgent
                );

            return Ok();
        }

        //[HttpPost]
        //public void PostTest([FromForm] string action)
        //{
        //    Console.WriteLine(action);
        //}
    }
}
