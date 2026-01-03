using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class caretakerRegister
    {
        [Required(ErrorMessage = "First name is Required."), StringLength(50)]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last name is Required."), StringLength(50)]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is Required."), EmailAddress(ErrorMessage ="Invalid Email.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone number is Required."), Phone(ErrorMessage ="Enter a valid phone number"),MinLength(10, ErrorMessage = "Enter a valid phone number"),MaxLength(10)]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Please enter your age."), Range(18, 100,ErrorMessage ="Age should be between 18 and 100.")]
        public int Age { get; set; }
        [Required(ErrorMessage = "Set a password."), MinLength(8, ErrorMessage ="Passwords must be at least 8 characters.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm your password."), Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string PasswordConfirm { get; set; }
        public string? Photo { get; set; }
        [StringLength(256)]
        public string? HomeAddress { get; set; }
        [Required(ErrorMessage = "Please select your gender.")]
        public string Gender { get; set; }
        [StringLength(500)]
        public string? Bio { get; set; }
    }
}
