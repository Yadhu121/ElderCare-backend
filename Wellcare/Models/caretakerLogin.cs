using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class caretakerLogin
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}