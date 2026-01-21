using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using wellcare.Models;
using wellcare.Services;

namespace wellcare.Controllers
{
    [Authorize]
    public class ElderController : Controller
    {
        private readonly elderTable _elderRepo;
        private readonly CaretakerElderService _linkService;
        private readonly elderProfile _elderProfile;
        private readonly JwtService _jwtService;
        private readonly IConfiguration _config;

        public ElderController(
            elderTable elderRepo,
            CaretakerElderService linkService,
            elderProfile elderProfile,
            JwtService jwtService,
            IConfiguration config)
        {
            _elderRepo = elderRepo;
            _linkService = linkService;
            _elderProfile = elderProfile;
            _jwtService = jwtService;
            _config = config;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add(AssignElderModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            //int? caretakerId = HttpContext.Session.GetInt32("CareTakerID");

            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (caretakerIdClaim == null)
            {
                return RedirectToAction("Login", "caretakerLogin");
            }

            int caretakerId = int.Parse(caretakerIdClaim);


            //if (caretakerId == null)
            //{
            //    return RedirectToAction("Login", "caretakerLogin");
            //}


            var elder = _elderRepo.GetElderByEmail(model.ElderEmail);
            if (elder == null)
            {
                ViewBag.Error = "Elder not found";
                return View(model);
            }

            bool passwordOk = BCrypt.Net.BCrypt.Verify(
                model.ElderPassword,
                elder.Value.PasswordHash
            );

            if (!passwordOk)
            {
                ViewBag.Error = "Invalid elder password";
                return View(model);
            }

            //int status = _linkService.AssignElder(caretakerId.Value, model.ElderEmail);
            int status = _linkService.AssignElder(caretakerId, model.ElderEmail);

            if (status == -2)
            {
                ViewBag.Error = "Elder already linked to a caretaker";
                return View(model);
            }

            TempData["Success"] = "Elder added successfully";
            return RedirectToAction("Index", "CaretakerHome");
        }

        [HttpGet]
        public IActionResult Profile(int id)
        {
            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (caretakerIdClaim == null)
            {
                return RedirectToAction("Login", "caretakerLogin");
            }

            int caretakerId = int.Parse(caretakerIdClaim);

            var elder = _elderProfile.GetElderProfile(caretakerId, id);

            if (elder == null)
            {
                return Unauthorized();
            }

            var token = _jwtService.GenerateToken(caretakerId, User.FindFirstValue(ClaimTypes.Email) ?? "");

            ViewBag.MicroJwt = token;
            ViewBag.LocationWsBase = _config["LocationService:WsBase"];

            return View(elder);
        }

        
        //[HttpGet]
        //public IActionResult Profile(int id)
        //{
        //   //int? caretakerId = HttpContext.Session.GetInt32("CareTakerID");
        //
        //    var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (caretakerIdClaim == null)
        //    {
        //        return RedirectToAction("Login", "caretakerLogin");
        //    }

        //    int caretakerId = int.Parse(caretakerIdClaim);


        //    //if (caretakerId == null)
        //    //{
        //    //    return RedirectToAction("Login", "caretakerLogin");
        //    //}

        //    //var elder = _elderProfile.GetElderProfile(caretakerId.Value, id);
        //    var elder = _elderProfile.GetElderProfile(caretakerId, id);

        //    if (elder == null)
        //    {
        //        return Unauthorized();
        //    }

        //    return View(elder);
        //}
    }
}
