using System.Data.SqlClient;

namespace wellcare.Models
{
    public class elderTable
    {
        private readonly DBConnect _db;

        public elderTable(DBConnect db)
        {
            _db = db;
        }

        public (int ElderID, string PasswordHash)? GetElderByEmail(string email)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("select elderId, PasswordHash from elderTable where eldermail = @email and IsActive = 1",con);

            cmd.Parameters.AddWithValue("@email", email);

            con.Open();
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return (Convert.ToInt32(reader["elderId"]),reader["PasswordHash"].ToString());
            }

            return null;
        }
    }
}
