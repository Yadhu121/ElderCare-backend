using Microsoft.AspNetCore.Mvc;

namespace wellcare.Controllers
{
    public class logoutController : Controller
    {
        public IActionResult Logout()
        {
            //HttpContext.Session.Clear();
            Response.Cookies.Delete("access_token");
            return RedirectToAction("Login", "caretakerLogin");
        }
    }
}
