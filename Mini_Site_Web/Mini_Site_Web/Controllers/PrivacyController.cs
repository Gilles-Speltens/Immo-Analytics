using Microsoft.AspNetCore.Mvc;

namespace Mini_Site_Web.Controllers
{
    public class PrivacyController : Controller
    {
        public IActionResult Index()
        {
            return View("Privacy");
        }

    }
}
