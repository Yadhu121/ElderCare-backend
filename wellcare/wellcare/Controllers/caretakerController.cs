using Microsoft.AspNetCore.Mvc;
// using System.Data.SqlClient; // ❌ Not used in this controller
using System.Security.Claims;
using wellcare.Models;

namespace wellcare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaretakerController : ControllerBase
    {
        private readonly caretakerTable _table;

        public CaretakerController(caretakerTable table)
        {
            _table = table;
        }

        // ❌ MVC View not used in API
        // public IActionResult Index()
        // {
        //     return View();
        // }

        // ================= GET PROFILE =================

        [HttpGet("profile")]
        public IActionResult GetCareProfile()
        {
            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (caretakerIdClaim == null)
                return Unauthorized("Caretaker not logged in.");

            if (!int.TryParse(caretakerIdClaim, out int caretakerId))
                return BadRequest("Invalid caretaker ID.");

            var profile = _table.caretakerProfile(caretakerId);

            if (profile == null)
                return NotFound("Profile not found.");

            return Ok(profile);
        }

        // ================= UPDATE PROFILE =================

        [HttpPost("profile")]
        public IActionResult UpdateCareProfile(
            [FromForm] string Bio,
            [FromForm] IFormFile? Photo
        )
        {
            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (caretakerIdClaim == null)
                return Unauthorized("Caretaker not logged in.");

            if (!int.TryParse(caretakerIdClaim, out int caretakerId))
                return BadRequest("Invalid caretaker ID.");

            string? photoPath = null;

            if (Photo != null && Photo.Length > 0)
            {
                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/caretakers"
                );

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = caretakerId + Path.GetExtension(Photo.FileName);
                string fullPath = Path.Combine(folder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                Photo.CopyTo(stream);

                photoPath = "/uploads/caretakers/" + fileName;
            }

            _table.UpdateCaretakerProfile(caretakerId, Bio, photoPath);

            return Ok(new { message = "Profile updated successfully." });
        }
    }
}
