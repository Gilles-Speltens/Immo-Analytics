using System.Diagnostics;
using Common.UserActions;
using Microsoft.AspNetCore.Mvc;
using Mini_Site_Web.Models;
using Mini_Site_Web.Services;

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
        public async Task<IActionResult> ContactButton([FromBody]ClientActionViewModel action)
        {
            ClientActionType actionType;
            var details = action.TargetUrl;
            switch (action.ClientAction)
            {
                case "external-link":
                    actionType = ClientActionType.EXTERNAL_LINK;
                    break;
                case "print":
                    actionType = ClientActionType.PRINT;
                    break;
                case "details":
                    actionType = ClientActionType.DETAILS;
                    break;
                case "email":
                    actionType = ClientActionType.EMAILS;
                    break;
                case "phone-call":
                    actionType = ClientActionType.PHONE_CALL;
                    break;
                case "virtual-visit":
                    actionType = ClientActionType.VIRTUAL_VISIT;
                    break;
                case "share":
                    actionType = ClientActionType.SHARE;
                    break;
                case "download":
                    actionType = ClientActionType.DOWNLOAD;
                    break;
                default:
                    actionType = ClientActionType.UNKNOWN;
                    break;
            }
            var param = new ClientActions { ActionType = actionType, Details = details };
            _logService.SendLog(HttpContext, param);
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<IActionResult> ContactForm(string Name, string Email, string Message)
        {
            var contact = new ContactRequestParameters { ConType = ContactType.STANDARD_INFO, EstateId = null, SearchParameters = null };
            _logService.SendLog(HttpContext, contact);
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<IActionResult> EstateInfoForm(bool toSell, string estateType, string locality, int minPrice, int maxPrice, int bedroomNb)
        {
            var param = new EstateSearchs { ToSell = toSell, EstateType = estateType, Locality = locality, MinPrice = minPrice, MaxPrice = maxPrice, BedroomNb = bedroomNb };
            var contact = new ContactRequestParameters { ConType = ContactType.NEW_ESTATES_NOTIFICATION, EstateId = null, SearchParameters = param };
            _logService.SendLog(HttpContext, contact);
            return new EmptyResult();
        }
    }
}
