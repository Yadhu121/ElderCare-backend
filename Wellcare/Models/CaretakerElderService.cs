using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace wellcare.Models
{
    public class CaretakerElderService
    {
        private readonly DBConnect _db;

        public CaretakerElderService(DBConnect db)
        {
            _db = db;
        }

        public int AssignElder(int caretakerId, string elderEmail)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_assign_elder_to_caretaker", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CareTakerID", caretakerId);
            cmd.Parameters.AddWithValue("@ElderEmail", elderEmail);

            con.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return Convert.ToInt32(reader["Status"]);
            }

            return -99;
        }
        public List<(int ElderID, string ElderName)> GetAssignedElders(int caretakerId)
        {
            var elders = new List<(int, string)>();

            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand(@"select e.elderId, e.elderName from CaretakerElderMap m inner join elderTable e on 
            m.ElderID = e.elderId where m.CareTakerID = @caretakerId", con);

            cmd.Parameters.AddWithValue("@caretakerId", caretakerId);

            con.Open();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                elders.Add((Convert.ToInt32(reader["elderId"]), reader["elderName"].ToString()));
            }

            return elders;
        }

        public int AssignElderById(int caretakerId, int elderId)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand(@"
        if exists (select 1 from CaretakerElderMap where ElderID = @elderId)
            select -2 AS Status
        else
        begin
            insert into CaretakerElderMap (CareTakerID, ElderID) values (@caretakerId, @elderId)
            select 1 AS Status
        end", con);
            cmd.Parameters.AddWithValue("@caretakerId", caretakerId);
            cmd.Parameters.AddWithValue("@elderId", elderId);
            con.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return Convert.ToInt32(reader["Status"]);
            return -99;
        }

        public int UnassignElder(int caretakerId, int elderId)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand("sp_unassign_elder_from_caretaker", con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@CareTakerID", caretakerId);
            cmd.Parameters.AddWithValue("@ElderID", elderId);

            con.Open();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return Convert.ToInt32(reader["Status"]);
            }

            return -99;
        }
    }
}