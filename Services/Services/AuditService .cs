using Entity.Data;
using Entity.Entities;

using Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LeavePayrollSystem.Services.Services
{
  
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogActionAsync(string userId, string action, string entityType, int entityId, object oldValues = null, object newValues = null, string ipAddress = null, string userAgent = null)
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues, new JsonSerializerOptions { WriteIndented = true }) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues, new JsonSerializerOptions { WriteIndented = true }) : null,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.Now
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string entityType = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(log => log.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(log => log.Timestamp <= toDate.Value);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(log => log.EntityType == entityType);

            return await query
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
    }
}