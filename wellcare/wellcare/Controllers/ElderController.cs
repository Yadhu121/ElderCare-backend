using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wellcare.Models;

namespace wellcare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ElderController : ControllerBase
    {
        private readonly elderTable _elderRepo;
        private readonly CaretakerElderService _linkService;
        private readonly elderProfile _elderProfile;

        public ElderController(
            elderTable elderRepo,
            CaretakerElderService linkService,
            elderProfile elderProfile)
        {
            _elderRepo = elderRepo;
            _linkService = linkService;
            _elderProfile = elderProfile;
        }

        // ❌ MVC Add page not used in API
        // [HttpGet]
        // public IActionResult Add()
        // {
        //     return View();
        // }

        // ================= ASSIGN ELDER =================

        [HttpPost("assign")]
        public IActionResult AssignElder([FromBody] AssignElderModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ❌ Session-based auth not used
            // int? caretakerId = HttpContext.Session.GetInt32("CareTakerID");

            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (caretakerIdClaim == null)
                return Unauthorized("Caretaker not logged in.");

            if (!int.TryParse(caretakerIdClaim, out int caretakerId))
                return BadRequest("Invalid caretaker ID.");

            var elder = _elderRepo.GetElderByEmail(model.ElderEmail);
            if (elder == null)
            {
                return NotFound(new { message = "Elder not found" });
            }

            bool passwordOk = BCrypt.Net.BCrypt.Verify(
                model.ElderPassword,
                elder.Value.PasswordHash
            );

            if (!passwordOk)
            {
                return Unauthorized(new { message = "Invalid elder password" });
            }

            // ❌ Nullable version not needed
            // int status = _linkService.AssignElder(caretakerId.Value, model.ElderEmail);

            int status = _linkService.AssignElder(caretakerId, model.ElderEmail);

            if (status == -2)
            {
                return Conflict(new { message = "Elder already linked to a caretaker" });
            }

            // ❌ MVC TempData + redirect not used
            // TempData["Success"] = "Elder added successfully";
            // return RedirectToAction("Index", "CaretakerHome");

            return Ok(new { message = "Elder assigned successfully" });
        }

        // ================= GET ELDER PROFILE =================

        [HttpGet("profile/{elderId:int}")]
        public IActionResult GetElderProfile(int elderId)
        {
            // ❌ Session-based auth not used
            // int? caretakerId = HttpContext.Session.GetInt32("CareTakerID");

            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (caretakerIdClaim == null)
                return Unauthorized("Caretaker not logged in.");

            if (!int.TryParse(caretakerIdClaim, out int caretakerId))
                return BadRequest("Invalid caretaker ID.");

            // ❌ Nullable version not needed
            // var elder = _elderProfile.GetElderProfile(caretakerId.Value, elderId);

            var elder = _elderProfile.GetElderProfile(caretakerId, elderId);

            if (elder == null)
            {
                return Unauthorized(new { message = "Access denied or elder not found" });
            }

            return Ok(elder);
        }
    }
}
