using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wellcare.Models;

namespace wellcare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CaretakerHomeController : ControllerBase
    {
        private readonly CaretakerElderService _linkService;

        public CaretakerHomeController(CaretakerElderService linkService)
        {
            _linkService = linkService;
        }

        // ❌ MVC View endpoint not used in API
        // public IActionResult Index()
        // {
        //     return View();
        // }

        // ================= GET ASSIGNED ELDERS =================

        [HttpGet("elders")]
        public IActionResult GetAssignedElders()
        {
            // ❌ Session-based auth not used
            // int? caretakerId = HttpContext.Session.GetInt32("CareTakerID");

            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (caretakerIdClaim == null)
                return Unauthorized("Caretaker not logged in.");

            if (!int.TryParse(caretakerIdClaim, out int caretakerId))
                return BadRequest("Invalid caretaker ID.");

            // ❌ Old nullable version not needed
            // var elders = _linkService.GetAssignedElders(caretakerId.Value);

            var elders = _linkService.GetAssignedElders(caretakerId);

            return Ok(elders);
        }

        // ================= API NAVIGATION HELPERS (OPTIONAL) =================
        // In API we do NOT redirect to pages. Frontend decides navigation.

        // ❌ Redirect not valid in API
        // public IActionResult AddElder()
        // {
        //     return RedirectToAction("Add", "Elder");
        // }

        // ❌ Redirect not valid in API
        // public IActionResult caretakerProfile()
        // {
        //     return RedirectToAction("careProfile", "caretaker");
        // }
    }
}
