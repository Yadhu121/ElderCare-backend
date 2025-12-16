using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class caretakerRegister
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; }
        [Required, StringLength(50)]
        public string LastName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required, Phone]
        public string Phone { get; set; }
        [Required, Range(18, 100)]
        public int Age { get; set; }
        [Required, MinLength(8)]
        public string Password { get; set; }
        [Required, Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string PasswordConfirm { get; set; }
        public string? Photo { get; set; }
        [StringLength(256)]
        public string? HomeAddress { get; set; }
        [Required]
        public string Gender { get; set; }
        [StringLength(500)]
        public string? Bio { get; set; }
    }
}
