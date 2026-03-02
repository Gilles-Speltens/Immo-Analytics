using Common;
using Microsoft.Extensions.Options;
using System.Net;
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
        private readonly string _logPath;
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
            IConfiguration config)
        {
            _ipManager = ipManager;
            _domainManager = domainManager;
            _next = next;
            _logPath = config["PathToErrorLogsFile"];

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
                if (context.Request.Path.StartsWithSegments("/Admin"))
                {
                    if (remoteIp != null)
                    {
                        if (_gestionIp.Equals(remoteIp))
                        {
                            badOrigin = false;
                        }
                    }
                } else
                {
                    var dto = await JsonSerializer.DeserializeAsync<RequestLogDto>(context.Request.Body);

                    context.Request.Body.Position = 0;

                    var match = Regex.Match(dto.Url, @"^(?:https?:\/\/)?([^\/:?#]+)");
                    var remoteDomain = match.Groups[1].Value;

                    File.AppendAllText(_logPath, $"Request from remote IP address: {remoteIp}" + Environment.NewLine);
                    //_logger.LogDebug("Request from Remote IP address: {RemoteIp}", remoteIp);

                    if (remoteIp != null && remoteDomain.Any())
                    {
                        badOrigin = !_ipManager.IsInSafeList(remoteIp.ToString()) && !_domainManager.IsInSafeList(remoteDomain);
                    }

                    context.Items["ParsedBody"] = dto;
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(_logPath, ex.ToString() + Environment.NewLine);
                badOrigin = true;
            }

            if (badOrigin)
            {
                File.AppendAllText(_logPath, $"Forbidden request from remote IP address: {remoteIp}" + Environment.NewLine);
                //_logger.LogWarning(
                //    "Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            await _next.Invoke(context);
        }
    }
}
