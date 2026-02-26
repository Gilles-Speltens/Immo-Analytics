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
        private IPManager _ipManager;
        private DomainManager _domainManager;
        private readonly IPAddress _gestionIp;
        private readonly string _errorPath;

        public TrackingDatasController(FileLogService logService, IPManager ipManager, DomainManager domainManager, IConfiguration config)
        {
            _logService = logService;
            _ipManager = ipManager;
            _gestionIp = IPAddress.Parse(config["AdminSafeList:InterfaceIP"]);
            _errorPath = config["PathToErrorLogsFile"];
            _domainManager = domainManager;
        }

        [HttpPost]
        public ActionResult PostRequest()
        {
            var dto = HttpContext.Items["ParsedBody"] as RequestLogDto;
            if (dto == null)
                return BadRequest("DTO manquant");

            _logService.AddEntryToQueue(dto);

            return NoContent();
        }

        [HttpGet("Health")]
        public ActionResult Health()
        {
            Console.WriteLine("test");
            return Ok();
        }

        [HttpGet("Ips")]
        public string[]? GetIps()
        {
            var sendIp = HttpContext.Connection.RemoteIpAddress;
            if (IsAdminRequest())
            {
                return _ipManager.GetSafeList();
            }
            else
            {
                System.IO.File.AppendAllText(_errorPath, $"Tentative de récupération de la white list d'adresse ip via l'addresse {sendIp.ToString()} bloqué" + Environment.NewLine);
                return null;
            }
        }

        [HttpGet("Domains")]
        public string[]? GetDomains()
        {
            var sendIp = HttpContext.Connection.RemoteIpAddress;
            if (IsAdminRequest())
            {
                return _domainManager.GetSafeList();
            }
            else
            {
                System.IO.File.AppendAllText(_errorPath, $"Tentative de récupération de la white list d'adresse ip via l'addresse {sendIp.ToString()} bloqué" + Environment.NewLine);
                return null;
            }
        }

        [HttpPost("AddIp")]
        public ActionResult AddIp([FromBody] string ip)
        {
            var sendIp = HttpContext.Connection.RemoteIpAddress;
            if (IsAdminRequest())
            {
                System.IO.File.AppendAllText(_errorPath, "Ajout : " + ip + Environment.NewLine);
                _ipManager.AddIpToSafeList(ip);
                return Ok(_ipManager.GetSafeList());
            } 
            else
            {
                System.IO.File.AppendAllText(_errorPath, $"Tentative d'ajout de l'ip {ip} via l'addresse {sendIp.ToString()} bloqué" + Environment.NewLine);
                return NoContent();
            }
            
        }

        [HttpPost("DeleteIp")]
        public IActionResult DeleteIp([FromBody] string ip)
        {
            var sendIp = HttpContext.Connection.RemoteIpAddress;
            if (IsAdminRequest())
            {
                System.IO.File.AppendAllText(_errorPath, "Suppression : " + ip + Environment.NewLine);
                _ipManager.RemoveIpFromSafeList(ip);
                return Ok(_ipManager.GetSafeList());
            }
            else
            {
                System.IO.File.AppendAllText(_errorPath, $"Tentative de suppression de l'ip {ip} via l'addresse {sendIp.ToString()} rejeté" + Environment.NewLine);
                return NoContent();
            }
        }

        [HttpPost("AddDomain")]
        public ActionResult AddDomain([FromBody] string domain)
        {
            var sendIp = HttpContext.Connection.RemoteIpAddress;
            if (IsAdminRequest())
            {
                System.IO.File.AppendAllText(_errorPath, "Ajout : " + domain + Environment.NewLine);
                _domainManager.AddDomainToSafeList(domain);
                return Ok(_domainManager.GetSafeList());
            }
            else
            {
                System.IO.File.AppendAllText(_errorPath, $"Tentative d'ajout du domaine {domain} via l'addresse {sendIp.ToString()} bloqué" + Environment.NewLine);
                return NoContent();
            }
        }

        [HttpPost("DeleteDomain")]
        public ActionResult DeleteDomain([FromBody] string domain)
        {
            var sendIp = HttpContext.Connection.RemoteIpAddress;
            if (IsAdminRequest())
            {
                System.IO.File.AppendAllText(_errorPath, "Suppression : " + domain + Environment.NewLine);
                _domainManager.RemoveDomainFromSafeList(domain);
                return Ok(_domainManager.GetSafeList());
            }
            else
            {
                System.IO.File.AppendAllText(_errorPath, $"Tentative d'ajout du domaine {domain} via l'addresse {sendIp.ToString()} bloqué" + Environment.NewLine);
                return NoContent();
            }

        }

        private bool IsAdminRequest()
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            return _gestionIp.Equals(remoteIp);
        }
    }
}
