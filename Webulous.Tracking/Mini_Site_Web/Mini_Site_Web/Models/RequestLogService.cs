using Common;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace Mini_Site_Web.Models
{
    public class RequestLogService
    {
        private readonly HttpClient _client;
        private string _pathAPI;

        public RequestLogService(string pathAPI)
        {
            this._pathAPI = pathAPI;
            _client = new HttpClient();
        }

        /// <summary>
        /// Envoye une requête à l'API avec dans le corp de la requête les logs au format RequestLog
        /// converti au JSON.
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête en cours.</param>
        public async void SendLog(HttpContext context)
        {
            try
            {
                //Formatage des logs.
                var logDto = await CreateRequestLog(context);

                var json = JsonSerializer.Serialize(logDto);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await _client.PostAsync(_pathAPI, content);
            }
            catch (Exception ex)
            {
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
            //Contenu du body sous forme de liste de strings.
            var body = await GetBody(context.Request);

            var userId = context.Request.Cookies["uid"] ?? "null";

            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";

            var urlReferrer = string.IsNullOrEmpty(context.Request.Headers.Referer)
                                ? "null"
                                : context.Request.Headers.Referer.ToString();

            var action = context.Request.Method == "POST"
                    ? string.Join(" | ", body)
                    : "null";

            var languageBrowser = Regex.Match(context.Request.Headers.AcceptLanguage, @"^[^,]*").Value;

            var sessionId = context.Session?.Id ?? "null";

            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "null";

            return new RequestLogDto
            {
                UserId = userId,
                Url = url,
                UrlReferrer = urlReferrer,
                Action = action,
                LanguageBrowser = languageBrowser,
                SessionId = sessionId,
                UserAgent = userAgent
            };
        }

        /// <summary>
        /// Lit le corps de la requête HTTP et renvoie chaque ligne sous forme de liste de string.
        /// </summary>
        /// <param name="request">La requête HTTP à lire.</param>
        /// <returns>Liste de lignes du corps de la requête, ou ["null"] si vide.</returns>
        private async Task<List<string>> GetBody(HttpRequest request)
        {
            var bodyLines = new List<string>();

            if (request.ContentLength is null or 0)
            {
                bodyLines.Add("null");
                return bodyLines;
            }

            request.EnableBuffering();

            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                bodyLines.Add(line);
            }

            request.Body.Position = 0;
            return bodyLines;
        }
    }
}
