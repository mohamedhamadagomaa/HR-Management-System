namespace HR_Management_System.Models
{
    public class DepartmentLeaveSummary
    {
        public string Department { get; set; }
        public int EmployeeCount { get; set; }
        public int TotalLeaves { get; set; }
        public int ApprovedLeaves { get; set; }
    }
}
