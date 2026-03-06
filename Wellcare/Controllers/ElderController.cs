using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
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

        public ElderController(
            elderTable elderRepo,
            CaretakerElderService linkService,
            elderProfile elderProfile)
        {
            _elderRepo = elderRepo;
            _linkService = linkService;
            _elderProfile = elderProfile;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendOtp(string elderEmail, [FromServices] EmailService emailService, [FromServices] OtpTable otpTable)
        {
            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (caretakerIdClaim == null)
                return RedirectToAction("Login", "caretakerLogin");

            var elder = _elderRepo.GetElderByEmail(elderEmail);
            if (elder == null)
            {
                TempData["Error"] = "Elder not found with this email";
                return RedirectToAction("Add");
            }

            string otp = new Random().Next(100000, 999999).ToString();
            otpTable.InsertOtpForElderLinking(elder.Value.ElderID, elderEmail, otp);
            await emailService.SendOtpEmailAsync(elderEmail, otp);

            TempData["ElderEmail"] = elderEmail;
            TempData["OtpSent"] = "true";
            return RedirectToAction("Add");
        }

        [HttpPost]
        public IActionResult Add(AssignElderModel model, [FromServices] OtpTable otpTable)
        {
            if (!ModelState.IsValid)
                return View(model);

            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (caretakerIdClaim == null)
                return RedirectToAction("Login", "caretakerLogin");

            int caretakerId = int.Parse(caretakerIdClaim);

            int elderId = otpTable.VerifyElderLinkingOtp(model.ElderEmail, model.OTP);
            if (elderId == -1)
            {
                ViewBag.Error = "Invalid or expired OTP";
                return View(model);
            }

            int status = _linkService.AssignElderById(caretakerId, elderId);
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