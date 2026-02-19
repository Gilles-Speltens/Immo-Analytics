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
        private readonly IpListViewModel _ipList = new IpListViewModel();
        private readonly string _APIPath;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Constructeur du controller avec injection de dépendances.
        /// </summary>
        /// <param name="options">Configuration API (ApiSettings)</param>
        /// <param name="factory">Factory pour créer des HttpClient</param>
        /// <param name="logger">Logger pour tracer les actions et erreurs</param>
        public HomeController(IOptions<ApiSettings> options, IHttpClientFactory factory, ILogger<HomeController> logger)
        {
            this._client = factory.CreateClient();
            this._APIPath = options.Value.APIPath;
            this._logger = logger;
        }

        /// <summary>
        /// Page principale affichant la liste des IPs.
        /// Récupère la liste depuis l'API.
        /// </summary>
        /// <returns>Vue avec le modèle IpListViewModel</returns>
        public async Task<IActionResult> Index()
        {
            var response = await _client.GetAsync(_APIPath);

            if (response.IsSuccessStatusCode)
            {
                await RefreshListIp(response);
            } else
            {
                return RedirectToAction("Error");
            }

            return View(_ipList);
        }

        /// <summary>
        /// Ajoute une IP à l'API externe via POST.
        /// </summary>
        /// <param name="ip">Adresse IP à ajouter</param>
        /// <returns>Redirection vers Index</returns>
        [HttpPost]
        public async Task<IActionResult> AddIp(string ip)
        {
            await launchRequest(ip, "/AddIp");

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
            await launchRequest(ip, "/DeleteIp");
                
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task launchRequest(string ip, string path)
        {
            try
            {
                var ipSubnet = new IPSubnet(ip);

                var json = JsonSerializer.Serialize(ipSubnet.GetIp());

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var fullPath = String.Concat(_APIPath, path);

                var response = await _client.PostAsync(fullPath, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Aucune reponse reçu de l'API");
                }
            }
            catch(InvalidIpException e)
            {
                TempData["Error"] = "Adresse IP invalide";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString(), "Erreur lors de l'appel à l'API");
                TempData["Error"] = "API Injoignable";
            }
        }

        /// Met à jour le modèle IpListViewModel à partir de la réponse de l'API.
        /// Sépare IPv4 et IPv6 et enlève le suffixe /32 ou /128 si nécessaire.
        /// </summary>
        /// <param name="response">Réponse HTTP de l'API contenant une liste JSON d'IP</param>
        private async Task RefreshListIp(HttpResponseMessage response)
        {
            var list = await JsonSerializer.DeserializeAsync<List<string>>(
                await response.Content.ReadAsStreamAsync()
            );

            _ipList.IpV4 = new List<IPSubnet>();
            _ipList.IpV6 = new List<IPSubnet>();
            foreach (var i in list)
            {
                if(i.Contains(":"))
                {
                    _ipList.IpV6.Add(new IPSubnet(i));
                } else
                {
                    _ipList.IpV4.Add(new IPSubnet(i));
                }
            }
        }

        /// <summary>
        /// Vérifie si une adresse IP ou un subnet est valide.
        /// Supporte IPv4 et IPv6.
        /// </summary>
        /// <param name="ip">Adresse IP ou subnet au format "IP" ou "IP/Prefix"</param>
        /// <returns>true si valide, false sinon</returns>
        private bool checkIpValidity(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            string[] parts = ip.Split('/');

            if (parts.Length == 1 || parts.Length == 2)
            {
                if (!IPAddress.TryParse(parts[0], out var address))
                {
                    return false;
                }

                if (parts.Length == 2)
                {
                    if (!int.TryParse(parts[1], out int mask))
                    {
                        return false;
                    }

                    if (address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (mask < 0 || mask > 32)
                        {
                            return false;
                        }
                    }
                    else if (address.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        if (mask < 0 || mask > 128)
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
