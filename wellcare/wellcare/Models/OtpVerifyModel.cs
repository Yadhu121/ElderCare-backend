//using System.ComponentModel.DataAnnotations;

//namespace wellcare.Models
//{
//    public class OtpVerifyModel
//    {
//        [Required, EmailAddress]
//        public string Email { get; set; }

//        [Required, StringLength(6, MinimumLength = 6)]
//        public string OTP { get; set; }
//    }
//}

using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class OtpVerifyModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only numbers")]
        public string OTP { get; set; } = string.Empty;
    }
}

