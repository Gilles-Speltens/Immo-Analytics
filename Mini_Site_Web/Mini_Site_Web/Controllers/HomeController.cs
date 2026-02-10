using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Mini_Site_Web.Models;

namespace Mini_Site_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly List<Collaborator> _collaborators;
        private readonly List<House> _houses;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _collaborators = new()
            {
                new Collaborator { Name = "Marie Martin", Phone = "04 99 99 99", PhotoUrl = "/images/collaborators/collaborator1.jpg" },
                new Collaborator { Name = "Paul Bernard", Phone = "04 99 99 99", PhotoUrl = "/images/collaborators/collaborator2.jpg" },
                new Collaborator { Name = "Stéphanie Kageneck", Phone = "04 99 99 99", PhotoUrl = "/images/collaborators/collaborator3.jpg" }
            };

            _houses = new()
            {
                new House
                {
                    Id = 1,
                    Address = "Woluwe-Saint-Pierre",
                    Price = 350000,
                    ImageUrl = "/images/houses/house1.jpg"
                },
                new House
                {
                    Id = 2,
                    Address = "Ever",
                    Price = 540000,
                    ImageUrl = "/images/houses/house2.jpg"
                },
                new House
                {
                    Id = 3,
                    Address = "Paris",
                    Price = 620000,
                    ImageUrl = "/images/houses/house3.jpg"
                },
                new House
                {
                    Id = 4,
                    Address = "Berlin",
                    Price = 560000,
                    ImageUrl = "/images/houses/house4.jpg"
                }
            };
        }

        public IActionResult Index()
        {
            var viewModel = new HouseCollaboratorViewModel
            {
                Houses = _houses,
                Collaborators = _collaborators
            };
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Faux contrôleur servent simplement à être contactés lors des useractions,
        /// afin que le middleware puisse intercepter la requête.
        /// Le nom ne reflète pas sa véritable fonction afin de ne pas alerter les bloqueurs de publicité.
        /// </summary>
        [HttpPost]
        public IActionResult Validation(string testData)
        {
            return new EmptyResult();
        }

        [HttpPost]
        public IActionResult Form(string Name, string Email, string Message)
        {
            return new EmptyResult();
        }
    }
}
