using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace wellcare.Models
{
    public class PrescriptionTable
    {
        private readonly DBConnect _db;

        public PrescriptionTable(DBConnect db)
        {
            _db = db;
        }

        public List<Prescription> GetPrescriptionsByElderId(int elderId)
        {
            var prescriptions = new List<Prescription>();
            using (SqlConnection con = _db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_prescription_get_by_elder", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ElderID", elderId);
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        prescriptions.Add(MapReaderToPrescription(reader));
                    }
                }
            }
            return prescriptions;
        }

        public int AddPrescription(Prescription prescription)
        {
            using (SqlConnection con = _db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_prescription_add", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ElderID", prescription.ElderID);
                cmd.Parameters.AddWithValue("@CaretakerID", prescription.CaretakerID);
                cmd.Parameters.AddWithValue("@MedicineName", prescription.MedicineName);
                cmd.Parameters.AddWithValue("@Dosage", prescription.Dosage);
                cmd.Parameters.AddWithValue("@Frequency", prescription.Frequency);
                cmd.Parameters.AddWithValue("@Notes", (object?)prescription.Notes ?? DBNull.Value);

                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Convert.ToInt32(reader["PrescriptionID"]);
                    }
                }
            }
            return 0;
        }

        public bool UpdatePrescription(Prescription prescription)
        {
            using (SqlConnection con = _db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_prescription_update", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PrescriptionID", prescription.PrescriptionID);
                cmd.Parameters.AddWithValue("@MedicineName", prescription.MedicineName);
                cmd.Parameters.AddWithValue("@Dosage", prescription.Dosage);
                cmd.Parameters.AddWithValue("@Frequency", prescription.Frequency);
                cmd.Parameters.AddWithValue("@Notes", (object?)prescription.Notes ?? DBNull.Value);

                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Convert.ToInt32(reader["Status"]) == 1;
                    }
                }
            }
            return false;
        }

        public bool DeletePrescription(int prescriptionId)
        {
            using (SqlConnection con = _db.GetConnection())
            using (SqlCommand cmd = new SqlCommand("sp_prescription_delete", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PrescriptionID", prescriptionId);

                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Convert.ToInt32(reader["Status"]) == 1;
                    }
                }
            }
            return false;
        }

        private Prescription MapReaderToPrescription(SqlDataReader reader)
        {
            return new Prescription
            {
                PrescriptionID = Convert.ToInt32(reader["PrescriptionID"]),
                ElderID = Convert.ToInt32(reader["ElderID"]),
                CaretakerID = Convert.ToInt32(reader["CaretakerID"]),
                MedicineName = reader["MedicineName"].ToString(),
                Dosage = reader["Dosage"].ToString(),
                Frequency = reader["Frequency"].ToString(),
                PrescriptionDate = Convert.ToDateTime(reader["PrescriptionDate"]),
                Notes = reader["Notes"]?.ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            };
        }
    }
}
