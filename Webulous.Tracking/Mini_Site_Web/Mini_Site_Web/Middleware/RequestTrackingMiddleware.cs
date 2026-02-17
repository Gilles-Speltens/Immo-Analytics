using Mini_Site_Web.Models;

namespace Mini_Site_Web.Middleware
{
    /// <summary>
    /// Middleware pour intercepter les requêtes HTTP GET et POST,
    /// collecter des informations de tracking et les envoyer à une API externe
    /// sous forme de ligne de log.
    /// </summary>
    public class RequestTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestLogService _logService;

        /// <summary>
        /// Constructeur du middleware.
        /// </summary>
        /// <param name="next">Le delegate représentant le prochain middleware du pipeline.</param>
        public RequestTrackingMiddleware(RequestDelegate next, RequestLogService logService)
        {
            _next = next;
            _logService = logService;
        }

        /// <summary>
        /// Méthode appelée pour chaque requête HTTP.
        /// Intercepte les requêtes GET et POST et envoie un log.
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête en cours.</param>
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Method == "GET" || context.Request.Method == "POST")
            {
                _logService.SendLog(context);
            }

            await _next(context);
        }
    }
}
