using Microsoft.AspNetCore.Mvc;
// using System; // ❌ Not required explicitly, Random still works without this using
using System.Threading.Tasks;
using wellcare.Models;
using wellcare.Services;

namespace wellcare.Controllers
{
    [ApiController]
    [Route("api/caretaker/auth")]
    public class CaretakerRegisterController : ControllerBase
    {
        private readonly caretakerTable _careTaker;
        private readonly OtpTable _otp;
        private readonly EmailService _emailService;

        public CaretakerRegisterController(
            caretakerTable caretaker,
            OtpTable otpRepo,
            EmailService emailService)
        {
            _careTaker = caretaker;
            _otp = otpRepo;
            _emailService = emailService;
        }

        // ❌ MVC register page not used in API
        // [HttpGet]
        // public IActionResult Register()
        // {
        //     return View();
        // }

        // ================= REGISTER =================

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] caretakerRegister model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var result = _careTaker.RegisterCaretaker(model, passwordHash);

            if (result.Status == -1)
            {
                return Conflict(new { message = "Email already registered." });
            }

            if (result.Status == -2)
            {
                return Conflict(new { message = "Phone number already registered." });
            }

            var random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            _otp.InsertOtp(result.CaretakerID, model.Email, otp);
            await _emailService.SendOtpEmailAsync(model.Email, otp);

            // ❌ No redirect in API
            // return RedirectToAction("Verify", "Otp", new { email = model.Email });

            return Ok(new
            {
                message = "Registration successful. OTP sent to email.",
                caretakerId = result.CaretakerID,
                email = model.Email
            });
        }
    }
}
