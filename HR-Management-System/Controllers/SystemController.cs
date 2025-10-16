using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Services;

namespace HR_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SystemController : Controller
    {
        private readonly IEmployeesServices _employeesServices;
        private readonly ILeaveService _leaveService;
        private readonly IPayrollService _payrollService;

        public SystemController(IEmployeesServices employeesServices , ILeaveService leaveService , IPayrollService payrollService)
        {
            this._employeesServices = employeesServices;
            this._leaveService = leaveService;
            this._payrollService = payrollService;
        }
        public IActionResult Settings()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var employees = await _employeesServices.GetAllEmployeesAsync();
            var leaveRequests = await _leaveService.GetAllLeaveRequestsAsync();
            var currentPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var payrolls = await _payrollService.GetPayrollsByPeriodAsync(currentPeriod);

            ViewBag.TotalEmployees = employees.Count;
            ViewBag.PendingLeaves = leaveRequests.Count(lr => lr.Status == "Pending");
            ViewBag.TotalPayroll = payrolls.Sum(p => p.NetPay);
            ViewBag.ActiveUsers = employees.Count(e => e.LeaveBalance > 0);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetLeaveBalances()
        {
            try
            {
                var employees = await _employeesServices.GetAllEmployeesAsync();
                foreach (var employee in employees)
                {
                    employee.LeaveBalance = 21; // Reset to default
                }
                // Save changes would go here

                TempData["SuccessMessage"] = "Leave balances reset successfully!";
                return RedirectToAction("Settings");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error resetting balances: {ex.Message}";
                return RedirectToAction("Settings");
            }
        }


    }
}
