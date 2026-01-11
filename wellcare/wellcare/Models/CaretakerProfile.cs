//namespace wellcare.Models
//{
//    public class CaretakerProfile
//    {
//        public string FirstName { get; set; }
//        public string LastName { get; set; }
//        public string Email { get; set; }
//        public string Phone { get; set; }
//        public int Age { get; set; }
//        public string Photo { get; set; }
//        public string HomeAddress { get; set; }
//        public string Gender { get; set; }
//        public string Bio { get; set; }
//    }
//}

using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class CaretakerProfile
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
        public int Age { get; set; }

        // URL or relative path to photo
        public string? Photo { get; set; }

        public string? HomeAddress { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Bio { get; set; }
    }
}

