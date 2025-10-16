using Entity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ILeaveService
    {
        Task<List<LeaveRequest>> GetAllLeaveRequestsAsync();
        Task<LeaveRequest> GetLeaveRequestByIdAsync(int id);
        Task CreateLeaveRequestAsync(LeaveRequest LeaveRequest);
    
        Task DeleteLeaveRequestAsync(int id);
        Task<List<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId);
        Task<List<LeaveRequest>> GetPendingLeaveRequestsAsync();
        Task ApproveLeaveRequestAsync(int leaveRequestId, string processedBy, string comments = "");
        Task RejectLeaveRequestAsync(int VacRequestId, string processedBy, string comments);
        Task<int> GetRemainingLeaveBalanceAsync(int employeeId);
        Task<bool> HasOverlappingLeaveAsync(int employeeId, DateTime startDate, DateTime endDate);
    }
}
