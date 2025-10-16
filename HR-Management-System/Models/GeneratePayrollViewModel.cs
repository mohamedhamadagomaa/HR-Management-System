using System.ComponentModel.DataAnnotations;

namespace HR_Management_System.Models
{
    public class GeneratePayrollViewModel
    {
        [Required(ErrorMessage = "Pay period is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Pay Period")]
        public DateTime PayPeriod { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

        [Display(Name = "Process for all employees")]
        public bool ProcessForAllEmployees { get; set; } = true;

        [Display(Name = "Include overtime")]
        public bool IncludeOvertime { get; set; } = true;

        [Display(Name = "Apply leave deductions")]
        public bool ApplyLeaveDeductions { get; set; } = true;

        [Display(Name = "Process bonuses")]
        public bool ProcessBonuses { get; set; } = true;

        [Display(Name = "Send notifications")]
        public bool SendNotifications { get; set; } = true;
    }
}
