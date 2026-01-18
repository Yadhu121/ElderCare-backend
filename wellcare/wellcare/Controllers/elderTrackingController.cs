using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace wellcare.Controllers
{
    [Authorize]
    public class elderTrackingController : Controller
    {
        public IActionResult LiveTracking(int elderId)
        {
            ViewBag.ElderId = elderId;
            return View();
        }
    }
}
