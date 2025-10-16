using Entity.Entities;
using HR_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Services;
using System.Globalization;

namespace HR_Management_System.Controllers
{
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly IPayrollService _payrollService;
        private readonly IEmployeesServices _employeesServices;

        public PayrollController(IPayrollService payrollService , IEmployeesServices employeesServices)
        {
            this._payrollService = payrollService;
            this._employeesServices = employeesServices;
        }
        public async Task<IActionResult> Index()
        {
            var currentUser = await _employeesServices.GetEmployeeByUserIdAsync(User.Identity.Name);
            if(currentUser == null) return RedirectToAction("Login", "Account");
            if(currentUser.Role == "Admin" | currentUser.Role == "HR")
            {
                var currentPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var payrolls = await _payrollService.GetPayrollsByPeriodAsync(currentPeriod);
                ViewBag.SelectedPeriod = currentPeriod.ToString("yyyy-mm");
                return View("ManagePayroll", payrolls);
            }
            else
            {
                var myPayrolls = await _payrollService.GetPayrollsByEmployeeAsync(currentUser.Id);
                return View("MyPayslips", myPayrolls);
            }
               
        }

        public async Task<IActionResult> MyPayslips()
        {
            var cueentUser = await _employeesServices.GetEmployeeByUserIdAsync(User.Identity.Name);
            if(cueentUser == null) return RedirectToAction("Login", "Account");
            var myPayrolls = await _payrollService.GetPayrollsByEmployeeAsync(cueentUser.Id);
            return View(myPayrolls);

        }
       
        [Authorize(Roles ="Admin,HR")]
        public async Task<IActionResult> Generate()
        {
            var model = new GeneratePayrollViewModel
            {
                PayPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
            };
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Generate(GeneratePayrollViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var employees = await _employeesServices.GetAllEmployeesAsync();
                    var generatedPayrolls = new List<Payroll>();
                    foreach (var employee in employees)
                    {
                        var payroll = await _payrollService.GeneratePayrollAsync(employee.Id, model.PayPeriod);
                        generatedPayrolls.Add(payroll);
                    }
                    TempData["SuccessMessage"] = $"Payroll generated for {generatedPayrolls.Count} employees for period {model.PayPeriod:MMMM yyyy}";
                    return RedirectToAction("Manage");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error generating payroll: {ex.Message}");
                }
            }
            return View(model);
        }

        [Authorize(Roles ="Admin,HR")]
        public async Task<IActionResult> Manage(string period = null)
        {
            DateTime selectedPeriod;
            if (string.IsNullOrEmpty(period)){
                selectedPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            else
            {
                selectedPeriod = DateTime.ParseExact(period, "yyyy-MM", CultureInfo.InvariantCulture);
            }
            var payrolls = await _payrollService.GetPayrollsByPeriodAsync(selectedPeriod);
            ViewBag.SelectedPeriod = selectedPeriod.ToString("yyyy-MM");
            ViewBag.PreviousPeriod = selectedPeriod.AddMonths(-1).ToString("yyyy-MM");
            ViewBag.NextPeriod = selectedPeriod.AddMonths(1).ToString("yyyy-MM");
            return View(payrolls);
        }

        public async Task<IActionResult> Details(int id)
        {
            var payroll = await _payrollService.GetPayrollByIdAsync(id);
            if (payroll == null) return NotFound();
            var currentUser = await _employeesServices.GetEmployeeByUserIdAsync(User.Identity.Name);
            if(currentUser.Role != "Admin" && currentUser.Role != "HR" && payroll.EmployeeId != id)
            {
                return Forbid();
            }
            return View(payroll);
        }

        // Get : Payroll/Exportpdf/{id}
        public async Task<IActionResult> Exportpdf(int id)
        {
            var payroll = await _payrollService.GetPayrollByIdAsync(id);
            if (payroll == null) return NotFound();
            return View("Payslippdf", payroll);
        }
        // AJAX: Payroll/GetPayrollSummary
        [HttpGet]
        public async Task<IActionResult> GetPayrollSummary(string period)
        {
            try
            {
                DateTime selectedPeriod = DateTime.ParseExact(period, "yyyy-MM", CultureInfo.InvariantCulture);
                var payrolls = await _payrollService.GetPayrollsByPeriodAsync(selectedPeriod);

                var summary = new
                {
                    TotalEmployees = payrolls.Count,
                    TotalBasicSalary = payrolls.Sum(p => p.BaseSalary),
                    TotalAllowances = payrolls.Sum(p => p.TotalAllowances),
                    TotalDeductions = payrolls.Sum(p => p.TotalDeductions),
                    TotalNetPay = payrolls.Sum(p => p.NetPay)
                };
                return Json(new { success = true, summary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



    }
}
