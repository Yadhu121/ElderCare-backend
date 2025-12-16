using BCrypt.Net;
using ElderProjectjr.Models;
using System.Data;
using System.Data.SqlClient;

namespace ElderProjectjr.Models
{
    public class CareTaker
    {
        private readonly DBConnect _db;

        public CareTaker(DBConnect db)
        {
            _db = db;
        }

        public (int Status, int CaretakerID) RegisterCaretaker(CaretakerModel model, string passwordHash)
        {
            int status = 0;
            int careTakerID = 0;
            using (SqlConnection con = _db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_caretaker_register", con))
            {

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FirstName", model.FirstName);
                cmd.Parameters.AddWithValue("@LastName", model.LastName);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@Phone", model.Phone);
                cmd.Parameters.AddWithValue("@Age", model.Age);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.AddWithValue("@HomeAddress", (object?)model.HomeAddress ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", model.Gender);

                con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        status = Convert.ToInt32(reader["Status"]);

                        if (status == 1)
                        {
                            careTakerID = Convert.ToInt32(reader["CareTakerID"]);
                        }
                    }
                }
            }

            return (status, careTakerID);
        }

        public void InsertOTP(string email, string otp)
        {
            using (SqlConnection con = _db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_otp", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@OTP", otp);
                cmd.Parameters.AddWithValue("@Purpose", "CaretakerEmailVerification");

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public int VerifyOtp(string email, string otp)
        {
            int status = 0;

            using (SqlConnection con = _db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_otp_verify", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@OTP", otp);
                cmd.Parameters.AddWithValue("@Purpose", "CaretakerEmailVerification");

                con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        status = Convert.ToInt32(reader["Status"]);
                    }
                }
            }

            return status;
        }
        public (int Status, int CareTakerID, string? FirstName) Login(CaretakerModel model)
        {
            int status = 0;
            int careTakerID = 0;
            string? firstName = null;
            string? dbHash = null;
            bool isEmailVerified = false;

            using (SqlConnection con = _db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_caretaker_login", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@loginInput", model.Email);

                con.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        careTakerID = Convert.ToInt32(reader["CareTakerID"]);
                        firstName = reader["FirstName"].ToString();
                        dbHash = reader["PasswordHash"].ToString();
                        isEmailVerified = Convert.ToBoolean(reader["IsEmailVerified"]);
                    }
                    else
                    {
                        return (-1, 0, null);
                    }
                }
                if (!isEmailVerified)
                {
                    return (-2, careTakerID, firstName);
                }

                bool passwordOk = BCrypt.Net.BCrypt.Verify(model.Password, dbHash);

                if (!passwordOk)
                {
                    return (-3, careTakerID, firstName);
                }

                status = 1;
                return (status, careTakerID, firstName);
            }
        }
    }
}
