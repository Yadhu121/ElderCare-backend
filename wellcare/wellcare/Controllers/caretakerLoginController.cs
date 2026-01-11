using Microsoft.AspNetCore.Mvc;
using wellcare.Models;
using wellcare.Services;

namespace wellcare.Controllers
{
    [ApiController]
    [Route("api/caretaker/auth")]
    public class CaretakerLoginController : ControllerBase
    {
        private readonly caretakerTable _careTaker;
        private readonly JwtService _jwtService;

        public CaretakerLoginController(caretakerTable careTaker, JwtService jwtService)
        {
            _careTaker = careTaker;
            _jwtService = jwtService;
        }

        // ❌ MVC login page not used in API
        // [HttpGet]
        // public IActionResult Login()
        // {
        //     return View();
        // }

        // ================= LOGIN =================

        [HttpPost("login")]
        public IActionResult Login([FromBody] caretakerLogin model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _careTaker.LoginCaretaker(model);

            if (result.Status == -1)
            {
                return Unauthorized(new { message = "User not found" });
            }

            if (result.Status == -2)
            {
                return Unauthorized(new
                {
                    message = "Email not verified",
                    action = "VERIFY_OTP",
                    email = model.Email
                });
            }

            if (result.Status == -3)
            {
                return Unauthorized(new { message = "Invalid password" });
            }

            // ❌ MVC temp data not used
            // TempData["message"] = "Successful Login";

            // ❌ Session not used in API
            // HttpContext.Session.SetInt32("CareTakerID", result.CareTakerID);
            // HttpContext.Session.SetString("CareTakerName", result.FirstName);

            var token = _jwtService.GenerateToken(result.CareTakerID, model.Email);

            // ✅ Optional: store JWT in HttpOnly cookie (if using browser-based frontend)
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // ⚠ set true in production (HTTPS)
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            });

            // ❌ No redirect in API
            // return RedirectToAction("Index", "CaretakerHome");

            return Ok(new
            {
                message = "Login successful",
                token = token,
                caretakerId = result.CareTakerID,
                firstName = result.FirstName
            });
        }

        // ❌ MVC dashboard not used
        // public IActionResult Dashboard()
        // {
        //     return View();
        // }
    }
}
