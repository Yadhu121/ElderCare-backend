using System;
using System.Data.SqlClient;

namespace wellcare.Models
{
    public class elderProfile
    {
        private readonly DBConnect _db;
        private readonly PrescriptionTable _prescriptionRepo;

        public elderProfile(DBConnect db, PrescriptionTable prescriptionRepo)
        {
            _db = db;
            _prescriptionRepo = prescriptionRepo;
        }

        public ElderProfileViewModel? GetElderProfile(int caretakerId, int elderId)
        {
            using SqlConnection con = _db.GetConnection();
            using SqlCommand cmd = new SqlCommand(@"select e.elderId,e.elderName,e.eldermail,e.Age,e.Gender,m.LinkedAt
                from CaretakerElderMap m inner join elderTable e on e.elderId = m.ElderID
                where m.CareTakerID = @cid and e.elderId = @eid", con);

            cmd.Parameters.AddWithValue("@cid", caretakerId);
            cmd.Parameters.AddWithValue("@eid", elderId);

            con.Open();
            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            var profile = new ElderProfileViewModel
            {
                ElderId = Convert.ToInt32(reader["elderId"]),
                ElderName = reader["elderName"].ToString(),
                Email = reader["eldermail"].ToString(),
                Age = Convert.ToInt32(reader["Age"]),
                Gender = reader["Gender"].ToString(),
                LinkedAt = Convert.ToDateTime(reader["LinkedAt"])
            };

            reader.Close(); 
            profile.Prescriptions = _prescriptionRepo.GetPrescriptionsByElderId(elderId);

            return profile;
        }
    }
}