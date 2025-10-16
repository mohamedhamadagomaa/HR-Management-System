using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

namespace HR_Management_System.Controllers
{
    [Authorize(Roles = "Admin,HR")]
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            this._auditService = auditService;
        }
        public async Task<IActionResult> Index(string entityType = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Default to last 30 days
            fromDate ??= DateTime.Now.AddDays(-30);
            toDate ??= DateTime.Now;
            var auditLogs = await _auditService.GetAuditLogsAsync(fromDate, toDate, entityType);

            ViewBag.EntityType = entityType;
            ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EntityTypes = new List<string> { "Employee", "LeaveRequest", "Payroll" };

            return View(auditLogs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var auditLogs = await _auditService.GetAuditLogsAsync();
            var log = auditLogs.FirstOrDefault(l => l.Id == id);

            if (log == null) return NotFound();

            return View(log);
        }
    }
}
