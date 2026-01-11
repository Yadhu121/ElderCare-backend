//using System.ComponentModel.DataAnnotations;

//namespace wellcare.Models
//{
//    public class ForgotPasswordModel
//   {
//        [Required, EmailAddress]
//        public string Email { get; set; }
//    }

//    public class ResetPasswordModel
//    {
//        [Required, EmailAddress]
//        public string Email { get; set; }

//        [Required]
//        public string OTP { get; set; }

//        [Required, MinLength(8)]
//        public string NewPassword { get; set; }

//        [Required, Compare("NewPassword")]
//        public string ConfirmPassword { get; set; }
//    }
//}

using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    // ================= FORGOT PASSWORD =================

    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }

    // ================= RESET PASSWORD =================

    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only numbers")]
        public string OTP { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
