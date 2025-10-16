using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public class LeaveRequestViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Leave type is required")]
        [Display(Name = "Leave Type")]
        public string LeaveType { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; }
    }


}
