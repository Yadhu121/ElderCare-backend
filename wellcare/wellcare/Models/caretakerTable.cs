using BCrypt.Net;
using System.Data;
using System.Data.SqlClient;
using wellcare.Models;

namespace wellcare.Models
{
    public class caretakerTable
    {
        private readonly DBConnect _db;

        public caretakerTable(DBConnect db)
        {
            _db = db;
        }

        public (int Status, int CaretakerID) RegisterCaretaker(caretakerRegister model, string passwordHash)
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

        public (int Status, int CareTakerID, string? FirstName) LoginCaretaker(caretakerLogin model)
        {
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
                    if (!reader.Read())
                    {
                        return (-1, 0, null);
                    }

                    careTakerID = Convert.ToInt32(reader["CareTakerID"]);
                    firstName = reader["FirstName"].ToString();
                    dbHash = reader["PasswordHash"].ToString();
                    isEmailVerified = Convert.ToBoolean(reader["IsEmailVerified"]);
                }
            }

            if (!isEmailVerified)
                return (-2, careTakerID, firstName);

            bool passwordOk = BCrypt.Net.BCrypt.Verify(model.Password, dbHash);

            if (!passwordOk)
                return (-3, careTakerID, firstName);

            return (1, careTakerID, firstName);
        }
        public void UpdatePassword(string email, string newHash)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("update caretakerTable set PasswordHash=@p where Email=@e", con);

            cmd.Parameters.AddWithValue("@p", newHash);
            cmd.Parameters.AddWithValue("@e", email);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        public CaretakerProfile caretakerProfile(int caretakerId)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand(@"select FirstName,LastName,Email,Phone,Age,Photo,HomeAddress,Gender,Bio from caretakerTable where CareTakerID = @caretakerId and IsEmailVerified = 1", con);

            cmd.Parameters.AddWithValue("@caretakerId", caretakerId);

            con.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new CaretakerProfile
            {
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                Email = reader["Email"].ToString(),
                Phone = reader["Phone"].ToString(),
                Age = Convert.ToInt32(reader["Age"]),
                Photo = reader["Photo"].ToString(),
                HomeAddress = reader["HomeAddress"].ToString(),
                Gender = reader["Gender"].ToString(),
                Bio = reader["Bio"].ToString()
            };
        }

        public void UpdateCaretakerProfile(int caretakerId, string bio, string photoPath)
        {
            using SqlConnection con = _db.GetConnection();

            using SqlCommand cmd = new SqlCommand(
                @"update caretakerTable set Bio = @bio, Photo = isnull(@photo, Photo) where CareTakerID = @id",con);

            cmd.Parameters.AddWithValue("@bio", bio ?? "");
            cmd.Parameters.AddWithValue("@photo", (object?)photoPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", caretakerId);

            con.Open();
            cmd.ExecuteNonQuery();
        }

    }
}
