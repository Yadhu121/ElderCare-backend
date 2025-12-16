using Microsoft.AspNetCore.Mvc;
using wellcare.Models;

namespace wellcare.Controllers
{
    public class CaretakerHomeController : Controller
    {
        private readonly CaretakerElderService _linkService;

        public CaretakerHomeController(CaretakerElderService linkService)
        {
            _linkService = linkService;
        }
        public IActionResult Index()
        {
            int? caretakerId = HttpContext.Session.GetInt32("CareTakerID");

            if (caretakerId == null)
                return RedirectToAction("Login", "caretakerLogin");

            var elders = _linkService.GetAssignedElders(caretakerId.Value);

            return View(elders);
        }

        public IActionResult AddElder()
        {
            return RedirectToAction("Add", "Elder");
        }
    }
}
