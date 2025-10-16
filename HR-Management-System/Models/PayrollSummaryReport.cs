namespace HR_Management_System.Models
{
    public class PayrollSummaryReport
    {
        public DateTime PayPeriod { get; set; }
        public int TotalEmployees { get; set; }
        public int ProcessedEmployees { get; set; }
        public decimal TotalBaseSalary { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetPay { get; set; }
        public List<DepartmentPayrollSummary> PayrollByDepartment { get; set; } = new();
    }
}
