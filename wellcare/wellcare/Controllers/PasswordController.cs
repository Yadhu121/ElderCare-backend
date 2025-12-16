using Microsoft.AspNetCore.Mvc;
using wellcare.Models;
using wellcare.Services;

namespace wellcare.Controllers
{
    public class PasswordController : Controller
    {
        private readonly caretakerTable _caretaker;
        private readonly OtpTable _otp;
        private readonly EmailService _email;

        public PasswordController(caretakerTable caretaker,OtpTable otp,EmailService email)
        {
            _caretaker = caretaker;
            _otp = otp;
            _email = email;
        }

        [HttpGet]
        public IActionResult Forgot()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Forgot(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string otp = new Random().Next(100000, 999999).ToString();

            _otp.InsertOtpForPasswordReset(model.Email, otp);
            await _email.SendOtpEmailAsync(model.Email, otp);

            return RedirectToAction("Reset", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult Reset(string email)
        {
            return View(new ResetPasswordModel { Email = email });
        }

        [HttpPost]
        public IActionResult Reset(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int status = _otp.VerifyPasswordResetOtp(model.Email, model.OTP);

            if (status != 1)
            {
                ViewBag.Error = "Invalid or expired OTP";
                return View(model);
            }

            string hash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _caretaker.UpdatePassword(model.Email, hash);

            TempData["message"] = "Password reset successful";
            return RedirectToAction("Login", "caretakerLogin");
        }
    }
}
