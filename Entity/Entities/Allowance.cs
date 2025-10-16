using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Entities
{
    public class Allowance
    {
        public int Id { get; set; }
        public int PayrollId { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; } // Housing, Transport, Bonus

        public decimal Amount { get; set; }

        public virtual Payroll Payroll { get; set; }
    }
}
