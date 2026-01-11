//namespace wellcare.Models
//{
//    public class ElderProfileViewModel
//    {
//        public int ElderId { get; set; }
//        public string ElderName { get; set; }
//        public string Email { get; set; }
//        public int Age { get; set; }
//        public string Gender { get; set; }
//        public DateTime LinkedAt { get; set; }
//    }
//}

using System;
using System.ComponentModel.DataAnnotations;

namespace wellcare.Models
{
    public class ElderProfileViewModel
    {
        [Required]
        public int ElderId { get; set; }

        [Required]
        [StringLength(100)]
        public string ElderName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Range(1, 120)]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        public DateTime LinkedAt { get; set; }
    }
}

