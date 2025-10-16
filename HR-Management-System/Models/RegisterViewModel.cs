using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public class RegisterViewModel
    {

        [Required(ErrorMessage = "User ID is required")]
        [Display(Name = "User ID")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "User ID must be between 3 and 20 characters")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public string Department { get; set; }

        [Required(ErrorMessage = "Position is required")]
        public string Position { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(1, 1000000, ErrorMessage = "Salary must be greater than 0")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "Employee";

    }
}


