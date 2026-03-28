using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Security.Claims;
using wellcare.Models;
using wellcare.Services;

namespace wellcare.Controllers
{
    [ApiController]
    [Route("api/elder")]
    public class ElderAPIController : ControllerBase
    {
        private readonly DBConnect _db;

        public ElderAPIController(DBConnect db)
        {
            _db = db;
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] ElderSIGNUP model)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid input");

            using SqlConnection con = _db.GetConnection();
            SqlCommand cmd = new SqlCommand("select count(1) from elderTable where eldermail = @mail", con);

            cmd.Parameters.AddWithValue("@mail", model.ElderMail);

            con.Open();
            int exists = (int)cmd.ExecuteScalar();

            if (exists > 0)
                return Conflict("Elder already registered");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            SqlCommand icmd = new SqlCommand(@"insert into elderTable(elderName,elderMail,Age,Gender,PasswordHash) values (@name,@mail,@age,@gender,@hash)", con);

            icmd.Parameters.AddWithValue("@name", model.ElderName);
            icmd.Parameters.AddWithValue("@mail", model.ElderMail);
            icmd.Parameters.AddWithValue("@age", model.Age);
            icmd.Parameters.AddWithValue("@gender", model.Gender);
            icmd.Parameters.AddWithValue("@hash", passwordHash);

            icmd.ExecuteNonQuery();

            return Ok("Registered successfully.");
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] ElderLogin model, [FromServices] JwtService jwt)
        {
            using SqlConnection con = _db.GetConnection();
            SqlCommand cmd = new SqlCommand(
                "SELECT ElderId, PasswordHash, elderMail, elderName FROM elderTable WHERE elderMail = @mail", con);
            cmd.Parameters.AddWithValue("@mail", model.ElderMail);
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return Unauthorized("Invalid credentials");

            int elderId = reader.GetInt32(0);
            string hash = reader.GetString(1);
            string email = reader.GetString(2);
            string elderName = reader.GetString(3);

            if (!BCrypt.Net.BCrypt.Verify(model.Password, hash))
                return Unauthorized("Invalid credentials");

            reader.Close();

            if (!string.IsNullOrEmpty(model.FCMToken))
            {
                using SqlCommand updateCmd = new SqlCommand(
                    "UPDATE elderTable SET FCMToken = @token WHERE ElderId = @id", con);
                updateCmd.Parameters.AddWithValue("@token", model.FCMToken);
                updateCmd.Parameters.AddWithValue("@id", elderId);
                updateCmd.ExecuteNonQuery();
            }

            string token = jwt.GenerateElderToken(elderId, email);
            return Ok(new { token, elderId, elderName });
        }

        //[HttpPost("set-home")]
        //[Authorize]
        //public IActionResult SetHome([FromBody] HomeLocationModel model)
        //{
        //    var elderIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    int elderId = int.Parse(elderIdClaim);

        //    using SqlConnection con = _db.GetConnection();
        //    using SqlCommand cmd = new SqlCommand(
        //        "UPDATE elderTable SET HomeLat = @lat, HomeLng = @lng WHERE ElderId = @id", con);
        //    cmd.Parameters.AddWithValue("@lat", model.Lat);
        //    cmd.Parameters.AddWithValue("@lng", model.Lng);
        //    cmd.Parameters.AddWithValue("@id", elderId);
        //    con.Open();
        //    cmd.ExecuteNonQuery();
        //    return Ok("Home location saved.");
        //}
    }
}