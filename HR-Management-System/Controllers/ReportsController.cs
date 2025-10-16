using Entity.Entities;
using HR_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Services;

namespace HR_Management_System.Controllers
{
    [Authorize(Roles ="Admin,HR,Manager")]
    public class ReportsController : Controller
    {
        private readonly IPayrollService _payrollService;
        private readonly IEmployeesServices _employeesServices;
        private readonly ILeaveService _leaveService;

        public ReportsController(IPayrollService payrollService , IEmployeesServices employeesServices , ILeaveService leaveService)
        {
            this._payrollService = payrollService;
            this._employeesServices = employeesServices;
            this._leaveService = leaveService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> LeaveSummary()
        {
            var leaveRequests = await _leaveService.GetAllLeaveRequestsAsync();
            var employees = await _employeesServices.GetAllEmployeesAsync();
            var model = new LeaveSummaryReport
            {
                Employees = employees,
                TotalLeaveRequests = leaveRequests.Count,
                ApprovedLeaves = leaveRequests.Count(lr => lr.Status == "Approved"),
                RejectedLeaves = leaveRequests.Count(lr => lr.Status == "Rejected"),
                PendingLeaves = leaveRequests.Count(lr => lr.Status == "Pending")

            };
            // calculate Leaves By type
            model.LeaveByType = leaveRequests.GroupBy(lr => lr.LeaveType)
                .Select(g => new LeaveTypeSummary
                {
                    Type = g.Key,
                    Count = g.Count(),
                    Percentage = (decimal)g.Count() / leaveRequests.Count * 100
                }).ToList();

            // calculate Leaves by Department
            model.LeaveByDepartment = employees.GroupBy(e => e.Department).Select(g => new DepartmentLeaveSummary
            {
                Department = g.Key,
                EmployeeCount = g.Count(),
                TotalLeaves = leaveRequests.Count(lr => g.Any(e => e.Id == lr.EmployeeId)),
                ApprovedLeaves = leaveRequests.Count(lr => lr.Status == "Approved" && g.Any(e => e.Id == lr.EmployeeId)),
            }).ToList();
            return View(model);
        }

        public async Task<IActionResult> PayrollSummary()
        {
            var currentPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var payrolls = await _payrollService.GetPayrollsByPeriodAsync(currentPeriod);
            var employees = await _employeesServices.GetAllEmployeesAsync();

            var model = new PayrollSummaryReport
            {
                PayPeriod = currentPeriod,
                TotalEmployees = employees.Count,
                ProcessedEmployees = payrolls.Count,
                TotalBaseSalary = payrolls.Sum(p => p.BaseSalary),
                TotalAllowances = payrolls.Sum(p => p.TotalAllowances),
                TotalDeductions = payrolls.Sum(p => p.TotalDeductions),
                TotalNetPay = payrolls.Sum(p => p.NetPay),
            };

            //Payroll By Department
            model.PayrollByDepartment = employees
                .GroupBy(e => e.Department)
                .Select(g => new DepartmentPayrollSummary
                {
                    Department = g.Key,
                    EmployeeCount = g.Count(),
                    TotalSalary = g.Sum(e => e.Salary),
                    ProcessedCount = payrolls.Count(p => g.Any(e => e.Id == p.EmployeeId))
                }).ToList();

            return View(model);

        }

        public async Task<IActionResult> EmployeeAttendance()
        {
            var employee = await _employeesServices.GetAllEmployeesAsync();
            var leavRequests = await _leaveService.GetAllLeaveRequestsAsync();
            var model = employee.Select(e => new EmployeeAttendanceSummary
            {
                Employee = e,
                TotalLeaves = leavRequests.Count(lr => lr.EmployeeId == e.Id && lr.Status == "Approved"),
                PendingLeaves = leavRequests.Count(lr => lr.EmployeeId == e.Id && lr.Status == "Pending"),
                LeaveBalance = e.LeaveBalance


            }).ToList();
            return View(model);

        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string chartType)
        {
            try
            {
                object chartDate = chartType switch
                {
                    "leaveByType" => await GetLeaveByTypeData(),
                    "leaveByDepartment" => await GetLeaveByDepartmentData(),
                    "payrollByDepartment" => await GetPayrollByDepartmentData(),
                    "attendanceTrend" => await GetAttendanceTrendData(),
                    _ => null
                };
                return Json(new { success = true, data = chartDate });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<object> GetAttendanceTrendData()
        {
            // Simplified - in real app, you'd get data for multiple months
            var leaveRequests = await _leaveService.GetAllLeaveRequestsAsync();
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);

            var monthlyData = Enumerable.Range(0, 6)
                .Select(i => new
                {
                    Month = DateTime.Now.AddMonths(-i).ToString("MMM yyyy"),
                    LeaveCount = leaveRequests.Count(lr => lr.StartDate.Month == DateTime.Now.AddMonths(-i).Month &&
                                                         lr.StartDate.Year == DateTime.Now.AddMonths(-i).Year &&
                                                         lr.Status == "Approved")
                })
                .Reverse()
                .ToList();

            return new
            {
                labels = monthlyData.Select(m => m.Month).ToArray(),
                data = monthlyData.Select(m => m.LeaveCount).ToArray()
            };
        }

        private async Task<object> GetPayrollByDepartmentData()
        {
            var employees = await _employeesServices.GetAllEmployeesAsync();
            var currentPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var payrolls = await _payrollService.GetPayrollsByPeriodAsync(currentPeriod);

            var grouped = employees
                .GroupBy(e => e.Department)
                .Select(g => new
                {
                    Department = g.Key,
                    TotalSalary = payrolls.Where(p => g.Any(e => e.Id == p.EmployeeId)).Sum(p => p.NetPay)
                })
                .ToList();

            return new
            {
                labels = grouped.Select(g => g.Department).ToArray(),
                data = grouped.Select(g => (double)g.TotalSalary).ToArray()
            };
        }

        private async Task<object> GetLeaveByDepartmentData()
        {
            var employees = await _employeesServices.GetAllEmployeesAsync();
            var leaveRequests = await _leaveService.GetAllLeaveRequestsAsync();

            var grouped = employees
                .GroupBy(e => e.Department)
                .Select(g => new
                {
                    Department = g.Key,
                    LeaveCount = leaveRequests.Count(lr => g.Any(e => e.Id == lr.EmployeeId && lr.Status == "Approved"))
                })
                .ToList();

            return new
            {
                labels = grouped.Select(g => g.Department).ToArray(),
                data = grouped.Select(g => g.LeaveCount).ToArray()
            };
        }

        private async Task<object> GetLeaveByTypeData()
        {
            var leaveRequests = await _leaveService.GetAllLeaveRequestsAsync();
            var grouped = leaveRequests.GroupBy(lr => lr.LeaveType).Select(g => new
            {
                Type = g.Key,
                Count = g.Count()
            }).ToList();

            return new
            {
                labels = grouped.Select(g => g.Type).ToArray(),
                data = grouped.Select(g => g.Count).ToArray(),
                backgroundColor = new[] { "#36a2eb", "#4bc0c0", "#ffcd56", "#ff6384", "#9966ff" }
            };
        }



        // Add to ReportsController class

        // GET: Reports/GetDashboardStats
        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var employees = await _employeesServices.GetAllEmployeesAsync();
                var leaveRequests = await _leaveService.GetAllLeaveRequestsAsync();
                var currentPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var payrolls = await _payrollService.GetPayrollsByPeriodAsync(currentPeriod);

                var stats = new
                {
                    totalEmployees = employees.Count,
                    activeLeaves = leaveRequests.Count(lr => lr.Status == "Pending"),
                    monthlyPayroll = payrolls.Sum(p => p.NetPay),
                    avgLeaveDays = leaveRequests.Any() ?
                        leaveRequests.Average(lr => (lr.EndDate - lr.StartDate).Days + 1) : 0
                };

                return Json(new { success = true, stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Reports/TopEmployees
        public async Task<IActionResult> TopEmployees()
        {
            var employees = await _employeesServices.GetAllEmployeesAsync();
            var leaveRequests = await _leaveService.GetAllLeaveRequestsAsync();

            var topEmployees = employees
                .Select(e => new
                {
                    Employee = e,
                    TotalLeaves = leaveRequests.Count(lr => lr.EmployeeId == e.Id && lr.Status == "Approved"),
                    LeaveBalance = e.LeaveBalance,
                    PerformanceScore = CalculatePerformanceScore(e, leaveRequests.Where(lr => lr.EmployeeId == e.Id).ToList())
                })
                .OrderByDescending(x => x.PerformanceScore)
                .Take(10)
                .ToList();

            return View(topEmployees);
        }

        private double CalculatePerformanceScore(Employee employee, List<LeaveRequest> employeeLeaves)
        {
            // Simple scoring algorithm - in real app, use more complex criteria
            double score = 100;

            // Deduct for low leave balance (indicates over-use)
            if (employee.LeaveBalance < 5)
                score -= 20;
            else if (employee.LeaveBalance > 25)
                score += 10;

            // Deduct for many rejected leaves
            var rejectedLeaves = employeeLeaves.Count(lr => lr.Status == "Rejected");
            score -= rejectedLeaves * 5;

            // Bonus for good attendance (high balance, approved leaves)
            var approvedLeaves = employeeLeaves.Count(lr => lr.Status == "Approved");
            if (approvedLeaves > 0 && employee.LeaveBalance > 10)
                score += 15;

            return Math.Max(0, score);
        }


    }
}
