using wellcare.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using wellcare.Services;

namespace wellcare.Controllers
{
    //[ApiController]
    //public class OtpController : ControllerBase
    public class OtpController : Controller
    {
        private readonly OtpTable _otp;
        private readonly EmailService _emailService;

        public OtpController(OtpTable otp, EmailService emailService)
        {
            _otp = otp;
            _emailService = emailService;
        }
        [HttpGet]
        public IActionResult Verify(string email)
        {
            var vm = new OtpVerifyModel
            {
                Email = email
            };

            return View(vm);
        }

        //[HttpPost("verify-otp")]
        [HttpPost]
        //public IActionResult VerifyOtp([FromBody] OtpVerifyModel model)
        public IActionResult Verify(OtpVerifyModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            int status = _otp.VerifyOtp(model.Email,model.OTP);

            if (status != 1)
            {
                ViewBag.Error = "Invalid or expired OTP";
                return View(model);
              //return BadRequest(new { message = "Invalid or expired OTP" });
            }

            return RedirectToAction("Login","caretakerLogin");
            //return Ok(new { message = "Email verified successfully" });
        }
        [HttpPost]
        public IActionResult Cancel(string email)
        {
            _otp.VerifyOtp(email, "INVALID");

            TempData["message"] = "Registration cancelled.";
            return RedirectToAction("Register", "caretakerRegister");
        }
        [HttpPost]
        public async Task<IActionResult> Resend(string email)
        {
            string? otp = _otp.ResendOtp(email);

            if (otp == null)
            {
                ViewBag.Error = "Unable to resend OTP.";
                return RedirectToAction("Verify", new { email });
            }

            await _emailService.SendOtpEmailAsync(email, otp);

            TempData["message"] = "New OTP sent to your email.";
            return RedirectToAction("Verify", new { email });
        }
    }
}
