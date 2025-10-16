using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entity.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; } // Created, Updated, Deleted, Approved, Rejected
        public string EntityType { get; set; } // Employee, LeaveRequest, Payroll
        public int EntityId { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
