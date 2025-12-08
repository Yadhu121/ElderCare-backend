using System.ComponentModel.DataAnnotations;

namespace ElderProjectjr.Models
{
    public class OTPVerifyModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 4)]
        public string OTP { get; set; }
    }
}
