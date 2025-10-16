using Entity.Entities;

namespace HR_Management_System.Models
{
    public class LeaveSummaryReport
    {
        public int TotalLeaveRequests { get; set; }
        public int ApprovedLeaves { get; set; }
        public int PendingLeaves { get; set; }
        public int RejectedLeaves { get; set; }
        public List<LeaveTypeSummary> LeaveByType { get; set; } = new();
        public List<DepartmentLeaveSummary> LeaveByDepartment { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();
    }
}
