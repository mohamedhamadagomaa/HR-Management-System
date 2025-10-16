using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IEmailService
    {
        Task SendLeaveRequestNotificationAsync(string toEmail, string employeeName, DateTime startDate, DateTime endDate, string leaveType);
        Task SendLeaveStatusUpdateAsync(string toEmail, string employeeName, string status, string managerComments);
        Task SendPayrollGeneratedNotificationAsync(string toEmail, string employeeName, DateTime payPeriod, decimal netPay);
    }
}
