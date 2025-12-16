using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class ForgotPasswordModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string OTP { get; set; }

        [Required, MinLength(8)]
        public string NewPassword { get; set; }

        [Required, Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
