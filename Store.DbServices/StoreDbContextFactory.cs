using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Store.DbServices.Context;

namespace Store.DbServices;

/// <summary>
/// Allows EF Core tools (migrations) to create StoreDbContext at design time
/// without running the full application host.
/// </summary>
public class StoreDbContextFactory : IDesignTimeDbContextFactory<StoreDbContext>
{
    public StoreDbContext CreateDbContext(string[] args)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var apiPath = Directory.Exists(Path.Combine(currentDir, "Store.API"))
            ? Path.Combine(currentDir, "Store.API")
            : Directory.Exists(Path.Combine(currentDir, "../Store.API"))
                ? Path.Combine(currentDir, "../Store.API")
                : currentDir;

        var config = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("OVERRIDE_ME"))
        {
            connectionString = "Server=127.0.0.1;Port=3306;Database=store_db_v2;User Id=root;Password=;AllowPublicKeyRetrieval=True;";
        }

        ServerVersion serverVersion;
        try
        {
            serverVersion = ServerVersion.AutoDetect(connectionString);
        }
        catch
        {
            serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
        }

        var optionsBuilder = new DbContextOptionsBuilder<StoreDbContext>();
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new StoreDbContext(optionsBuilder.Options);
    }
}
