using Entity.Data;
using LeavePayrollSystem.Services.Services;
using LeavePayrollSystem.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using Services.Services;

namespace HR_Management_System
{
    public class Program
    {
        public  static void Main(string[] args)
        {
        
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("HR-Management")));


            // Configure Identity with custom login path
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3;

                // Set custom login path
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();


            // Configure Application Cookie to use custom login path
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "YourAppCookie";
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Session expires after 30 mins
                options.SlidingExpiration = true; // Reset expiration on activity
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;

                // For development - expires when browser closes
                if (builder.Environment.IsDevelopment())
                {
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                    
                    // Or set to shorter time
                }
            });
            // Add services to the container.
            builder.Services.AddScoped<IEmployeesServices, EmployeeService>();
            builder.Services.AddScoped<ILeaveService, LeaveService>();
            builder.Services.AddScoped<IPayrollService, PayrollServices>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuditService, AuditService>();


            builder.Services.AddControllersWithViews();

            //dotnet ef migrations add AddAuditLogs --project "D:\dotnet\projects\HR-Management-System\Entity" --startup-project "D:\dotnet\projects\HR-Management-System\HR-Management-System"
            //dotnet ef database update --project "D:\dotnet\projects\HR-Management-System\Entity" --startup-project "D:\dotnet\projects\HR-Management-System\HR-Management-System"
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            //if (app.Environment.IsDevelopment())
            //{
            //    app.Use(async (context, next) =>
            //    {
            //        // Clear any existing authentication
            //        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            //        await next();
            //    });
            //}
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // Map both controllers and Razor Pages
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Public}/{action=Index}/{id?}");
            app.MapRazorPages();
         
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();

                // Create default admin user
                 SeedData.Initialize(scope.ServiceProvider);
            }

            app.Run();
        }
    }
}
