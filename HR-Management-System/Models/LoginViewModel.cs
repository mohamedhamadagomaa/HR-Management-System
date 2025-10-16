using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "User ID is required")]
        [Display(Name = "User ID")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "User ID must be between 3 and 20 characters")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
