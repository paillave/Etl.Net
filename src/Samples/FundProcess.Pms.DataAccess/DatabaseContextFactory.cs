using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace FundProcess.Pms.DataAccess
{
    //https://www.benday.com/2017/12/19/ef-core-2-0-migrations-without-hard-coded-connection-strings/
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            string connectionString = GetConnectionString();
            System.Console.WriteLine(connectionString);
            optionsBuilder.UseSqlServer(connectionString);
            return new DatabaseContext(optionsBuilder.Options, TenantContext.Empty);
        }
        private string GetConnectionString()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))) return "N/A";
            var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");
            var config = builder.Build();
            return config.GetConnectionString("Default");
        }
    }
}