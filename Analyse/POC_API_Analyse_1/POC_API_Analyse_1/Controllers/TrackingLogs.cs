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
            Console.WriteLine("test");
            Log.Information(
                    "{DateTime} - {Path} - {UrlReferrer} - {Action} - {SessionId} - {UserAgent}",
                    log.Date,
                    log.Path,
                    log.UrlReferrer,
                    log.Action,
                    log.SessionId,
                    log.UserAgent
                );

            return Ok( );
        }

        //[HttpPost]
        //public void PostTest([FromForm] string action)
        //{
        //    Console.WriteLine(action);
        //}
    }
}
