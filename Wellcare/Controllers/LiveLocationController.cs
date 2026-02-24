using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wellcare.Models;

namespace wellcare.Controllers
{
    [Authorize]
    [Route("api/live")]
    public class LiveLocationController : Controller
    {
        private readonly IConfiguration _config;
        private readonly CaretakerElderService _linkService;

        public LiveLocationController(
            IConfiguration config,
            CaretakerElderService linkService)
        {
            _config = config;
            _linkService = linkService;
        }

        [HttpGet("{elderId}")]
        public async Task<IActionResult> GetLocation(int elderId)
        {

            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (caretakerIdClaim == null)
                return Unauthorized();

            int caretakerId = int.Parse(caretakerIdClaim);

            var elders = _linkService.GetAssignedElders(caretakerId);

            if (!elders.Any(e => e.ElderID == elderId))
                return Forbid();

            var microBase = _config["LocationService:RestBase"];

            if (string.IsNullOrEmpty(microBase))
                return StatusCode(500, "Microservice base URL not configured");

            var url = $"{microBase}/location/{elderId}";

            using var client = new HttpClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var content = await response.Content.ReadAsStringAsync();

            return Content(content, "application/json");
        }
    }
}
