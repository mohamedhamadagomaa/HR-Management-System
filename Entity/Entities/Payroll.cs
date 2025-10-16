using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Entities
{
    public class Payroll
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }

        public DateTime PayPeriod { get; set; } // Year-Month
        public decimal BaseSalary { get; set; }
        public virtual ICollection<Allowance> Allowances { get; set; } = new List<Allowance>();
        public virtual ICollection<Deduction> Deductions { get; set; } = new List<Deduction>();
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalAllowances => Allowances?.Sum(a => a.Amount) ?? 0;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal TotalDeductions => Deductions?.Sum(d => d.Amount) ?? 0;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal NetPay => BaseSalary + TotalAllowances - TotalDeductions;

        public DateTime GeneratedAt { get; set; } = DateTime.Now;
        public string GeneratedBy { get; set; }

        // Navigation properties
        public virtual Employee Employee { get; set; }
    }
}
