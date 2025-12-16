using ElderProjectjr.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Runtime.CompilerServices;

namespace ElderProjectjr.Controllers
{
    public class caretakerLoginController : Controller
    {
        private readonly CareTaker _careTaker;

        public caretakerLoginController(CareTaker careTaker)
        {
            _careTaker = careTaker;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(CaretakerModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = _careTaker.Login(new CaretakerModel
            {
                Email = model.Email,
                Password = model.Password
            });

            if (result.Status == -1)
            {
                ModelState.AddModelError(string.Empty, "Invalid email/phone or password.");
                return View(model);
            }

            if (result.Status == -2)
            {
                ModelState.AddModelError(string.Empty, "Email not verified. Please verify your email first.");
                return View(model);
            }

            if (result.Status == -3)
            {
                ModelState.AddModelError(string.Empty, "Invalid email/phone or password.");
                return View(model);
            }
            HttpContext.Session.SetInt32("CaretakerID", result.CareTakerID);
            HttpContext.Session.SetString("CaretakerName", result.FirstName ?? "Caretaker");

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var id = HttpContext.Session.GetInt32("CaretakerID");
            if (id == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Name = HttpContext.Session.GetString("CaretakerName") ?? "Caretaker";

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}