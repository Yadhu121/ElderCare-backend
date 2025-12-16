using Microsoft.AspNetCore.Mvc;

namespace wellcare.Controllers
{
    public class logoutController : Controller
    {
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "caretakerLogin");
        }
    }
}
