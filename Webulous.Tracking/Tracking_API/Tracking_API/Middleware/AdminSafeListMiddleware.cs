using System.Net;
using Tracking_API.Model;

namespace Tracking_API.Middleware
{
    /// <summary>
    /// Middleware ASP.NET Core qui filtre les requêtes HTTP
    /// en fonction d'une liste blanche d'IP gérée par IPManager.
    /// </summary>
    public class AdminSafeListMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AdminSafeListMiddleware> _logger;
        private readonly IPManager _ipManager;

        /// <summary>
        /// Initialise le middleware avec les dépendances nécessaires.
        /// </summary>
        /// <param name="next">Delegate pour la requête suivante dans le pipeline</param>
        /// <param name="logger">Logger pour consigner les informations et avertissements</param>
        /// <param name="ipManager">Gestionnaire de whitelist d'IP</param>
        public AdminSafeListMiddleware(
            RequestDelegate next,
            ILogger<AdminSafeListMiddleware> logger,
            IPManager ipManager)
        {
            _ipManager = ipManager;
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Méthode appelée par le pipeline ASP.NET Core pour chaque requête HTTP.
        /// Vérifie si l'adresse IP distante est autorisée par la whitelist.
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête</param>
        public async Task Invoke(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            _logger.LogDebug("Request from Remote IP address: {RemoteIp}", remoteIp);
            var badIp = true;

            try
            {
                if(remoteIp != null) badIp = !_ipManager.IsInSafeList(remoteIp.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                badIp = true;
            }
            

            if (badIp)
            {
                _logger.LogWarning(
                    "Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            await _next.Invoke(context);
        }
    }
}
