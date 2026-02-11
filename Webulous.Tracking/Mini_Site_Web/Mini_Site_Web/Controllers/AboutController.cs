using Microsoft.AspNetCore.Mvc;

namespace Mini_Site_Web.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View("About");
        }

    }
}
