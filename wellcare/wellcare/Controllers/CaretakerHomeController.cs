using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wellcare.Models;

namespace wellcare.Controllers
{
    [Authorize]
    public class CaretakerHomeController : Controller
    {
        private readonly CaretakerElderService _linkService;

        public CaretakerHomeController(CaretakerElderService linkService)
        {
            _linkService = linkService;
        }
        public IActionResult Index()
        {
            //int? caretakerId = HttpContext.Session.GetInt32("CareTakerID");
            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (caretakerIdClaim == null)
            {
                return RedirectToAction("Login", "caretakerLogin");
            }

            int caretakerId = int.Parse(caretakerIdClaim);


            //if (caretakerId == null)
            //return RedirectToAction("Login", "caretakerLogin");

            //var elders = _linkService.GetAssignedElders(caretakerId.Value);
            var elders = _linkService.GetAssignedElders(caretakerId);

            return View(elders);
        }

        public IActionResult AddElder()
        {
            return RedirectToAction("Add", "Elder");
        }
    }
}
