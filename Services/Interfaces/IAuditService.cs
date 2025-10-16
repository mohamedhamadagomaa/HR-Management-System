using Entity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IAuditService
    {
        Task LogActionAsync(string userId, string action, string entityType, int entityId, object oldValues = null, object newValues = null, string ipAddress = null, string userAgent = null);
        Task<List<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string entityType = null);
    }
}
