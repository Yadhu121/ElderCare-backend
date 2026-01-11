using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using wellcare.Models;

namespace wellcare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // ❌ MVC Home page not used in API
        // public IActionResult Index()
        // {
        //     return View();
        // }

        // ================= HEALTH CHECK =================

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new
            {
                status = "API is running",
                time = DateTime.UtcNow
            });
        }

        // ❌ MVC privacy page not used in API
        // public IActionResult Privacy()
        // {
        //     return View();
        // }

        // ================= ERROR TEST ENDPOINT =================

        [HttpGet("error")]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            _logger.LogError("Error endpoint hit. RequestId: {RequestId}", requestId);

            return Problem(
                detail: "An error occurred while processing your request.",
                instance: requestId
            );
        }

        // ❌ MVC ResponseCache attribute not needed in API
        // [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    }
}
