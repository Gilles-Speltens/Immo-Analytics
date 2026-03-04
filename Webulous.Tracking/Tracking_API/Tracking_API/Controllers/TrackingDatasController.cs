using Microsoft.AspNetCore.Mvc;
using Tracking_API.Model;

namespace Tracking_API.Controllers
{
    /// <summary>
    /// Contrôleur principal recevant les données de tracking.
    ///
    /// Rôle :
    /// - Recevoir les requêtes HTTP POST contenant des données de tracking
    /// - Ne pas désérialiser le body (optimisation performance)
    /// - Transmettre le flux brut au FileLogService
    ///
    /// Architecture :
    /// Client → Controller → FileLogService → ConcurrentQueue → Consumer → Fichier
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TrackingDatasController : ControllerBase
    {
        private FileLogService _logService;

        /// <summary>
        /// Injection du service via le système de dépendances ASP.NET Core.
        /// </summary>
        public TrackingDatasController(FileLogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Endpoint POST recevant les données de tracking.
        ///
        /// - Récupère le Body brut de la requête
        /// - Ne fait aucune désérialisation (gain CPU + GC)
        /// - Envoie directement le Stream au service de log
        /// - Retourne 204 NoContent pour minimiser la taille de réponse
        /// </summary>
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
