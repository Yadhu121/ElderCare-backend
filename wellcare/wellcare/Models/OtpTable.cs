using System.Data;
using System.Data.SqlClient;
using wellcare.Models;

namespace wellcare.Models
{
    public class OtpTable
    {
        private readonly DBConnect _db;

        public OtpTable(DBConnect db)
        {
            _db = db;
        }

        public void InsertOtp(int careTakerID, string email, string otp)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_otp", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CareTakerID", careTakerID);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@OTP", otp);
            cmd.Parameters.AddWithValue("@Purpose", "CaretakerEmailVerification");

            con.Open();
            cmd.ExecuteNonQuery();
        }

        public int VerifyOtp(string email, string otp)
        {
            int status = -1;

            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_otp_verify", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@OTP", otp);

            con.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                status = Convert.ToInt32(reader["Status"]);
            }

            return status;
        }
        public string? ResendOtp(string email)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_otp_resend", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Email", email);

            con.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                int status = Convert.ToInt32(reader["Status"]);
                if (status == 1)
                {
                    return reader["OTP"].ToString();
                }
            }
            return null;
        }
        public void InsertOtpForPasswordReset(string email, string otp)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_otp_password_reset", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@OTP", otp);

            con.Open();
            cmd.ExecuteNonQuery();
        }
        public int VerifyPasswordResetOtp(string email, string otp)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_password_reset_verify", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@OTP", otp);

            con.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return Convert.ToInt32(reader["Status"]);

            return -1;
        }
    }
}
