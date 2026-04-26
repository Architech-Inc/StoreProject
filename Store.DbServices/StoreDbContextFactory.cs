using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Store.DbServices.Context;

namespace Store.DbServices;

/// <summary>
/// Allows EF Core tools (migrations) to create StoreDbContext at design time
/// without running the full application host.
/// Run from Store.DbServices project directory:
///   dotnet ef migrations add InitialCreate --startup-project ../Store.API
///   dotnet ef database update --startup-project ../Store.API
/// </summary>
public class StoreDbContextFactory : IDesignTimeDbContextFactory<StoreDbContext>
{
    public StoreDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Store.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<StoreDbContext>();
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new StoreDbContext(optionsBuilder.Options);
    }
}
