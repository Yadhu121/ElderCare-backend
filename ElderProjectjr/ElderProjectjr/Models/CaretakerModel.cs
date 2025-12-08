using System.ComponentModel.DataAnnotations;

namespace ElderProjectjr.Models
{
    public class CaretakerModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public int Age { get; set; }
        [Required]
        public string Password { get; set; }
        [Required, Compare("Password",ErrorMessage ="Passwords do not match.")]
        public string PasswordConfirm { get; set; }
        public string? Photo { get; set; }
        public string? HomeAddress { get; set; }
        [Required]
        public string Gender { get; set; }
        public string? Bio { get; set; }
    }
}
