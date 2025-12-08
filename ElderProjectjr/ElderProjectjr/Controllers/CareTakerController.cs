using ElderProjectjr.Models;
using Microsoft.AspNetCore.Mvc;
using ElderProjectjr.Services;
using System;
using System.Threading.Tasks;

namespace ElderProjectjr.Controllers
{
    public class CareTakerController : Controller
    {
        private readonly CareTaker _careTaker;
        private readonly EmailService _emailService;

        public CareTakerController(CareTaker careTaker, EmailService emailService)
        {
            _careTaker = careTaker;
            _emailService = emailService;
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(CaretakerModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var result = _careTaker.RegisterCaretaker(model, passwordHash);

            if (result.Status == -1)
            {
                TempData["message"] = "Email already registered.";
                return View(model);
            }

            if (result.Status == -2)
            {
                TempData["message"] = "Phone number already registered.";
                return View(model);
            }

            var random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            _careTaker.InsertOTP(model.Email, otp);
            await _emailService.SendOtpEmailAsync(model.Email, otp);

            return RedirectToAction("VerifyOtp", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            var vm = new OTPVerifyModel
            {
                Email = email
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult VerifyOtp(OTPVerifyModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int status = _careTaker.VerifyOtp(model.Email, model.OTP);

            if (status == -1)
            {
                ModelState.AddModelError("", "Invalid or expired OTP.");
                return View(model);
            }

            return RedirectToAction("OtpVerifiedSuccess");
        }

        public IActionResult OtpVerifiedSuccess()
        {
            return View();
        }
    }
}
