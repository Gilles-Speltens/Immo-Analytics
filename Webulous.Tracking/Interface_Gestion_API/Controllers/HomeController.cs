using Common;
using Common.Exceptions;
using Interface_Gestion_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

namespace Interface_Gestion_API.Controllers
{
    /// <summary>
    /// Controller principal de l'application qui gère l'affichage,
    /// l'ajout et la suppression d'adresses IP via l'API externe.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AdminPasswordValidator _pswValidator;
        private readonly RequestApiService _apiService;
        private readonly WhiteListManager _whiteListManager;

        /// <summary>
        /// Constructeur du controller avec injection de dépendances.
        /// </summary>
        /// <param name="options">Configuration API (ApiSettings)</param>
        /// <param name="factory">Factory pour créer des HttpClient</param>
        /// <param name="logger">Logger pour tracer les actions et erreurs</param>
        public HomeController(IConfiguration config, ILogger<HomeController> logger, RequestApiService apiService, WhiteListManager whiteListManager)
        {
            this._logger = logger;
            _pswValidator = new AdminPasswordValidator(config["MotDePasse"]);
            _apiService = apiService;
            _whiteListManager = whiteListManager;
        }

        /// <summary>
        /// Page principale affichant la liste des IPs.
        /// Récupère la liste depuis l'API.
        /// </summary>
        /// <returns>Vue avec le modèle IpListViewModel</returns>
        [Authentication]
        public async Task<IActionResult> Index()
        {
            if(_whiteListManager.IsEmpty())
            {
                try
                {
                    var ipsTask = _apiService.GetList("/Admin/Ips");
                    var domainsTask = _apiService.GetList("/Admin/Domains");

                    await Task.WhenAll(ipsTask, domainsTask);

                    var (responseIps, listIps) = await ipsTask;
                    var (responseDomains, listDomains) = await domainsTask;

                    if (responseIps == HttpStatusCode.OK && responseDomains == HttpStatusCode.OK)
                    {
                        _whiteListManager.RefreshWhiteListIfEmpty(listIps, listDomains);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _whiteListManager.ClearWhitelist();
                    _logger.LogError("Connection avec l'API perdue");
                }
            }

            return View(_whiteListManager.GetWhiteList());
        }

        /// <summary>
        /// Affiche la page de connexion.
        /// 
        /// Vérifie au préalable si l'API est disponible.
        /// Si l'API n'est pas joignable, un message d'erreur est envoyé à la vue.
        /// </summary>
        /// <returns>
        /// La vue de connexion.
        /// </returns>
        public async Task<IActionResult> SignIn()
        {
            var isAvailable = await _apiService.IsApiAvailableAsync();

            if (!isAvailable)
            {
                ViewBag.ConnectionError = "Connexion à l'API impossible";
            }

            return View();
        }

        /// <summary>
        /// Traite la soumission du formulaire de connexion.
        /// 
        /// Vérifie la validité du mot de passe.
        /// Si le mot de passe est valide il y a une redirection vers l'action "Index".
        /// 
        /// Sinon, un message d'erreur est renvoyé à la vue.
        /// </summary>
        /// <param name="pwd">Mot de passe saisi par l'utilisateur.</param>
        /// <returns>
        /// Redirection vers Index si authentification réussie,
        /// sinon réaffiche la vue avec un message d'erreur.
        /// </returns>
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

                await LaunchAndRefresh(ipSubnet.GetIp(), "/Admin/AddIp");
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

                await LaunchAndRefresh(ipSubnet.GetIp(), "/Admin/DeleteIp");
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

            await LaunchAndRefresh(domain, "/Admin/AddDomain");

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
            await LaunchAndRefresh(domain, "/Admin/DeleteDomain");

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task LaunchAndRefresh (string value, string path)
        {
            try
            {
                var (responseStatus, list) = await _apiService.LaunchPostAsync(value, path);

                if (responseStatus == HttpStatusCode.OK)
                {
                    if (path.Contains("Domain"))
                    {
                        _whiteListManager.RefreshDomainList(list);
                    }
                    else
                    {
                        _whiteListManager.RefreshIpList(list);
                    }
                }
            }
            catch (HttpRequestException e)
            {
                _whiteListManager.ClearWhitelist();
                _logger.LogError("Connection avec l'API perdue");
            }
            
        }
    }
}
