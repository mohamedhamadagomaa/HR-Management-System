using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalEmployees { get; set; }
        public int PendingVacRequests { get; set; }
        public int TotalLeaveRequestsThisMonth { get; set; }
        public decimal TotalPayrollThisMonth { get; set; }
    }
}
