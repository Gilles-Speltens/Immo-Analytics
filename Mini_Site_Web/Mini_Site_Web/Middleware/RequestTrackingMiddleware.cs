using System.Text;

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
        private readonly HttpClient client;

        /// <summary>
        /// Constructeur du middleware.
        /// </summary>
        /// <param name="next">Le delegate représentant le prochain middleware du pipeline.</param>
        public RequestTrackingMiddleware(RequestDelegate next)
        {
            _next = next;
            client = new HttpClient();
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
                //Formatage des logs.
                var log = await Format(context);

                Console.Write(log);

                var content = new StringContent(log, Encoding.UTF8, "text/plain");

                //await client.PostAsync("http://localhost:5188/TrackingLogs", content);
            }

            await _next(context);
        }

        /// <summary>
        /// Formatte une requête HTTP en une ligne de log.
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête.</param>
        /// <returns>Une chaîne de caractères représentant la requête pour le tracking.</returns>
        private async Task<string> Format(HttpContext context)
        {
            //Contenu du body sous forme de liste de strings.
            var body = await getBody(context.Request);

            var date = DateTimeOffset.Now;

            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";

            var urlReferrer = string.IsNullOrEmpty(context.Request.Headers.Referer)
                                ? "null"
                                : context.Request.Headers.Referer.ToString();

            var action = context.Request.Method == "POST"
                    ? $"{string.Join(" | ", body)}"
                    : "null";

            var sessionId = context.Session.Id ?? "null";

            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "null";

            return $"{date} - {url} - {urlReferrer} - {action} - {sessionId} - {userAgent}\n";
        }

        /// <summary>
        /// Lit le corps de la requête HTTP et renvoie chaque ligne sous forme de liste de string.
        /// </summary>
        /// <param name="request">La requête HTTP à lire.</param>
        /// <returns>Liste de lignes du corps de la requête, ou ["null"] si vide.</returns>
        private async Task<List<string>> getBody(HttpRequest request)
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
