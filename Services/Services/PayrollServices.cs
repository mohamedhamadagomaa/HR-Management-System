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
    public class PayrollServices : IPayrollService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILeaveService _leaveService;

        public PayrollServices(ApplicationDbContext context, ILeaveService leaveService)
        {
            this._context = context;
            this._leaveService = leaveService;
        }
        public async Task<Payroll> GeneratePayrollAsync(int employeeId, DateTime payPeriod)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new Exception("Employee not found");
            // Check if payroll already exists for the period
            var existingPayroll = await _context.Payrolls
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId &&
                p.PayPeriod.Year == payPeriod.Year &&
                p.PayPeriod.Month == payPeriod.Month);
            if (existingPayroll != null)
                return existingPayroll;
            // Calculate allowances and deductions
            var baseSalary = employee.Salary;
            var allowances = await CalculateAllowancesAsync(employeeId, payPeriod);
            var deductions = await CalculateDeductionsAsync(employeeId, payPeriod, baseSalary);

            var payroll = new Payroll
            {
                EmployeeId = employeeId,
                PayPeriod = new DateTime(payPeriod.Year, payPeriod.Month, 1),
                BaseSalary = baseSalary,
                Allowances = allowances,
                Deductions = deductions,
                GeneratedAt = DateTime.Now,
                GeneratedBy = "System" // This should be replaced with the actual user generating the payroll
            };
            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();

            //Add Alowance and Deduction Records
            await AddAllowanceRecordsAsync(payroll.Id, employeeId, payPeriod);
            await AddDeductionRecordsAsync(payroll.Id, employeeId, payPeriod, baseSalary);

            return payroll;

        }

        private async Task AddDeductionRecordsAsync(int id, int employeeId, DateTime payPeriod, decimal baseSalary)
        {
            var deductions = new List<Deduction>();
            var unpaidLeaveDays = await GetUnpaidLeaveDaysAsync(employeeId, payPeriod);
            var dailyRate = baseSalary / 22;
            if (unpaidLeaveDays > 0)
            {
                deductions.Add(new Deduction
                {
                    PayrollId = id,
                    Type = "Unpaid Leave",
                    Amount = unpaidLeaveDays * dailyRate,
                });
            }
            // Tax deduction 15% of base salary
            deductions.Add(new Deduction
            {
                PayrollId = id,
                Type = "Tax",
                Amount = baseSalary * 0.15m,
            });
            _context.Deductions.AddRange(deductions);
            await _context.SaveChangesAsync();
        }

        private async Task AddAllowanceRecordsAsync(int id, int employeeId, DateTime payPeriod)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            var allowance = new List<Allowance>();

            if(employee.Role == "Manager" || employee.Role == "HR" || employee.Role == "Admin")
            {
                allowance.Add(new Allowance
                {
                    PayrollId = id,
                    Type = "Housing",
                    Amount = employee.Salary * 0.10m,

                });
            }
            allowance.Add(new Allowance
            {
                PayrollId = id,
                Type = "Transport",
                Amount = 200,
            });

            if(employee.Department == "IT")
            {
                allowance.Add(new Allowance
                {
                    PayrollId = id,
                    Type = "Department Bonus",
                    Amount = 150,
                });

                _context.Allowances.AddRange(allowance);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<List<Deduction>> CalculateDeductionsAsync(int employeeId, DateTime payPeriod, decimal baseSalary)
        {
            var deductions = new List<Deduction>();
            decimal totalDeductions = 0;

            var uppaidLeveDays = await GetUnpaidLeaveDaysAsync(employeeId, payPeriod);
            var dailyRate = baseSalary / 22;
         
            if (uppaidLeveDays > 0)
            {
                deductions.Add(new Deduction
                {
                    Type = "Unpaid Leave",
                    Amount = uppaidLeveDays * dailyRate,
                });
            }

            //Tax deduction 
          
            deductions.Add(new Deduction
            {
                Type = "Tax",
                Amount = baseSalary * 0.15m,
            });
            return deductions;
        }

        private async Task<int> GetUnpaidLeaveDaysAsync(int employeeId, DateTime payPeriod)
        {
            var startDate = new DateTime(payPeriod.Year, payPeriod.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var unpaidLeaves = await _context.LeaveRequests
                .Where(lr => lr.EmployeeId == employeeId &&
                lr.LeaveType == "Unpaid" &&
                lr.Status == "Approved" &&
                lr.StartDate >= startDate &&
                lr.EndDate <= endDate).ToListAsync();
            return unpaidLeaves.Sum(lr => (lr.EndDate - lr.StartDate).Days +1); 
        }

        private async Task<List<Allowance>> CalculateAllowancesAsync(int employeeId, DateTime payPeriod)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            var allowances = new List<Allowance>();
            decimal totalAllowance = 0;

            // Housing allowance 10% of base salary for managers and above
            if (employee.Role == "Manager" || employee.Role == "HR" || employee.Role == "Admin")
            {
                var housingAllowance = employee.Salary * 0.10m;
                allowances.Add(new Allowance
                {
                    Type = "Housing Allowance",
                    Amount = housingAllowance,
              
                });
                totalAllowance += housingAllowance;
            }

            // Transport allowance $200
            allowances.Add(new Allowance
            {
                Type = "Transportation Allowance",
                Amount = 200,
              
            });
            totalAllowance += 200;

            // IT department allowance
            if (employee.Department == "IT")
            {
                allowances.Add(new Allowance
                {
                    Type = "IT Department Allowance",
                    Amount = 150,
               
                });
                totalAllowance += 150;
            }

            return allowances;
        }

        public async Task<List<Payroll>> GetAllPayrollsAsync()
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Include(p => p.Allowances)
                .Include(p => p.Deductions)
                .OrderByDescending(p => p.PayPeriod)
                .ToListAsync();

        }

       public async Task<Payroll> GetPayrollByIdAsync(int id)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Include(p => p.Allowances)
                .Include(p => p.Deductions)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Payroll>> GetPayrollsByEmployeeAsync(int employeeId)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Include(p => p.Allowances)
                .Include(p => p.Deductions)
                .Where(p => p.EmployeeId == employeeId).OrderByDescending(p => p.PayPeriod).ToListAsync();
        }

       public async Task<List<Payroll>> GetPayrollsByPeriodAsync(DateTime payPeriod)
        {
           return await _context.Payrolls
                .Include(p => p.Employee)
                .Include(p => p.Allowances)
                .Include(p => p.Deductions)
                .Where(p => p.PayPeriod.Year == payPeriod.Year && p.PayPeriod.Month == payPeriod.Month)
                .OrderByDescending(p => p.Employee.Name)
                .ToListAsync();
        }
    }
}
