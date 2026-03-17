using System.Diagnostics;
using Common;
using Microsoft.AspNetCore.Mvc;
using Mini_Site_Web.Models;

namespace Mini_Site_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly List<Collaborator> _collaborators;
        private readonly List<House> _houses;
        private readonly RequestLogService _logService;

        public HomeController(RequestLogService logService)
        {
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

            _logService = logService;
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

        [HttpPost]
        public async Task<IActionResult> ContactButton()
        {
            await _logService.SendLog(HttpContext, ActionsType.BUTTON_CLICK, null);
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<IActionResult> Form(string Name, string Email, string Message)
        {
            await _logService.SendLog(HttpContext, ActionsType.CONTACT_REQUEST, Name);
            return new EmptyResult();
        }
    }
}
