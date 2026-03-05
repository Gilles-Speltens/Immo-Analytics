using Common;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Mini_Site_Web.Models
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
        /// Envoye une requête à l'API avec dans le corp de la requête les logs au format RequestLog
        /// converti au JSON.
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête en cours.</param>
        public async Task SendLog(HttpContext context)
        {
            try
            {
                //Formatage des logs.
                var logDto = await CreateRequestLog(context);

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
        public async Task<RequestLogDto> CreateRequestLog(HttpContext context)
        {
            var user_cookie_consent = true;//context.Request.Cookies["user_cookie_consent"] == "true";
            var session_cookie_consent = true;//context.Request.Cookies["session_cookie_consent"] == "true";

            var date = DateTime.UtcNow;

            var userId = user_cookie_consent
                ? (context.Request.Cookies["uid"] ?? "null")
                : "null";

            if (context.Session.GetString("init") == null)
            {
                context.Session.SetString("init", "true");
            }
            var sessionId = session_cookie_consent
                ? (context.Session?.Id ?? "null")
                : "null";

            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";

            var urlReferrer = string.IsNullOrEmpty(context.Request.Headers.Referer)
                                ? "null"
                                : context.Request.Headers.Referer.ToString();

            var action = context.Request.Method == "POST"
                    ? await GetBody(context.Request)
                    : "HITPAGE";

            var languageBrowser = Regex.Match(context.Request.Headers.AcceptLanguage, @"^[^,]*").Value;

            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "null";

            return new RequestLogDto
            {
                Date = date,
                UserId = userId,
                SessionId = sessionId,
                Url = url,
                UrlReferrer = urlReferrer,
                Action = action,
                LanguageBrowser = languageBrowser,
                UserAgent = userAgent
            };
        }

        /// <summary>
        /// Lit le corps d'une requête HTTP de manière asynchrone et filtre certaines clés.
        /// </summary>
        /// <param name="request">L'objet HttpRequest à lire.</param>
        /// <returns>
        /// Une chaîne représentant le corps filtré :
        /// - "empty" si le corps filtré est vide
        /// - sinon, le corps filtré sans les segments correspondant aux filtres
        /// </returns>
        private async Task<string> GetBody(HttpRequest request)
        {
            if (request.ContentLength is null or 0)
            {
                return "empty";
            }

            request.EnableBuffering();

            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            var body = await ReadAndFilters(reader, new[] { "JSONSerial" });

            if (body.Equals("")) return "empty";

            return body;
        }

        /// <summary>
        /// Lit un StreamReader jusqu'à la fin du flux et ignore les segments contenant certaines chaînes.
        /// </summary>
        /// <param name="sr">StreamReader déjà initialisé sur le flux à lire.</param>
        /// <param name="filters">Tableau de mots-clés à ignorer dans la lecture.</param>
        /// <returns>
        /// La chaîne concaténée de tous les segments lus, sans ceux correspondant aux filtres,
        /// et sans le dernier caractère '&' si présent.
        /// </returns>
        private async Task<string> ReadAndFilters(StreamReader sr, string[] filters)
        {
            var str = "";
            var param = "";

            do
            {
                param = await ReadUntil(sr, '&');

                var isInFilters = false;
                foreach (var filter in filters)
                {
                    if (param.Contains(filter))
                    {
                        isInFilters = true; break;
                    }
                }

                if (!isInFilters) str = string.Concat(str, param);
            } while (!param.Equals("")) ;

            if (!string.IsNullOrEmpty(str) && str.EndsWith("&"))
            {
                return str.Substring(0, str.Length - 1);
            }

            return str;
        }

        /// <summary>
        /// Lit un StreamReader caractère par caractère jusqu'à atteindre un délimiteur.
        /// </summary>
        /// <param name="rd">StreamReader déjà initialisé sur le flux.</param>
        /// <param name="delimiter">Caractère à utiliser comme limite de lecture.</param>
        /// <returns>
        /// Une chaîne contenant tous les caractères lus jusqu'au délimiteur inclus.
        /// </returns>
        private async Task<string> ReadUntil(StreamReader rd, char delimiter)
        {
            var sb = new StringBuilder();
            var buffer = new char[1];

            while (await rd.ReadAsync(buffer, 0, 1) > 0)
            {
                sb.Append(buffer[0]);

                if (buffer[0] == delimiter)
                    break;
            }

            return sb.ToString();
        }
    }
}
