using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class ElderLogin
    {
        [Required]
        public string ElderMail { get; set; }

        [Required]
        public string Password { get; set; }
        public string FCMToken { get; set; }
    }
}