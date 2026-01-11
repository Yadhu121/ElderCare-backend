//namespace wellcare.Models
//{
//    public class AssignElderModel
//   {
//        public string ElderEmail { get; set; }
//        public string ElderPassword { get; set; }
//    }
//}

using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class AssignElderModel
    {
        [Required(ErrorMessage = "Elder email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string ElderEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Elder password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string ElderPassword { get; set; } = string.Empty;
    }
}
