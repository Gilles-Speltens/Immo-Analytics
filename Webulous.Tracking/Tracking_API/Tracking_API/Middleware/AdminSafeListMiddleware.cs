using Common;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        private readonly DomainManager _domainManager;
        private readonly IPAddress _gestionIp;

        /// <summary>
        /// Initialise le middleware avec les dépendances nécessaires.
        /// </summary>
        /// <param name="next">Delegate pour la requête suivante dans le pipeline</param>
        /// <param name="logger">Logger pour consigner les informations et avertissements</param>
        /// <param name="ipManager">Gestionnaire de whitelist d'IP</param>
        public AdminSafeListMiddleware(
            RequestDelegate next,
            IPManager ipManager,
            DomainManager domainManager,
            IOptions<AdminSafeListOptions> options,
            ILogger<AdminSafeListMiddleware> logger)
        {
            _ipManager = ipManager;
            _domainManager = domainManager;
            _next = next;
            _logger = logger;

            _gestionIp = IPAddress.Parse(options.Value.InterfaceIP);
        }

        /// <summary>
        /// Méthode appelée par le pipeline ASP.NET Core pour chaque requête HTTP.
        /// Vérifie si l'adresse IP distante est autorisée par la whitelist.
        /// Laisse authomitiquement passer les requêtes venant de l'interface de gestion de l'API.
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête</param>
        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableBuffering();

            var badOrigin = true;
            var remoteIp = context.Connection.RemoteIpAddress;

            try
            {
                //Si la requête concerne les whitelists
                if (context.Request.Path.StartsWithSegments("/TrackingDatas"))
                {
                    //var dto = await JsonSerializer.DeserializeAsync<RequestLogDto>(context.Request.Body);

                    //context.Request.Body.Position = 0;

                    //var match = Regex.Match(dto.Url, @"^(?:https?:\/\/)?([^\/:?#]+)");
                    //var remoteDomain = match.Groups[1].Value;

                    var remoteDomain = context.Request.Headers["Domain"].FirstOrDefault();

                    if (remoteIp != null && remoteDomain.Length != 0)
                    {
                        badOrigin = !_ipManager.IsInSafeList(remoteIp.ToString()) && !_domainManager.IsInSafeList(remoteDomain);
                    }
                } else
                {
                    if (remoteIp != null)
                    {
                        if (_gestionIp.Equals(remoteIp))
                        {
                            badOrigin = false;
                        }
                    }
                    
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex.Message, ex);
                badOrigin = true;
            }

            if (badOrigin)
            {
                _logger.LogWarning($"Forbidden Request from remote IP address: {remoteIp}", remoteIp);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            await _next.Invoke(context);
        }
    }
}
