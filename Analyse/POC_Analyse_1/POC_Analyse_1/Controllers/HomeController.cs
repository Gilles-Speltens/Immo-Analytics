using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using POC_Analyse_1.Models;
using POC_Analyse_1.Models.DTO;

namespace POC_Analyse_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private LogManager _logManager;

        public HomeController(ILogger<HomeController> logger, LogManager logManager)
        {
            _logger = logger;
            this._logManager = logManager;
        }

        public IActionResult Index()
        {
            generateLogs();
            return View();
        }

        public IActionResult Privacy()
        {
            generateLogs();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void generateLogs()
        {
            var log = new RequestLogDto(
                Request.Headers.UserAgent,
                DateTime.Now,
                Request.Path.ToString(),
                Request.GetTypedHeaders().Referer?.ToString(),
                HttpContext.Session.Id,
                null
                );

            _logManager.addLogs(log);
        }
    }
}
