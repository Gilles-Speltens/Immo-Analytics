using Microsoft.AspNetCore.Mvc;
using Mini_Site_Web.Models;

namespace Mini_Site_Web.Controllers
{
    public class SalesController : Controller
    {
        private readonly List<House> _houses = new List<House>
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
                },
                new House
                {
                    Id = 5,
                    Address = "Namur",
                    Price = 120000,
                    ImageUrl = "/images/houses/house5.jpg"
                }
            };
        public IActionResult Index()
        {
            return View("Sales", _houses);
        }

        public IActionResult Details(int id)
        {
            var house = _houses.FirstOrDefault(h => h.Id == id);

            if (house == null)
            {
                return NotFound();
            }

            return View(house);
        }

    }
}
