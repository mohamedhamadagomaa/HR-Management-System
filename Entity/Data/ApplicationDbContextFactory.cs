using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Entity.Data;

namespace Entity.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Use the same connection string as in appsettings.json
            optionsBuilder.UseSqlServer("Server=MOHAMMAD-HAMADA\\SQLEXPRESS;Database=HR-Management-System;User Id=sa;Password=78951;TrustServerCertificate=true;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}