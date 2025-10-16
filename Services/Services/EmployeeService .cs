using Entity.Data;
using Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class EmployeeService : IEmployeesServices
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            this._context = context;
        }
        public async Task CreateEmployeeAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await GetEmployeeByIdAsync(id);
            if(employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                 .OrderBy(e => e.Name)
                 .ToListAsync();
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees
         .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Employee> GetEmployeeByUserIdAsync(string userId)
        {
            return await _context.Employees
      .FirstOrDefaultAsync(e => e.UserId == userId);
        }

        public async Task<List<Employee>> GetEmployeesByManagerAsync(string managerUserId)
        {
            var manager = await GetEmployeeByUserIdAsync(managerUserId);
            if (manager == null) return new List<Employee>();
            return await GetAllEmployeesAsync();
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            var existingEmployee = await _context.Employees.FindAsync(employee.Id);
            if (existingEmployee != null)
            {
                // Update only the fields that should be editable
                existingEmployee.Name = employee.Name;
                existingEmployee.Department = employee.Department;
                existingEmployee.Position = employee.Position;
                existingEmployee.Salary = employee.Salary;
                existingEmployee.Role = employee.Role;
                existingEmployee.LeaveBalance = employee.LeaveBalance;
                // Note: UserId and HireDate are preserved from the original

                _context.Employees.Update(existingEmployee);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new ArgumentException("Employee not found");
            }
        }
    }
}
