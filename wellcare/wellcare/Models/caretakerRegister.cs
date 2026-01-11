//using System.ComponentModel.DataAnnotations;

//namespace wellcare.Models
//{
//    public class caretakerRegister
//   {
//        [Required(ErrorMessage = "First name is Required."), StringLength(50)]
//        public string FirstName { get; set; }
//        [Required(ErrorMessage = "Last name is Required."), StringLength(50)]
//        public string LastName { get; set; }
//        [Required(ErrorMessage = "Email is Required."), EmailAddress(ErrorMessage ="Invalid Email.")]
//        public string Email { get; set; }
//       [Required(ErrorMessage = "Phone number is Required."), Phone(ErrorMessage ="Enter a valid phone number"),MinLength(10, ErrorMessage = "Enter a valid phone number"),MaxLength(10)]
//        public string Phone { get; set; }
//        [Required(ErrorMessage = "Please enter your age."), Range(18, 100,ErrorMessage ="Age should be between 18 and 100.")]
//        public int Age { get; set; }
//        [Required(ErrorMessage = "Set a password."), MinLength(8, ErrorMessage ="Passwords must be at least 8 characters.")]
//        public string Password { get; set; }
//        [Required(ErrorMessage = "Confirm your password."), Compare("Password", ErrorMessage = "Passwords do not match.")]
//        public string PasswordConfirm { get; set; }
//        public string? Photo { get; set; }
//        [StringLength(256)]
//        public string? HomeAddress { get; set; }
//        [Required(ErrorMessage = "Please select your gender.")]
//        public string Gender { get; set; }
//        [StringLength(500)]
//        public string? Bio { get; set; }
//    }
//}

using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class caretakerRegister
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Enter a valid phone number")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be 10 digits")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required")]
        [Range(18, 100, ErrorMessage = "Age must be between 18 and 100")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string PasswordConfirm { get; set; } = string.Empty;
        public string? Photo { get; set; }

        [StringLength(256, ErrorMessage = "Address too long")]
        public string? HomeAddress { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Bio too long")]
        public string? Bio { get; set; }
    }
}

