using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class ElderSIGNUP
    {
        [Required]
        public string ElderName { get; set; }

        [Required]
        [EmailAddress]
        public string ElderMail { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
