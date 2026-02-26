using Common;
using Common.Exceptions;
using Interface_Gestion_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

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
        private readonly ILogger<HomeController> _logger;
        private readonly AdminPasswordValidator _pswValidator;

        /// <summary>
        /// Constructeur du controller avec injection de dépendances.
        /// </summary>
        /// <param name="options">Configuration API (ApiSettings)</param>
        /// <param name="factory">Factory pour créer des HttpClient</param>
        /// <param name="logger">Logger pour tracer les actions et erreurs</param>
        public HomeController(IOptions<ApiSettings> options, IHttpClientFactory factory, IConfiguration config, ILogger<HomeController> logger)
        {
            this._client = factory.CreateClient();
            this._APIPath = options.Value.APIPath;
            this._logger = logger;
            _pswValidator = new AdminPasswordValidator(config["MotDePasse"]);
        }

        /// <summary>
        /// Page principale affichant la liste des IPs.
        /// Récupère la liste depuis l'API.
        /// </summary>
        /// <returns>Vue avec le modèle IpListViewModel</returns>
        [Authentication]
        public async Task<IActionResult> Index()
        {
            if (!_whiteList.IPv4.Any() && !_whiteList.IPv6.Any() && !_whiteList.Domains.Any())
            {
                try
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
                        _logger.LogWarning("Réponse de l'API " + _APIPath + " : " + responseDomains.StatusCode);
                    }
                } catch (HttpRequestException ex)
                {
                    _whiteList.IPv4.Clear();
                    _whiteList.IPv6.Clear();
                    _whiteList.Domains.Clear();
                    _logger.LogError("Connection avec l'API perdue");
                }
            }

            return View(_whiteList);
        }

        public async Task<IActionResult> SignIn()
        {
            var isAvailable = await IsApiAvailableAsync();

            if (!isAvailable)
            {
                ViewBag.ConnectionError = "Connexion à l'API impossible";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(string pwd)
        {
            if (_pswValidator.ValidPassword(pwd))
            {
                HttpContext.Session.SetString("isAuthenticated", "true");
                return RedirectToAction("Index");
            }
            
            ViewBag.PasswordError = "Mot de passe incorrect";
            return View();
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

        private async Task<bool> IsApiAvailableAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_APIPath}/Health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
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
                    _logger.LogError("API injoignable");
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
            try
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
                    _whiteList.IPv4 = new List<IPSubnet>();
                    _whiteList.IPv6 = new List<IPSubnet>();
                    foreach (var i in list)
                    {
                        if (i.Contains(":"))
                        {
                            _whiteList.IPv6.Add(new IPSubnet(i));
                        }
                        else
                        {
                            _whiteList.IPv4.Add(new IPSubnet(i));
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError("Réponse de l'API pas au format JSON");
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
