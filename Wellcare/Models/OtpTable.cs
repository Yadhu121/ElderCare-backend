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

        public void InsertOtpForElderLinking(int elderId, string email, string otp)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand(@"
        insert into ElderOTPTable (ElderID, Email, OTP, ExpiresAt, IsUsed, CreatedAt)
        values (@elderId, @email, @otp, DATEADD(MINUTE, 10, SYSUTCDATETIME()), 0, SYSUTCDATETIME())", con);
            cmd.Parameters.AddWithValue("@elderId", elderId);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@otp", otp);
            con.Open();
            cmd.ExecuteNonQuery();
        }

        public int VerifyElderLinkingOtp(string email, string otp)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand(@"
        select ElderID from ElderOTPTable
        where Email = @email and OTP = @otp
        and IsUsed = 0 AND ExpiresAt > SYSUTCDATETIME()", con);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@otp", otp);
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return -1;
            int elderId = Convert.ToInt32(reader["ElderID"]);
            reader.Close();
            con.Close();

            using SqlCommand updateCmd = new SqlCommand(@"
        update ElderOTPTable set IsUsed = 1 
        where Email = @email and OTP = @otp", con);
            updateCmd.Parameters.AddWithValue("@email", email);
            updateCmd.Parameters.AddWithValue("@otp", otp);
            con.Open();
            updateCmd.ExecuteNonQuery();

            return elderId;
        }
    }
}