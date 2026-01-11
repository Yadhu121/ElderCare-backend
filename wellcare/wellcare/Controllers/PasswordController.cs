using Microsoft.AspNetCore.Mvc;
using wellcare.Models;
using wellcare.Services;

namespace wellcare.Controllers
{
    [ApiController]
    [Route("api/password")]
    public class PasswordController : ControllerBase
    {
        private readonly caretakerTable _caretaker;
        private readonly OtpTable _otp;
        private readonly EmailService _email;

        public PasswordController(caretakerTable caretaker, OtpTable otp, EmailService email)
        {
            _caretaker = caretaker;
            _otp = otp;
            _email = email;
        }

        // ❌ MVC forgot page not used in API
        // [HttpGet]
        // public IActionResult Forgot()
        // {
        //     return View();
        // }

        // ================= REQUEST PASSWORD RESET (SEND OTP) =================

        [HttpPost("forgot")]
        public async Task<IActionResult> Forgot([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string otp = new Random().Next(100000, 999999).ToString();

            _otp.InsertOtpForPasswordReset(model.Email, otp);
            await _email.SendOtpEmailAsync(model.Email, otp);

            // ❌ No redirect in API
            // return RedirectToAction("Reset", new { email = model.Email });

            return Ok(new
            {
                message = "OTP sent to email for password reset",
                email = model.Email
            });
        }

        // ❌ MVC reset page not used in API
        // [HttpGet]
        // public IActionResult Reset(string email)
        // {
        //     return View(new ResetPasswordModel { Email = email });
        // }

        // ================= RESET PASSWORD =================

        [HttpPost("reset")]
        public IActionResult Reset([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int status = _otp.VerifyPasswordResetOtp(model.Email, model.OTP);

            if (status != 1)
            {
                return BadRequest(new { message = "Invalid or expired OTP" });
            }

            string hash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _caretaker.UpdatePassword(model.Email, hash);

            // ❌ MVC temp data + redirect not used
            // TempData["message"] = "Password reset successful";
            // return RedirectToAction("Login", "caretakerLogin");

            return Ok(new { message = "Password reset successful" });
        }
    }
}
