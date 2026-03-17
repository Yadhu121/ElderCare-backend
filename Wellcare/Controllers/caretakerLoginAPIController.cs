using Microsoft.AspNetCore.Mvc;
using wellcare.Models;
using wellcare.Services;

namespace wellcare.Controllers
{
    [ApiController]
    [Route("api/caretaker")]
    public class caretakerAPIController : ControllerBase
    {
        private readonly caretakerTable _careTaker;
        private readonly JwtService _jwtService;

        public caretakerAPIController(caretakerTable careTaker, JwtService jwtService)
        {
            _careTaker = careTaker;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] caretakerLogin model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = _careTaker.LoginCaretaker(model);

            if (result.Status == -1)
                return Unauthorized(new { message = "User not found" });

            if (result.Status == -2)
                return Unauthorized(new { message = "Email not verified" });

            if (result.Status == -3)
                return Unauthorized(new { message = "Invalid password" });

            var token = _jwtService.GenerateToken(result.CareTakerID, model.Email);

            return Ok(new
            {
                token,
                caretakerId = result.CareTakerID,
                firstName = result.FirstName
            });
        }
    }
}