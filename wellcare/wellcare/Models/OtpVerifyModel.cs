using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class OtpVerifyModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(6, MinimumLength = 6)]
        public string OTP { get; set; }
    }
}
