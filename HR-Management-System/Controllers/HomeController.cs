using LeavePayrollSystem.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;

[Authorize]
public class HomeController : BaseController
{
    private readonly ILeaveService _leaveService;
    private readonly IPayrollService _payrollService;

    public HomeController(IEmployeesServices employeeService,
                        ILeaveService leaveService,
                        IPayrollService payrollService)
        : base(employeeService)
    {
        _leaveService = leaveService;
        _payrollService = payrollService;
    }

    public async Task<IActionResult> Index()
    {
        var currentEmployee = await GetCurrentEmployeeAsync();

        if (currentEmployee != null)
        {
            SetEmployeeViewData(currentEmployee);

            if (currentEmployee.Role == "Admin" || currentEmployee.Role == "HR")
            {
                try
                {
                    var pendingRequests = await _leaveService.GetPendingLeaveRequestsAsync();
                    ViewBag.PendingRequestsCount = pendingRequests.Count;
                }
                catch (Exception ex)
                {
                    ViewBag.PendingRequestsCount = 0;
                    Console.WriteLine($"Error loading pending requests: {ex.Message}");
                }
            }

            return View(currentEmployee);
        }

        // If no employee record found, redirect to access denied
        return RedirectToAction("AccessDenied", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardData()
    {
        try
        {
            var currentEmployee = await GetCurrentEmployeeAsync();

            if (currentEmployee == null)
            {
                return Json(new
                {
                    success = true,
                    leaveBalance = 21,
                    monthlyPay = 3000,
                    myRequests = 0,
                    recentActivityHtml = "<p class='text-muted'>No recent activity</p>"
                });
            }

            var leaveBalance = currentEmployee.LeaveBalance;
            var myRequests = await _leaveService.GetLeaveRequestsByEmployeeIdAsync(currentEmployee.Id);
            
       
            var monthlyPay = currentEmployee.Salary;

            // Build recent activity HTML
            var recentActivityHtml = await BuildRecentActivityHtml(currentEmployee.Id);

            return Json(new
            {
                success = true,
                leaveBalance,
                monthlyPay,
                myRequests = myRequests.Count,
                recentActivityHtml
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = true,
                leaveBalance = 21,
                monthlyPay = 3000,
                myRequests = 0,
                recentActivityHtml = "<p class='text-muted'>Error loading activity</p>"
            });
        }
    }

    private async Task<string> BuildRecentActivityHtml(int employeeId)
    {
        try
        {
            var recentRequests = await _leaveService.GetLeaveRequestsByEmployeeIdAsync(employeeId);
            var recentRequestsList = recentRequests.Take(5).ToList();

            if (!recentRequestsList.Any())
            {
                return "<p class='text-muted'>No recent leave requests</p>";
            }

            var html = "";
            foreach (var request in recentRequestsList)
            {
                var statusBadge = request.Status switch
                {
                    "Approved" => "<span class='badge bg-success'>Approved</span>",
                    "Rejected" => "<span class='badge bg-danger'>Rejected</span>",
                    _ => "<span class='badge bg-warning'>Pending</span>"
                };

                html += $@"
                        <div class='d-flex justify-content-between align-items-center border-bottom pb-2 mb-2'>
                            <div>
                                <strong>{request.LeaveType} Leave</strong><br>
                                <small class='text-muted'>{request.StartDate:MMM dd} - {request.EndDate:MMM dd}</small>
                            </div>
                            <div>
                                {statusBadge}
                            </div>
                        </div>";
            }
            return html;
        }
        catch (Exception ex)
        {
            return "<p class='text-muted'>Error loading activity</p>";
        }
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}