using Entity.Entities;

namespace HR_Management_System.Models
{
    public class EmployeeAttendanceSummary
    {
        public Employee Employee { get; set; }
        public int TotalLeaves { get; set; }
        public int PendingLeaves { get; set; }
        public int LeaveBalance { get; set; }
    }
}
