using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using wellcare.Models;
using wellcare.Services;

namespace wellcare.Controllers
{
    [ApiController]
    [Route("api/otp")]
    public class OtpController : ControllerBase
    {
        private readonly OtpTable _otp;
        private readonly EmailService _emailService;

        public OtpController(OtpTable otp, EmailService emailService)
        {
            _otp = otp;
            _emailService = emailService;
        }

        // ❌ MVC Verify page not used in API
        // [HttpGet]
        // public IActionResult Verify(string email)
        // {
        //     var vm = new OtpVerifyModel
        //     {
        //         Email = email
        //     };
        //     return View(vm);
        // }

        // ================= VERIFY OTP =================

        [HttpPost("verify")]
        public IActionResult VerifyOtp([FromBody] OtpVerifyModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int status = _otp.VerifyOtp(model.Email, model.OTP);

            if (status != 1)
            {
                return BadRequest(new { message = "Invalid or expired OTP" });
            }

            // ❌ No redirect in API
            // return RedirectToAction("Login", "caretakerLogin");

            return Ok(new { message = "Email verified successfully" });
        }

        // ================= CANCEL REGISTRATION =================

        [HttpPost("cancel")]
        public IActionResult Cancel([FromBody] string email)
        {
            _otp.VerifyOtp(email, "INVALID");

            // ❌ TempData and redirect not used
            // TempData["message"] = "Registration cancelled.";
            // return RedirectToAction("Register", "caretakerRegister");

            return Ok(new { message = "Registration cancelled" });
        }

        // ================= RESEND OTP =================

        [HttpPost("resend")]
        public async Task<IActionResult> ResendOtp([FromBody] string email)
        {
            string? otp = _otp.ResendOtp(email);

            if (otp == null)
            {
                return NotFound(new { message = "Unable to resend OTP" });
            }

            await _emailService.SendOtpEmailAsync(email, otp);

            // ❌ MVC redirect not used
            // return RedirectToAction("Verify", new { email });

            return Ok(new { message = "New OTP sent to email" });
        }
    }
}
