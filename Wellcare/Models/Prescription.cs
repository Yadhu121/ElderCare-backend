using System;

namespace wellcare.Models
{
    public class Prescription
    {
        public int PrescriptionID { get; set; }
        public int ElderID { get; set; }
        public int CaretakerID { get; set; }
        public string MedicineName { get; set; }
        public string Dosage { get; set; }
        public string Frequency { get; set; }
        public DateTime PrescriptionDate { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
    }
}
