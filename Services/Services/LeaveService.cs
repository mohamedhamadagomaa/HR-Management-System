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
    public class LeaveService : ILeaveService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly IEmployeesServices _employeeService;

        public LeaveService(ApplicationDbContext context , IEmailService emailService, IAuditService auditService, IEmployeesServices employeeService)
        {
            this._context = context;
            this._emailService = emailService;
            this._auditService = auditService;
            this._employeeService = employeeService;
        }

        public async Task CreateLeaveRequestAsync(LeaveRequest leaveRequest)
        {
            // Validate leave balance for annual leave
            if (leaveRequest.LeaveType == "Annual")
            {
                var balance = await GetRemainingLeaveBalanceAsync(leaveRequest.EmployeeId);
                var reqDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;
                if (reqDays > balance)
                {
                    throw new InvalidOperationException("Insufficient Leave balance");
                }
            }

            // Check for Overlapping Leave
            if (await HasOverlappingLeaveAsync(leaveRequest.EmployeeId, leaveRequest.StartDate, leaveRequest.EndDate))
            {
                throw new InvalidOperationException("Overlapping Leave request exists");
            }

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Leave request saved successfully with ID: {leaveRequest.Id}");

            try
            {
                Console.WriteLine("Starting audit log...");
                await _auditService.LogActionAsync(
                    userId: "System",
                    action: "Create",
                    entityType: "LeaveRequest",
                    entityId: leaveRequest.Id,
                    newValues: new
                    {
                        leaveRequest.Id,
                        leaveRequest.StartDate,
                        leaveRequest.EndDate,
                        leaveRequest.LeaveType,
                        leaveRequest.Status
                    }
                );
                Console.WriteLine("Audit log completed successfully.");
            }
            catch (Exception auditEx)
            {
                Console.WriteLine($"Audit log failed: {auditEx.Message}");
                // Don't re-throw - we don't want audit failure to prevent leave request
            }

            try
            {
                Console.WriteLine("Starting manager notification...");
                await NotifyManagersAboutNewLeaveRequest(leaveRequest);
                Console.WriteLine("Manager notification completed successfully.");
            }
            catch (Exception notifyEx)
            {
                Console.WriteLine($"Manager notification failed: {notifyEx.Message}");
                // Don't re-throw - we don't want notification failure to prevent leave request
            }
        }
        private async Task NotifyManagersAboutNewLeaveRequest(LeaveRequest leaveRequest)
        {
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(leaveRequest.EmployeeId);
                var managers = await _context.Employees
                    .Where(e => e.Role == "Manager" || e.Role == "HR" || e.Role == "Admin")
                    .ToListAsync();

                foreach (var manager in managers)
                {
                    await _emailService.SendLeaveRequestNotificationAsync(
                        toEmail: "manager@company.com", // In real app, get manager email
                        employeeName: employee.Name,
                        startDate: leaveRequest.StartDate,
                        endDate: leaveRequest.EndDate,
                        leaveType: leaveRequest.LeaveType
                    );
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the application
                Console.WriteLine($"Failed to send notifications: {ex.Message}");
            }
        }

        public Task DeleteLeaveRequestAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<LeaveRequest>> GetAllLeaveRequestsAsync()
        {
            return await _context.LeaveRequests
                .Include(v => v.Employee).OrderByDescending(v => v.CreatedAt).ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetPendingLeaveRequestsAsync()
        {
            return await _context.LeaveRequests.
                Include(v => v.Employee)
                .Where(v => v.Status == "Pending")
                .OrderBy(v => v.CreatedAt)
                .ToListAsync();
        }

        // Inside Services/Services/LeaveService.cs

        public async Task<int> GetRemainingLeaveBalanceAsync(int employeeId)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) throw new ArgumentException("Employee Not Found");

            // 1. Get the Official Balance (Balance after approved deductions)
            int officialBalance = employee.LeaveBalance;

            // 2. Calculate total days for Pending Annual Leave requests
            // We only deduct "Annual" leave type and only those that are "Pending".
            var pendingAnnualRequests = await _context.LeaveRequests
                .Where(v => v.EmployeeId == employeeId &&
                            v.LeaveType == "Annual" &&
                            v.Status == "Pending")
                .ToListAsync();

            int pendingDays = 0;
            foreach (var request in pendingAnnualRequests)
            {
                // Calculate days: (End - Start) + 1
                // Ensure you handle the case where StartDate and EndDate might be the same day
                pendingDays += (request.EndDate - request.StartDate).Days + 1;
            }

            // 3. Calculate the Available Balance for display/validation
            int availableBalance = officialBalance - pendingDays;

            // Ensure the returned balance is not negative for display purposes, 
            // although validation should ideally catch this when creating the request.
            return Math.Max(0, availableBalance);
        }

        public async Task<LeaveRequest> GetLeaveRequestByIdAsync(int id)
        {
            return await _context.LeaveRequests.Include(v => v.Employee).FirstOrDefaultAsync(v => v.Id == id);
             
        }

        public async Task<List<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
        {
            return await _context.LeaveRequests
                .Where(v => v.EmployeeId == employeeId)
                .OrderByDescending(v => v.CreatedAt).
                ToListAsync();
        }

        public async Task<bool> HasOverlappingLeaveAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            return await _context.LeaveRequests
                .AnyAsync(v => v.EmployeeId == employeeId &&
                               v.Status != "Rejected" && (startDate <= v.EndDate && endDate >= v.StartDate));
        }

        public async Task RejectLeaveRequestAsync(int VacRequestId, string processedBy, string comments)
        {
            var LeaveRequest = await GetLeaveRequestByIdAsync(VacRequestId);
            if (LeaveRequest == null) throw new ArgumentException("Leave Request Not Found");
            LeaveRequest.Status = "Rejected";
            LeaveRequest.ProcessedAt = DateTime.Now;
            LeaveRequest.ProcessedBy = processedBy;
            LeaveRequest.ManagerComments = comments;

            await _context.SaveChangesAsync();
        }



        //public async Task ApproveLeaveRequestAsync(int leaveRequestId, string processedBy, string comments)
        //{
        //    var vacreq = await GetLeaveRequestByIdAsync(leaveRequestId);
        //    if (vacreq == null) throw new ArgumentException("Leave Request Not Found");
        //    vacreq.Status = "Approved";
        //    vacreq.ProcessedAt = DateTime.Now;
        //    vacreq.ProcessedBy = processedBy;
        //    vacreq.ManagerComments = comments;

        //    //deduct from balance if annual Leave
        //    if(vacreq.LeaveType == "Annual")
        //    {
        //        var employee = await _context.Employees.FindAsync(vacreq.EmployeeId);
        //        var days = (vacreq.EndDate - vacreq.StartDate).Days + 1;
        //        employee.LeaveBalance -= days;
        //    }
        //    await _context.SaveChangesAsync();
        //}

        public async Task ApproveLeaveRequestAsync(int leaveRequestId, string processedBy, string comments = "")
        {
            var leaveRequest = await GetLeaveRequestByIdAsync(leaveRequestId);
            if (leaveRequest == null) throw new ArgumentException("Leave request not found");

            // Store old status for audit
            var oldStatus = leaveRequest.Status;

            leaveRequest.Status = "Approved";
            leaveRequest.ProcessedAt = DateTime.Now;
            leaveRequest.ProcessedBy = processedBy;
            leaveRequest.ManagerComments = comments;

            // Deduct from balance if annual leave - THIS IS THE KEY PART
            if (leaveRequest.LeaveType == "Annual")
            {
                var employee = await _context.Employees.FindAsync(leaveRequest.EmployeeId);
                if (employee != null)
                {
                    var requestedDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days + 1;

                    Console.WriteLine($"=== LEAVE BALANCE UPDATE ===");
                    Console.WriteLine($"Employee: {employee.Name}");
                    Console.WriteLine($"Old Balance: {employee.LeaveBalance}");
                    Console.WriteLine($"Requested Days: {requestedDays}");
                    Console.WriteLine($"Leave Type: {leaveRequest.LeaveType}");

                    // Only deduct if the request is being approved (not if it was already approved)
                    if (oldStatus != "Approved")
                    {
                        employee.LeaveBalance -= requestedDays;
                        Console.WriteLine($"New Balance: {employee.LeaveBalance}");
                    }
                    else
                    {
                        Console.WriteLine("Leave was already approved - no balance change");
                    }
                    Console.WriteLine($"============================");
                }
                else
                {
                    Console.WriteLine($"ERROR: Employee not found for ID: {leaveRequest.EmployeeId}");
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
