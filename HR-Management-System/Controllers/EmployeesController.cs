using Entity.Entities;
using LeavePayrollSystem.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Services.Services;

namespace HR_Management_System.Controllers
{
        //[Authorize(Roles ="Admin,HR")]
    public class EmployeesController : BaseController
    {
        private readonly IEmployeesServices employeeService;
    

        public EmployeesController(IEmployeesServices employeeService ) : base(employeeService)
        {
            employeeService = employeeService;
            this.employeeService = employeeService;
        }

        public async Task<IActionResult> Index()
        {
            var currentEmployee = await GetCurrentEmployeeAsync();
            SetEmployeeViewData(currentEmployee);

            try
            {
                var employees = await employeeService.GetAllEmployeesAsync();

                if (employees == null)
                {
                    employees = new List<Employee>();
                }

                return View(employees);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employees: {ex.Message}");
                return View(new List<Employee>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            Console.WriteLine($"Create method called. ModelState.IsValid: {ModelState.IsValid}");

            if (ModelState.IsValid)
            {
                try
                {
                    var currentEmployee = await GetCurrentEmployeeAsync();
                    // Debug: Log the employee data
                    Console.WriteLine($"Employee data - Name: {employee.Name}, Department: {employee.Department}, Position: {employee.Position}");

                    // Ensure all required fields have values
                    employee.HireDate = employee.HireDate == DateTime.MinValue ? DateTime.Now : employee.HireDate;
                    employee.LeaveBalance = employee.LeaveBalance <= 0 ? 21 : employee.LeaveBalance;
                  
                    employee.Role = string.IsNullOrEmpty(employee.Role) ? "Employee" : employee.Role;
                    employee.UserId = string.IsNullOrEmpty(employee.UserId) ? "temp-" + Guid.NewGuid().ToString() : employee.UserId;

                    // Set a default name if empty
                    if (string.IsNullOrEmpty(employee.Name))
                    {
                        employee.Name = "New Employee";
                    }

                    await employeeService.CreateEmployeeAsync(employee);
                    Console.WriteLine("Employee created successfully");

                    TempData["SuccessMessage"] = "Employee created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating employee: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");

                    ModelState.AddModelError("", $"Error creating employee: {ex.Message}");
                }
            }
            else
            {
                // Log validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
            }

            // If we got here, something went wrong - return to the form
            return View(employee);
        }


        // Simple test action
        public IActionResult CreateSimple()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSimple(string name, string department)
        {
            try
            {
                var employee = new Employee
                {
                    Name = name ?? "Test Employee",
                    Department = department ?? "IT",
                    Position = "Developer",
                    Salary = 50000,
                    Role = "Employee",
                    LeaveBalance = 21,
                    HireDate = DateTime.Now,
                    UserId = "test-" + Guid.NewGuid().ToString()
                };

                await employeeService.CreateEmployeeAsync(employee);
                TempData["SuccessMessage"] = "Test employee created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var employee = await employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Employee not found.";
                    return RedirectToAction(nameof(Index));
                }

                var currentEmployee = await GetCurrentEmployeeAsync();
                SetEmployeeViewData(currentEmployee);

                return View(employee);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading employee: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                TempData["ErrorMessage"] = "Employee ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing employee to preserve some fields
                    var existingEmployee = await employeeService.GetEmployeeByIdAsync(id);
                    if (existingEmployee == null)
                    {
                        TempData["ErrorMessage"] = "Employee not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Preserve the UserId and other fields that shouldn't be changed
                    employee.UserId = existingEmployee.UserId;
                    employee.HireDate = existingEmployee.HireDate;

                    await employeeService.UpdateEmployeeAsync(employee);
                    TempData["SuccessMessage"] = "Employee updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating employee: {ex.Message}");
                    TempData["ErrorMessage"] = $"Error updating employee: {ex.Message}";
                }
            }

            // If we got here, something went wrong
            var currentEmployee = await GetCurrentEmployeeAsync();
            SetEmployeeViewData(currentEmployee);
            return View(employee);
        }
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var employee = await employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }
                return View(employee);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employee for delete: {ex.Message}");
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await employeeService.DeleteEmployeeAsync(id);
                TempData["SuccessMessage"] = "Employee deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting employee: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var employee = await employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }
                return View(employee);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employee details: {ex.Message}");
                TempData["ErrorMessage"] = "Employee not found.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

