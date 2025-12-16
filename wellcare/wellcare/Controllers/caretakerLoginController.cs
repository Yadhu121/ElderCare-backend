using Microsoft.AspNetCore.Mvc;
using wellcare.Models;

namespace wellcare.Controllers
{
    //[ApiController]
    //[Route("api/caretaker")]
    //public class caretakerLoginController : ControllerBase
    public class caretakerLoginController : Controller
    {

        private readonly caretakerTable _careTaker;

        public caretakerLoginController(caretakerTable careTaker)
        {
            _careTaker = careTaker;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        //[HttpPost("login")]
        [HttpPost]
        //public IActionResult Login([FromBody] caretakerLogin model)
        public IActionResult Login(caretakerLogin model)
        {
            if (!ModelState.IsValid)
                return View(model);
            //return BadRequest(ModelState);

            var result = _careTaker.LoginCaretaker(model);

            if (result.Status == -1)
            {
                ViewBag.Error = "User not found";
                return View(model);
                //return Unauthorized(new { message = "User not found" });
            }

            if (result.Status == -2)
            {
                return RedirectToAction("Verify", "Otp", new { email = model.Email });
                //TempData["message"] = "Email not verified";
                //return View(model);
                //return Unauthorized(new { message = "Email not verified" });
            }

            if (result.Status == -3)
            {
                ViewBag.Error = "Invalid password";
                return View(model);
                //return Unauthorized(new { message = "Invalid password" });
            }
            TempData["message"] = "Successful Login";
            HttpContext.Session.SetInt32("CareTakerID", result.CareTakerID);
            HttpContext.Session.SetString("CareTakerName", result.FirstName);
            return RedirectToAction("Index", "CaretakerHome");
            //return RedirectToAction("Dashboard");
        }
            //public IActionResult Dashboard()
            //{
            //return View();
            //}
        //return Ok(new
        //{
        //    message = "Login successful",
        //   caretakerId = result.CareTakerID,
        //    firstName = result.FirstName
        //});
    }
}
