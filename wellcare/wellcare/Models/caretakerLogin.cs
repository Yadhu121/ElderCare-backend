//using System.ComponentModel.DataAnnotations;

//namespace wellcare.Models
//{
//public class caretakerLogin
//{
//      [Required, EmailAddress]
//       public string Email { get; set; }
//        [Required, MinLength(8)]
//        public string Password { get; set; }
//    }
//}

using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class caretakerLogin
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;
    }
}

