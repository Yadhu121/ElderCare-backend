using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Security.Claims;
using wellcare.Models;

namespace wellcare.Controllers
{
    public class caretakerController : Controller
    {
        private readonly caretakerTable _table;

            public caretakerController(caretakerTable table)
        {
            _table = table;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult careProfile()
        {
            var caretakerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (caretakerIdClaim == null)
                return RedirectToAction("Login", "caretakerLogin");

            int caretakerId = int.Parse(caretakerIdClaim);

            var profile = _table.caretakerProfile(caretakerId);

            if (profile == null)
                return Unauthorized();

            return View(profile);
        }
        [HttpPost]
        public IActionResult careProfile(string Bio, IFormFile Photo)
        {
            int caretakerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            string photoPath = null;

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

            return RedirectToAction("careProfile");
        }

    }
}
