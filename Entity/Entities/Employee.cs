using System.ComponentModel.DataAnnotations;

namespace Entity.Entities
{
    public class Employee
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } // Link to Identity User
        [Required, MaxLength(100)]
        public string Name { get; set; }
        [Required, MaxLength(50)]
        public string Department { get; set; }
        [Required, MaxLength(50)]
        public string Position { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Salary { get; set; }

        public DateTime HireDate { get; set; } = DateTime.Now;

        [Required, MaxLength(20)]
        public string Role { get; set; } = "Employee";

        public int LeaveBalance { get; set; } = 21;
        public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    }
}
