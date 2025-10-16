using Entity.Data;
using Entity.Entities;
using HR_Management_System.Controllers;
using HR_Management_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Services.Interfaces;
using Services.Services;
using System.ComponentModel.DataAnnotations;

namespace LeavePayrollSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmployeesServices _employeesServices;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,IEmployeesServices employeesServices)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            this._employeesServices = employeesServices;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Since we used UserId as the username in Identity, we can sign in directly
                var result = await _signInManager.PasswordSignInAsync(
                    model.UserId, // This is the username in Identity
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // We'll get employee info in other controllers when needed
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid User ID or password.");
            }
            return View(model);

        }

        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if UserId already exists in Employees
                    var existingEmployee = await _employeesServices.GetEmployeeByUserIdAsync(model.UserId);
                    if (existingEmployee != null)
                    {
                        ModelState.AddModelError("UserId", "This User ID is already taken.");
                        return View(model);
                    }

                    // Check if UserId already exists as a username in Identity
                    var existingUser = await _userManager.FindByNameAsync(model.UserId);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("UserId", "This User ID is already taken.");
                        return View(model);
                    }

                    // Create Identity user with UserId as username
                    var user = new IdentityUser
                    {
                        UserName = model.UserId, // Use UserId as username
                        Email = model.Email
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // Create employee record with the custom UserId
                        var employee = new Employee
                        {
                            UserId = model.UserId,
                            Name = model.Name,
                            Department = model.Department,
                            Position = model.Position,
                            Salary = model.Salary,
                            Role = model.Role,
                            LeaveBalance = 21,
                            HireDate = DateTime.Now
                        };

                        await _employeesServices.CreateEmployeeAsync(employee);

                        // Assign role to user
                        await _userManager.AddToRoleAsync(user, model.Role);

                        // Sign in the user
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        TempData["SuccessMessage"] = $"Account created successfully! You can now login with User ID: {model.UserId}";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error creating account: {ex.Message}");
                }
            }
            return View(model);
        }
        private async Task CreateEmployeeRecord(string userId, string email)
        {
            // You'll need to inject ApplicationDbContext here
            using var scope = HttpContext.RequestServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var employee = new Employee
            {
                UserId = userId,
                Name = email.Split('@')[0], // Use part of email as name
                Department = "General",
                Position = "Employee",
                Salary = 30000,
                Role = "Employee",
                LeaveBalance = 21,
                HireDate = DateTime.Now
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login", "Account"); // Redirect to custom login page
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }



  
}