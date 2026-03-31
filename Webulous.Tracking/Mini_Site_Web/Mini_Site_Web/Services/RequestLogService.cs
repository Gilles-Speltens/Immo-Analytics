using Common;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Mini_Site_Web.Services
{
    public class RequestLogService
    {
        private readonly HttpClient _client;
        private string _pathAPI;
        private readonly string _domain;

        public RequestLogService(string pathAPI, IHttpClientFactory factory)
        {
            _domain = Regex.Match(pathAPI, @"^(?:https?:\/\/)?([^\/:?#]+)").Groups[1].Value;
            _pathAPI = pathAPI;
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Converti le context en un RequestLogDto puis l'envoie à l'API via la classe SendLog(RequestLogDto logDto).
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête en cours.</param>
        public async Task SendLog(HttpContext context)
        {
            //Formatage des logs.
            var logDto = await CreateRequestLog(context, null, null);
            await SendLog(logDto);
        }

        /// <summary>
        /// Converti le context en un RequestLogDto puis l'envoie à l'API via la classe SendLog(RequestLogDto logDto).
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête en cours.</param>
        public async Task SendLog(HttpContext context, ActionsType? action, string? actionParam)
        {
            //Formatage des logs.
            var logDto = await CreateRequestLog(context, action, actionParam);
            await SendLog(logDto);
        }

        /// <summary>
        /// Envoie les logs au format JSON via une requête POST à l'API de tracking.
        /// </summary>
        /// <param name="logDto">Les logs envoyé à l'API</param>
        public async Task SendLog(RequestLogDto logDto)
        {
            try
            {
                var json = JsonSerializer.Serialize(logDto);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, _pathAPI);
                request.Content = content;

                request.Headers.Add("Domain", _domain);

                await _client.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Failed to send the Logs to the API");
            }

        }

        /// <summary>
        /// Crée un objet RequestLogDto en récupérant toutes les informations de la requête HTTP.
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête en cours.</param>
        /// <returns>Un objet RequestLogDto complet</returns>
        private async Task<RequestLogDto> CreateRequestLog(HttpContext context, ActionsType? actionType, string? actionParam)
        {
            var user_cookie_consent = true;//context.Request.Cookies["user_cookie_consent"] == "true";
            var session_cookie_consent = true;//context.Request.Cookies["session_cookie_consent"] == "true";

            var date = DateTime.UtcNow;

            var userId = user_cookie_consent
                ? (context.Request.Cookies["uid"] ?? null)
                : null;

            var userIp = context.Connection.RemoteIpAddress.ToString();

            if (context.Session.GetString("init") == null)
            {
                context.Session.SetString("init", "true");
            }
            var sessionId = session_cookie_consent
                ? (context.Session?.Id ?? "null")
                : null;

            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";

            var urlReferrer = string.IsNullOrEmpty(context.Request.Headers.Referer)
                                ? null
                                : context.Request.Headers.Referer.ToString();

            var action = ActionsType.HITPAGE;

            if(context.Request.Method == "POST")
            {
                if(actionType != null)
                {
                    action = actionType.Value;
                } else
                {
                    action = ActionsType.UNKNOWN;
                }
            }

            var languageBrowser = Regex.Match(context.Request.Headers.AcceptLanguage, @"^[^,]*").Value;

            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "null";

            return new RequestLogDto
            {
                Date = date,
                UserId = userId,
                UserIp = userIp,
                SessionId = sessionId,
                Url = url,
                UrlReferrer = urlReferrer,
                Action = action,
                ActionParameters = actionParam,
                LanguageBrowser = languageBrowser,
                UserAgent = userAgent
            };
        }
    }
}