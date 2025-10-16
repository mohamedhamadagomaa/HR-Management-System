using Entity.Data;
using Entity.Entities;
using HR_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using System.Threading.Tasks;

namespace HR_Management_System.Controllers
{
    public class LeaveController : Controller
    {
        private readonly ILeaveService _leaveService;
        private readonly IEmployeesServices _employeesServices;
        private readonly ApplicationDbContext _context;

        public LeaveController(ILeaveService leaveService , IEmployeesServices employeesServices,ApplicationDbContext context)
        {
            this._leaveService = leaveService;
            this._employeesServices = employeesServices;
            this._context = context;
        }
        public async Task<IActionResult> Index()
        {
            var currentUser = await _employeesServices.GetEmployeeByUserIdAsync(User.Identity.Name);
            
            if (currentUser == null) return RedirectToAction("Login", "Account");
            if(currentUser.Role == "Admin" || currentUser.Role == "Manager")
            {
                var allRequests = await _leaveService.GetAllLeaveRequestsAsync();
                return View("ManageLeaves", allRequests);
            }
            else
            {
                var myRequests = await _leaveService.GetLeaveRequestsByEmployeeIdAsync(currentUser.Id);
                return View("EmployeeLeaves", myRequests);
            }
          
        }



        // GET: Leave/Apply
        public async Task<IActionResult> Apply()
        {
            var currentUser = await _employeesServices.GetEmployeeByUserIdAsync(User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Create ViewModel, not Entity
            var model = new LeaveRequestViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(LeaveRequestViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var currentUser = await _employeesServices.GetEmployeeByUserIdAsync(User.Identity.Name);
                if (currentUser == null)
                {
                    TempData["ErrorMessage"] = "Employee profile not found.";
                    return RedirectToAction("Login", "Account");
                }

                if (model.StartDate >= model.EndDate)
                {
                    ModelState.AddModelError("", "End date must be after start date.");
                    return View(model);
                }

                var leaveRequest = new LeaveRequest
                {
                    EmployeeId = currentUser.Id,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    LeaveType = model.LeaveType,
                    Reason = model.Reason,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                await _leaveService.CreateLeaveRequestAsync(leaveRequest);
                TempData["SuccessMessage"] = "Leave request submitted successfully!";
                return RedirectToAction("MyRequests");
            }
            catch (Exception ex)
            {
                var currentUser = await _employeesServices.GetEmployeeByUserIdAsync(User.Identity.Name);
                // Check if the leave request was actually saved despite the error
                var wasSaved = await _context.LeaveRequests
                    .AnyAsync(lr => lr.EmployeeId == currentUser.Id &&
                                   lr.StartDate == model.StartDate &&
                                   lr.EndDate == model.EndDate &&
                                   lr.LeaveType == model.LeaveType);

                if (wasSaved)
                {
                  
                    return RedirectToAction("MyRequests");
                }
                else
                {
                    // Data was not saved - show error
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                    return View(model);
                }
            }
        }
        public async Task<IActionResult> MyRequests()
        {
            var currentUser = await _employeesServices.GetEmployeeByUserIdAsync(User.Identity.Name);
            if (currentUser == null) return RedirectToAction("Login", "Account");
            var myRequests = await _leaveService.GetLeaveRequestsByEmployeeIdAsync(currentUser.Id);
            return View(myRequests);
        }

        [Authorize(Roles ="Admin,HR,Manager")]
        public async Task<IActionResult> Manage()
        {
            var pendingRequests = await _leaveService.GetPendingLeaveRequestsAsync();
            return View(pendingRequests);
        }
        [HttpPost]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> Approve(int id , string comments = "")
        {
            var currentUser = await _employeesServices.GetEmployeeByUserIdAsync(User.Identity.Name);
            if (currentUser == null) return RedirectToAction("Login", "Account");
            try
            {
                await _leaveService.ApproveLeaveRequestAsync(id, currentUser.Name, comments);
                return Json(new { success = true, message = "Leave request approved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
           
        }
        [HttpPost]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> Reject(int id , string comments)
        {
            var currentUser = await _employeesServices.GetEmployeeByUserIdAsync(User.Identity.Name);
            if (currentUser == null) return RedirectToAction("Login", "Account");
            try
            {
                await _leaveService.RejectLeaveRequestAsync(id, currentUser.Name, comments);
                return Json(new { success = true, message = "Leave request rejected !" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> GetDashboardDate()
        {
            var currentUser = await _employeesServices.GetEmployeeByUserIdAsync(User.Identity.Name);
            if (currentUser == null) return Json(new { success = false, message = "User not found" });
            var leavBalance = await _leaveService.GetRemainingLeaveBalanceAsync(currentUser.Id);
            var myRequests = await _leaveService.GetLeaveRequestsByEmployeeIdAsync(currentUser.Id);
            var recentRequests = myRequests.Take(5).ToList();

            var recentActivityHtml = "";
            if (recentRequests.Any())
            {
                foreach(var request in recentRequests)
                {
                    var statusBadge = request.Status switch
                    {
                        "Approved" => "<span class='badge bg-success'>Approved</span>",
                        "Rejected" => "<span class='badge bg-danger'>Rejected</span>",
                        _ => "<span class='badge bg-warning'>Pending</span>"
                    };
                    recentActivityHtml += $@"
                        <div class='d-flex justify-content-between align-items-center border-bottom pb-2 mb-2'>
                            <div>
                                <strong>{request.LeaveType} Leave</strong><br>
                                <small class='text-muted'>{request.StartDate:MMM dd} - {request.EndDate:MMM dd}</small>
                            </div>
                            <div>
                                {statusBadge}
                            </div>
                        </div>";
                }
                
            }
            else
            {
                recentActivityHtml = "<p class='text-muted'>No recent leave requests</p>";
            }
            return Json(new
            {
                success = true,
                leavBalance,
                monthlyPay = currentUser.Salary,
                myRequests = myRequests.Count,
                recentActivityHtml
            });
        }
        }

    }

