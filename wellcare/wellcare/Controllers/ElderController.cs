using Microsoft.AspNetCore.Mvc;
using wellcare.Models;

namespace wellcare.Controllers
{
    public class ElderController : Controller
    {
        private readonly elderTable _elderRepo;
        private readonly CaretakerElderService _linkService;

        public ElderController(
            elderTable elderRepo,
            CaretakerElderService linkService)
        {
            _elderRepo = elderRepo;
            _linkService = linkService;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(AssignElderModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int? caretakerId = HttpContext.Session.GetInt32("CareTakerID");

            if (caretakerId == null)
            {
                return RedirectToAction("Login", "caretakerLogin");
            }


            var elder = _elderRepo.GetElderByEmail(model.ElderEmail);
            if (elder == null)
            {
                ViewBag.Error = "Elder not found";
                return View(model);
            }

            bool passwordOk = BCrypt.Net.BCrypt.Verify(
                model.ElderPassword,
                elder.Value.PasswordHash
            );

            if (!passwordOk)
            {
                ViewBag.Error = "Invalid elder password";
                return View(model);
            }

            int status = _linkService.AssignElder(caretakerId.Value, model.ElderEmail);

            if (status == -2)
            {
                ViewBag.Error = "Elder already linked to a caretaker";
                return View(model);
            }

            TempData["Success"] = "Elder added successfully";
            return RedirectToAction("Index", "CaretakerHome");
        }
    }
}
