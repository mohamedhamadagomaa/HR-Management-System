namespace HR_Management_System.Models
{
    public class DepartmentPayrollSummary
    {
        public string Department { get; set; }
        public int EmployeeCount { get; set; }
        public int ProcessedCount { get; set; }
        public decimal TotalSalary { get; set; }
    }
}
