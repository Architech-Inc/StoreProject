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
using Store.DbServices.Services.Interfaces;
using Store.DbServices.Workers;

namespace Store.DbServices.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStoreDbServices(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is required.");

        ServerVersion serverVersion;
        try
        {
            serverVersion = ServerVersion.AutoDetect(connectionString);
        }
        catch
        {
            serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
        }

        services.AddDbContext<StoreDbContext>(options =>
            options.UseMySql(
                connectionString,
                serverVersion,
                mySql => mySql.EnableRetryOnFailure(3)));

        // Repository + Unit of Work
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserAggregateRepository, UserAggregateRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        // Services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
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
        services.AddScoped<IStoreOperationsService, StoreOperationsService>();
        services.AddScoped<IBranchPricingService, BranchPricingService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<IMobileMoneyService, MobileMoneyService>();
        services.AddScoped<ILoyaltyService, LoyaltyService>();
        services.AddScoped<ILoyaltyCampaignService, LoyaltyCampaignService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<IBatchService, BatchService>();
        services.AddScoped<IStockTransferService, StockTransferService>();
        services.AddScoped<IWastageService, WastageService>();
        services.AddScoped<IDiscountOverrideService, DiscountOverrideService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<ICashVarianceService, CashVarianceService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<ICommunicationLogService, CommunicationLogService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IProcurementAutomationService, ProcurementAutomationService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<ITaxBracketService, TaxBracketService>();
        services.AddScoped<ISalaryService, SalaryService>();
        services.AddScoped<IDemandForecastingService, DemandForecastingService>();
        services.AddScoped<IEmailService, MockEmailService>();

        // SEC-06 — HMAC pepper for OTP hashing. Bound at startup so the service
        // throws on construction if the pepper is missing or < 32 bytes (raw or base64-decoded).
        services.AddOptions<OtpPepperOptions>()
                .Bind(config.GetSection(OtpPepperOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.OtpPepper),
                    "Auth:OtpPepper must be set to a non-empty value (env var Auth__OtpPepper).")
                .ValidateOnStart();

        services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OtpPepperOptions>>().Value);

        services.AddHostedService<OfflineLogSyncWorker>();
        services.AddHostedService<LogRetentionWorker>();
        services.AddHostedService<AutomatedReorderWorker>();

        return services;
    }
}
