using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using wellcare.Models;

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
            if(!ModelState.IsValid)
                return BadRequest("Invalid input");

            using SqlConnection con = _db.GetConnection();
            SqlCommand cmd = new SqlCommand("select count(1) from elderTable where eldermail = @mail",con);

            cmd.Parameters.AddWithValue("@mail",model.ElderMail);

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
    }
}
