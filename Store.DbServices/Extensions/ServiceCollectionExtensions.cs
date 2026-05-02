using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Store.DbServices.Context;
using Store.DbServices.Repositories;
using Store.DbServices.Repositories.Users;
using Store.DbServices.Services;
using Store.DbServices.UnitOfWork;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Repositories;
using Store.Models.Interfaces.Repositories.Users;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStoreDbServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is required.");

        services.AddDbContext<StoreDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mySql => mySql.EnableRetryOnFailure(3)));

        // Repository + Unit of Work
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserAggregateRepository, UserAggregateRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        // Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IOtpService, OtpService>();

        return services;
    }
}
