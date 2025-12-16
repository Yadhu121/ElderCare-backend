using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using wellcare.Models;
using wellcare.Services;

namespace wellcare.Controllers
{
    //[ApiController]
    //[Route("api/caretaker")]
    //public class caretakerRegisterController : ControllerBase
    public class caretakerRegisterController : Controller
    {
        private readonly caretakerTable _careTaker;
        private readonly OtpTable _otp;
        private readonly EmailService _emailService;
        public caretakerRegisterController(caretakerTable caretaker, OtpTable otpRepo, EmailService emailService)
        {
            _careTaker = caretaker;
            _otp = otpRepo;
            _emailService = emailService;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        //[HttpPost("register")]
        [HttpPost]
        //public async Task<IActionResult> Register([FromBody] caretakerRegister model)
        public async Task<IActionResult> Register(caretakerRegister model)
        {
            if (!ModelState.IsValid)
                return View(model);
              //return BadRequest(ModelState);

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var result = _careTaker.RegisterCaretaker(model, passwordHash);

            if (result.Status == -1)
            {
                ViewBag.Error = "Email already registered.";
                return View(model);
                //return Conflict(new { message = "Email already registered." });
            }

            if (result.Status == -2)
            {
                ViewBag.Error = "Phone number already registered.";
                return View(model);
                //return Conflict(new { message = "Phone number already registered." });
            }

            var random = new Random();
            string otp = random.Next(100000, 999999).ToString();

            _otp.InsertOtp(result.CaretakerID, model.Email, otp);
            await _emailService.SendOtpEmailAsync(model.Email, otp);

            return RedirectToAction("Verify", "Otp", new { email = model.Email });

            //return Ok(new
            //{
            //    message = "Registration successful. OTP sent.",
            //    caretakerId = result.CaretakerID
            //});
        }
    }
}
