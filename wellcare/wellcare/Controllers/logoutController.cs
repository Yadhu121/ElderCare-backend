using Microsoft.AspNetCore.Mvc;

namespace wellcare.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class LogoutController : ControllerBase
    {
        // ================= LOGOUT =================

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // ❌ Session not used in API
            // HttpContext.Session.Clear();

            // ✅ Delete JWT cookie (if you are using cookie-based auth)
            Response.Cookies.Delete("access_token");

            // ❌ No redirect in API
            // return RedirectToAction("Login", "caretakerLogin");

            return Ok(new { message = "Logged out successfully" });
        }
    }
}
