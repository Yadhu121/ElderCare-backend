using Microsoft.AspNetCore.Mvc;

namespace Wellcare.Controllers
{
    public class landingController : Controller
    {
        public IActionResult landing()
        {
            return View();
        }
    }
}
