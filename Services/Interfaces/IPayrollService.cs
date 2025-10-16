using Entity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IPayrollService
    {
        Task<Payroll> GeneratePayrollAsync(int employeeId, DateTime payPeriod);
        Task<List<Payroll>> GetPayrollsByEmployeeAsync(int employeeId);
        Task<List<Payroll>> GetPayrollsByPeriodAsync(DateTime payPeriod);
        Task<Payroll> GetPayrollByIdAsync(int id);
        Task<List<Payroll>> GetAllPayrollsAsync(); // Add this method
    }
}
