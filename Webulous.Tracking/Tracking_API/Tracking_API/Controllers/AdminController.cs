using Microsoft.AspNetCore.Mvc;
using System.Net;
using Tracking_API.Model;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace Tracking_API.Controllers
{

    /// <summary>
    /// Contrôleur d'administration de l'API de tracking.
    /// 
    /// Permet :
    /// - La gestion de la whitelist des IP autorisées
    /// - La gestion de la whitelist des domaines autorisés
    /// 
    /// !!! Accès restreint à une IP spécifique définie en configuration
    /// (AdminSafeList:InterfaceIP).
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private IPManager _ipManager;
        private DomainManager _domainManager;
        private readonly IPAddress _gestionIp;
        private readonly ILogger<AdminController> _logger;

        /// <summary>
        /// Constructeur avec injection de dépendances.
        /// 
        /// Récupère l'IP d'administration depuis la configuration :
        /// AdminSafeList:InterfaceIP
        /// </summary>
        public AdminController(IPManager ipManager, DomainManager domainManager, IConfiguration config, ILogger<AdminController> logger)
        {
            _ipManager = ipManager;
            _domainManager = domainManager;
            _gestionIp = IPAddress.Parse(config["AdminSafeList:InterfaceIP"]);
            _logger = logger;
        }

        /// <summary>
        /// Endpoint de santé de l'API.
        /// 
        /// Permet de vérifier que l'API est en ligne.
        /// Aucun contrôle d'accès.
        /// </summary>
        [HttpGet("Health")]
        public ActionResult Health()
        {
            return Ok();
        }

        /// <summary>
        /// Retourne la liste des IP autorisées.
        /// 
        /// Accès autorisé uniquement depuis l'IP d'administration.
        /// </summary>
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
                _logger.LogWarning($"Tentative de récupération de la white list d'adresse ip via l'addresse {sendIp.ToString()} bloqué");
                return null;
            }
        }

        /// <summary>
        /// Retourne la liste des domaines autorisés.
        /// </summary>
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
                _logger.LogWarning($"Tentative de récupération de la white list d'adresse ip via l'addresse {sendIp.ToString()} bloqué");
                return null;
            }
        }

        /// <summary>
        /// Ajoute une IP à la whitelist.
        /// </summary>
        [HttpPost("AddIp")]
        public ActionResult AddIp([FromBody] string ip)
        {
            var sendIp = HttpContext.Connection.RemoteIpAddress;
            if (IsAdminRequest())
            {
                _ipManager.AddIpToSafeList(ip);
                return Ok(_ipManager.GetSafeList());
            }
            else
            {
                _logger.LogWarning($"Tentative d'ajout de l'ip {ip} via l'addresse {sendIp.ToString()} bloqué");
                return NoContent();
            }

        }

        /// <summary>
        /// Supprime une IP de la whitelist.
        /// </summary>
        [HttpPost("DeleteIp")]
        public IActionResult DeleteIp([FromBody] string ip)
        {
            var sendIp = HttpContext.Connection.RemoteIpAddress;
            if (IsAdminRequest())
            {
                _ipManager.RemoveIpFromSafeList(ip);
                return Ok(_ipManager.GetSafeList());
            }
            else
            {
                _logger.LogWarning($"Tentative de suppression de l'ip {ip} via l'addresse {sendIp.ToString()} bloqué");
                return NoContent();
            }
        }

        /// <summary>
        /// Ajoute un domaine à la whitelist.
        /// </summary>
        [HttpPost("AddDomain")]
        public ActionResult AddDomain([FromBody] string domain)
        {
            var sendIp = HttpContext.Connection.RemoteIpAddress;
            if (IsAdminRequest())
            {
                _domainManager.AddDomainToSafeList(domain);
                return Ok(_domainManager.GetSafeList());
            }
            else
            {
                _logger.LogWarning($"Tentative d'ajout du domaine {domain} via l'addresse {sendIp.ToString()} bloqué");
                return NoContent();
            }
        }

        /// <summary>
        /// Supprime un domaine de la whitelist.
        /// </summary>
        [HttpPost("DeleteDomain")]
        public ActionResult DeleteDomain([FromBody] string domain)
        {
            var sendIp = HttpContext.Connection.RemoteIpAddress;
            if (IsAdminRequest())
            {
                _domainManager.RemoveDomainFromSafeList(domain);
                return Ok(_domainManager.GetSafeList());
            }
            else
            {
                _logger.LogWarning($"Tentative de suppression du domaine {domain} via l'addresse {sendIp.ToString()} bloqué");
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
