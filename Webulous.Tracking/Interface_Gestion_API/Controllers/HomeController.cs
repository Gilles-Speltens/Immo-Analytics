using Interface_Gestion_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Common;
using Common.Exceptions;

namespace Interface_Gestion_API.Controllers
{
    /// <summary>
    /// Controller principal de l'application qui gère l'affichage,
    /// l'ajout et la suppression d'adresses IP via l'API externe.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly HttpClient _client;
        private readonly WhiteListViewModel _whiteList = new WhiteListViewModel();
        private readonly string _APIPath;
        //private readonly ILogger<HomeController> _logger;
        private readonly string _logPath;

        /// <summary>
        /// Constructeur du controller avec injection de dépendances.
        /// </summary>
        /// <param name="options">Configuration API (ApiSettings)</param>
        /// <param name="factory">Factory pour créer des HttpClient</param>
        /// <param name="logger">Logger pour tracer les actions et erreurs</param>
        public HomeController(IOptions<ApiSettings> options, IHttpClientFactory factory, IConfiguration config)
        {
            this._client = factory.CreateClient();
            this._APIPath = options.Value.APIPath;
            this._logPath = config["LogFilePath"];
        }

        /// <summary>
        /// Page principale affichant la liste des IPs.
        /// Récupère la liste depuis l'API.
        /// </summary>
        /// <returns>Vue avec le modèle IpListViewModel</returns>
        public async Task<IActionResult> Index()
        {
            if (_whiteList.IpV4 == null && _whiteList.IpV6 == null && _whiteList.Domains == null)
            {
                var responseIps = await _client.GetAsync(string.Concat(_APIPath, "/Ips"));
                var responseDomains = await _client.GetAsync(string.Concat(_APIPath, "/Domains"));

                if (responseIps.StatusCode == HttpStatusCode.OK && responseDomains.StatusCode == HttpStatusCode.OK)
                {
                    await RefreshListIp(responseIps);
                    await RefreshListDomain(responseDomains);
                }
                else
                {
                    System.IO.File.AppendAllText(_logPath, "Chemin de l'API : " + _APIPath + Environment.NewLine);
                    System.IO.File.AppendAllText(_logPath, "Réponse de l'API : " + responseIps.StatusCode + " / " + responseDomains.StatusCode + Environment.NewLine);
                }
            }

            return View(_whiteList);
        }

        /// <summary>
        /// Ajoute une IP à l'API externe via POST.
        /// </summary>
        /// <param name="ip">Adresse IP à ajouter</param>
        /// <returns>Redirection vers Index</returns>
        [HttpPost]
        public async Task<IActionResult> AddIp(string ip)
        {
            try
            {
                var ipSubnet = new IPSubnet(ip);

                var response = await launchRequest(ipSubnet.GetIp(), "/AddIp");

                if (response != null)
                {
                    await RefreshListIp(response);
                }
            }
            catch (InvalidIpException e)
            {
                TempData["Error"] = "Adresse IP invalide";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Supprime une IP de l'API externe via POST.
        /// </summary>
        /// <param name="ip">Adresse IP à supprimer</param>
        /// <returns>Redirection vers Index</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteIp(string ip)
        {
            try
            {
                var ipSubnet = new IPSubnet(ip);

                var response = await launchRequest(ipSubnet.GetIp(), "/DeleteIp");

                if (response != null)
                {
                    await RefreshListIp(response);
                }
            }
            catch (InvalidIpException e)
            {
                TempData["Error"] = "Adresse IP invalide";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Ajoute un Domaine à l'API externe via POST.
        /// </summary>
        /// <param name="ip">Domaine à ajouter</param>
        /// <returns>Redirection vers Index</returns>
        [HttpPost]
        public async Task<IActionResult> AddDomain(string domain)
        {
            var response = await launchRequest(domain, "/AddDomain");

            if (response != null)
            {
                await RefreshListDomain(response);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Supprime un domaine de l'API externe via POST.
        /// </summary>
        /// <param name="ip">Domaine à supprimer</param>
        /// <returns>Redirection vers Index</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteDomain(string domain)
        {
            var response = await launchRequest(domain, "/DeleteDomain");

            if (response != null)
            {
                await RefreshListDomain(response);
            }

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<HttpResponseMessage?> launchRequest(string value, string path)
        {
            try
            {
                var fullPath = String.Concat(_APIPath, path);

                var json = JsonSerializer.Serialize(value);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine(fullPath);

                var response = await _client.PostAsync(fullPath, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Aucune reponse reçu de l'API");
                }
                
                return response;
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(_logPath, ex.Message + Environment.NewLine);
                //_logger.LogError(ex.ToString(), "Erreur lors de l'appel à l'API");
                TempData["Error"] = "API Injoignable";
                return null;
            }
        }

        /// Met à jour le modèle IpListViewModel à partir de la réponse de l'API.
        /// Sépare IPv4 et IPv6 et enlève le suffixe /32 ou /128 si nécessaire.
        /// </summary>
        /// <param name="response">Réponse HTTP de l'API contenant une liste JSON d'IP</param>
        private async Task RefreshListIp(HttpResponseMessage response)
        {
            var list = new List<string>();
            if(response != null)
            {
                list = await JsonSerializer.DeserializeAsync<List<string>>(
                await response.Content.ReadAsStreamAsync()
                );
            }
            
            if(list != null)
            {
                _whiteList.IpV4 = new List<IPSubnet>();
                _whiteList.IpV6 = new List<IPSubnet>();
                foreach (var i in list)
                {
                    if (i.Contains(":"))
                    {
                        _whiteList.IpV6.Add(new IPSubnet(i));
                    }
                    else
                    {
                        _whiteList.IpV4.Add(new IPSubnet(i));
                    }
                }
            }
        }

        private async Task RefreshListDomain(HttpResponseMessage response)
        {
            var list = new List<string>();
            if (response != null)
            {
                list = await JsonSerializer.DeserializeAsync<List<string>>(
                await response.Content.ReadAsStreamAsync()
                );
            }

            if (list != null)
            {
                _whiteList.Domains = new List<string>(list);
            }
        }
    }
}
