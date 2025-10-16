using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs
{
    public class LeaveRequestDto
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string vacType { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public int RequestedDays => (EndDate - StartDate).Days + 1;
    }
}
