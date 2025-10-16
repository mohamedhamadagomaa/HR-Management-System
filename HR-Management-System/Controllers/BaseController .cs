using Entity.Entities;

using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;

namespace LeavePayrollSystem.Web.Controllers
{
    public class BaseController : Controller
    {
        private readonly IEmployeesServices _employeesServices;

        public BaseController(IEmployeesServices employeesServices )
        {
            this._employeesServices = employeesServices;
        }

        protected async Task<Employee> GetCurrentEmployeeAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Get the current user's UserId (which is their username in Identity)
                var userId = User.Identity.Name;

                if (!string.IsNullOrEmpty(userId))
                {
                    return await _employeesServices.GetEmployeeByUserIdAsync(userId);
                }
            }
            return null;
        }

        protected void SetEmployeeViewData(Employee employee)
        {
            if (employee != null)
            {
                ViewBag.EmployeeName = employee.Name;
                ViewBag.EmployeeRole = employee.Role;
                ViewBag.EmployeeDepartment = employee.Department;
                ViewBag.EmployeeId = employee.Id;
            }
        }
    }
}