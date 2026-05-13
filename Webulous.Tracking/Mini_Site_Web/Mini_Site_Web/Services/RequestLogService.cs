using Common;
using Common.UserActions;
using Newtonsoft.Json;
using System.Reflection;
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
            var logDto = await CreateRequestLog(context, null);
            await SendLog(logDto);
        }

        /// <summary>
        /// Converti le context en un RequestLogDto puis l'envoie à l'API via la classe SendLog(RequestLogDto logDto).
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête en cours.</param>
        public async Task SendLog(HttpContext context, UserActionsBase? actionParam)
        {
            //Formatage des logs.
            var logDto = await CreateRequestLog(context, actionParam);
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
                //Utilisation de Newtonsoft.Json car System.Text.Json ne gère pas le polymorphisme en .NET 6
                //var settings = new JsonSerializerSettings
                //{
                //    TypeNameHandling = TypeNameHandling.Auto
                //};

                var json = JsonConvert.SerializeObject(logDto);

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
        private async Task<RequestLogDto> CreateRequestLog(HttpContext context, UserActionsBase? actionParam)
        {
            var user_cookie_consent = IsConsentGuid(context);
            //var session_cookie_consent = true;

            // Get current UTC time
            DateTime timeUtc = DateTime.UtcNow;

            // Find the target time zone for Belgium / CEST (UTC + 02:00 in summer)
            TimeZoneInfo cestZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");

            // Convert UTC to the local CEST time
            var date = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cestZone);

            var userId = user_cookie_consent
                ? (context.Request.Cookies["uid"] ?? null)
                : null;

            var userIp = context.Connection.RemoteIpAddress.ToString();

            if (context.Session.GetString("init") == null)
            {
                context.Session.SetString("init", "true");
            }
            var sessionId = context.Session?.Id ?? null;

            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";

            var urlReferrer = string.IsNullOrEmpty(context.Request.Headers.Referer)
                                ? null
                                : context.Request.Headers.Referer.ToString();

            var actionType = ActionsType.HITPAGE;

            switch(actionParam)
            {
                case ClientActions :
                    actionType = ActionsType.CLIENT_ACTION;
                    break;
                case ContactRequest :
                    actionType = ActionsType.CONTACT_REQUEST;
                    break;
                case EstateSearchs :
                    actionType = ActionsType.ESTATE_SEARCH;
                    break;
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
                ActionType = actionType,
                ActionParameters = actionParam,
                LanguageBrowser = languageBrowser,
                UserAgent = userAgent
            };
        }

        private bool IsConsentGuid(HttpContext context)
        {
            if (context.Request.Cookies != null && context.Request.Cookies["w-consent"] != null)
            {
                var cookieObj = context.Request.Cookies["w-consent"];
                if (!string.IsNullOrEmpty(cookieObj))
                {
                    //Format should be : "nv:2021120601_date:2021111_gc:1_pref:{GA:1_GM:1_YT:1_FPX:1_RAD:1}"
                    int pos1 = cookieObj.IndexOf('{');
                    int pos2 = cookieObj.IndexOf('}');

                    if (pos1 > 0 && pos2 > 0 && pos2 > pos1 && (pos1 + 1 < cookieObj.Length && (pos1 + (pos2 - pos1)) < cookieObj.Length))
                    {
                        string purposesVal = cookieObj.Substring(pos1 + 1, pos2 - pos1);

                        if (!string.IsNullOrEmpty(purposesVal))
                        {
                            string[] splitPurposes = purposesVal.Split('_');

                            if (splitPurposes != null && splitPurposes.Count() > 0)
                            {
                                foreach (string sp in splitPurposes)
                                {
                                    if (!string.IsNullOrEmpty(sp) && sp.Contains("GA")) //Google Analytics
                                    {
                                        string concent = sp.Split(':')[1];

                                        if (!string.IsNullOrEmpty(concent) && concent.Equals("1"))
                                            return true;

                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}