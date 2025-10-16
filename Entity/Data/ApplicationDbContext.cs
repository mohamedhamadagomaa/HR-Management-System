using Entity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Entity.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Allowance> Allowances { get; set; }
        public DbSet<Deduction> Deductions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Additional configuration can go here
            builder.Entity<Employee>(entity => entity.HasIndex(e => e.UserId).IsUnique());
            builder.Entity<LeaveRequest>(entity =>
            {
                entity.HasOne(v => v.Employee)
                      .WithMany(e => e.LeaveRequests)
                      .HasForeignKey(v => v.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<Payroll>(entity =>
            {
                entity.HasOne(p => p.Employee)
                      .WithMany(e => e.Payrolls)
                      .HasForeignKey(p => p.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
