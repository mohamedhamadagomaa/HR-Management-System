using Entity.Data;
using Entity.Entities;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeavePayrollSystem.Web.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Create roles
            string[] roleNames = { "Admin", "HR", "Manager", "Employee" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create Admin user
            var adminUser = await userManager.FindByEmailAsync("admin@company.com");
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = "admin@company.com",
                    Email = "admin@company.com",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRolesAsync(adminUser, new[] { "Admin", "HR", "Manager" });

                // Create admin employee record
                var adminEmployee = new Employee
                {
                    UserId = adminUser.Id,
                    Name = "System Administrator",
                    Department = "IT",
                    Position = "System Admin",
                    Salary = 100000,
                    Role = "Admin",
                    LeaveBalance = 30
                };
                context.Employees.Add(adminEmployee);
            }

            await context.SaveChangesAsync();
        }
    }
}