using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.RateLimiting;
using Store.API.Controllers;
using Store.Models.DTOs.Operations;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 38 — Comprehensive Controller Authorization Policy & Endpoint Security Audit (SEC-20).
/// Verifies reflection-based policy coverage across all controllers, ensures no unintended
/// anonymous endpoints leak, and validates policy hardening across sensitive endpoints.
/// </summary>
public class EndpointAuthorizationSecurityTests
{
    private static readonly HashSet<Type> ControllerTypes = typeof(AuthController).Assembly
        .GetTypes()
        .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
        .ToHashSet();

    [Fact]
    public void AllControllers_AreDiscovered_ExpectedCount39()
    {
        Assert.Equal(39, ControllerTypes.Count);
    }

    [Fact]
    public void AllowAnonymous_Endpoints_AreStrictlyAllowlisted()
    {
        var allowedAnonymousEndpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Public pre-auth & recovery endpoints
            "AuthController.Login",
            "AuthController.LoginWithEmail",
            "AuthController.LoginWithPhone",
            "AuthController.Login2FA",
            "AuthController.Refresh",
            "AuthController.GetAvatar",
            "PasswordRecoveryController.RequestOtp",
            "PasswordRecoveryController.VerifyOtp",
            "PasswordRecoveryController.ResetPassword",

            // Public country/currency catalog
            "CountriesController.GetAll",
            "CountriesController.GetByIsoCode",
            "CountriesController.GetDefault",

            // Public WebAuthn assertion (FIDO2 challenge/assertion)
            "WebAuthnController.AssertionOptions",
            "WebAuthnController.MakeAssertion",

            // Webhook callbacks (HMAC-SHA256 signature / verif-hash verified)
            "PaymentsController.MtnMomoCallback",
            "PaymentsController.OrangeMoneyCallback",
            "PaymentsController.FlutterwaveWebhook",

            // Public receipt lookup by GUID
            "InvoicesController.GetPublicReceipt",

            // Email/SMS token contact change verification link
            "UsersController.VerifyContactChange"
        };

        var discoveredAnonymousEndpoints = new List<string>();

        foreach (var controller in ControllerTypes)
        {
            var isClassAnonymous = controller.GetCustomAttribute<AllowAnonymousAttribute>() != null;

            var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => m.GetCustomAttributes().Any(a => a is HttpMethodAttribute));

            foreach (var m in methods)
            {
                var isMethodAnonymous = m.GetCustomAttribute<AllowAnonymousAttribute>() != null;
                var isMethodAuthorize = m.GetCustomAttribute<AuthorizeAttribute>() != null;

                // An action is anonymous if: (class is AllowAnonymous AND method has no Authorize) OR (method has AllowAnonymous)
                if (isMethodAnonymous || (isClassAnonymous && !isMethodAuthorize))
                {
                    discoveredAnonymousEndpoints.Add($"{controller.Name}.{m.Name}");
                }
            }
        }

        foreach (var endpoint in discoveredAnonymousEndpoints)
        {
            Assert.Contains(endpoint, allowedAnonymousEndpoints);
        }

        Assert.Equal(allowedAnonymousEndpoints.Count, discoveredAnonymousEndpoints.Count);
    }

    [Fact]
    public void PublicReceipt_HasRateLimitingAttribute()
    {
        var method = typeof(InvoicesController).GetMethod(nameof(InvoicesController.GetPublicReceipt));
        Assert.NotNull(method);

        var rateLimitAttr = method.GetCustomAttribute<EnableRateLimitingAttribute>();
        Assert.NotNull(rateLimitAttr);
        Assert.Equal("general", rateLimitAttr.PolicyName);
    }

    [Fact]
    public void VerifyContactChange_HasRateLimitingAttribute()
    {
        var method = typeof(UsersController).GetMethod(nameof(UsersController.VerifyContactChange));
        Assert.NotNull(method);

        var rateLimitAttr = method.GetCustomAttribute<EnableRateLimitingAttribute>();
        Assert.NotNull(rateLimitAttr);
        Assert.Equal("general", rateLimitAttr.PolicyName);
    }

    [Fact]
    public void CommunicationLogsController_HasCommunicationsReadPolicy()
    {
        var classAuth = typeof(CommunicationLogsController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(classAuth);
        Assert.Equal(PermissionKeys.CommunicationsRead, classAuth.Policy);
    }

    [Fact]
    public void FinanceController_HasFinanceReadPolicy()
    {
        var classAuth = typeof(FinanceController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(classAuth);
        Assert.Equal(PermissionKeys.FinanceRead, classAuth.Policy);
    }

    [Fact]
    public void PayrollController_HasFineGrainedPolicies()
    {
        var classAuth = typeof(PayrollController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(classAuth);
        Assert.Equal(PermissionKeys.PayrollRead, classAuth.Policy);

        var draftMethod = typeof(PayrollController).GetMethod(nameof(PayrollController.DraftPayrollRun));
        Assert.NotNull(draftMethod);
        var draftAuth = draftMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(draftAuth);
        Assert.Equal(PermissionKeys.PayrollWrite, draftAuth.Policy);

        var approveMethod = typeof(PayrollController).GetMethod(nameof(PayrollController.ApprovePayroll));
        Assert.NotNull(approveMethod);
        var approveAuth = approveMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(approveAuth);
        Assert.Equal(PermissionKeys.PayrollWrite, approveAuth.Policy);

        var payMethod = typeof(PayrollController).GetMethod(nameof(PayrollController.PayPayroll));
        Assert.NotNull(payMethod);
        var payAuth = payMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(payAuth);
        Assert.Equal(PermissionKeys.PayrollWrite, payAuth.Policy);
    }

    [Fact]
    public void CustomersController_HasFineGrainedPolicies()
    {
        var classAuth = typeof(CustomersController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(classAuth);
        Assert.Equal(PermissionKeys.CustomerRead, classAuth.Policy);

        var createMethod = typeof(CustomersController).GetMethod(nameof(CustomersController.Create));
        Assert.NotNull(createMethod);
        var createAuth = createMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(createAuth);
        Assert.Equal(PermissionKeys.CustomerCreate, createAuth.Policy);

        var updateMethod = typeof(CustomersController).GetMethod(nameof(CustomersController.Update));
        Assert.NotNull(updateMethod);
        var updateAuth = updateMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(updateAuth);
        Assert.Equal(PermissionKeys.CustomerUpdate, updateAuth.Policy);

        var deleteMethod = typeof(CustomersController).GetMethod(nameof(CustomersController.Delete));
        Assert.NotNull(deleteMethod);
        var deleteAuth = deleteMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(deleteAuth);
        Assert.Equal(PermissionKeys.CustomerDelete, deleteAuth.Policy);
    }

    [Fact]
    public void EmployeesController_HasFineGrainedPolicies()
    {
        var classAuth = typeof(EmployeesController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(classAuth);
        Assert.Equal(PermissionKeys.EmployeeRead, classAuth.Policy);

        var createMethod = typeof(EmployeesController).GetMethod(nameof(EmployeesController.Create));
        Assert.NotNull(createMethod);
        var createAuth = createMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(createAuth);
        Assert.Equal(PermissionKeys.EmployeeCreate, createAuth.Policy);

        var updateMethod = typeof(EmployeesController).GetMethod(nameof(EmployeesController.Update));
        Assert.NotNull(updateMethod);
        var updateAuth = updateMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(updateAuth);
        Assert.Equal(PermissionKeys.EmployeeUpdate, updateAuth.Policy);

        var deleteMethod = typeof(EmployeesController).GetMethod(nameof(EmployeesController.Delete));
        Assert.NotNull(deleteMethod);
        var deleteAuth = deleteMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(deleteAuth);
        Assert.Equal(PermissionKeys.EmployeeDelete, deleteAuth.Policy);
    }

    [Fact]
    public void InvoicesController_HasFineGrainedPolicies()
    {
        var getAllMethod = typeof(InvoicesController).GetMethod(nameof(InvoicesController.GetAll));
        Assert.NotNull(getAllMethod);
        var getAllAuth = getAllMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(getAllAuth);
        Assert.Equal(PermissionKeys.InvoiceRead, getAllAuth.Policy);

        var createMethod = typeof(InvoicesController).GetMethod(nameof(InvoicesController.Create));
        Assert.NotNull(createMethod);
        var createAuth = createMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(createAuth);
        Assert.Equal(PermissionKeys.InvoiceCreate, createAuth.Policy);
    }

    [Fact]
    public void LoyaltyCampaignsController_HasLoyaltyPolicies()
    {
        var classAuth = typeof(LoyaltyCampaignsController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(classAuth);
        Assert.Equal(PermissionKeys.LoyaltyRead, classAuth.Policy);

        var createMethod = typeof(LoyaltyCampaignsController).GetMethod(nameof(LoyaltyCampaignsController.Create));
        Assert.NotNull(createMethod);
        var createAuth = createMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(createAuth);
        Assert.Equal(PermissionKeys.LoyaltyWrite, createAuth.Policy);
    }

    [Fact]
    public void UsersController_GetById_RequiresAdminUsersPolicy()
    {
        var getByIdMethod = typeof(UsersController).GetMethod(nameof(UsersController.GetById));
        Assert.NotNull(getByIdMethod);
        var getByIdAuth = getByIdMethod.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(getByIdAuth);
        Assert.Equal(PermissionKeys.AdminUsers, getByIdAuth.Policy);

        var get360Method = typeof(UsersController).GetMethod(nameof(UsersController.Get360ById));
        Assert.NotNull(get360Method);
        var get360Auth = get360Method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(get360Auth);
        Assert.Equal(PermissionKeys.AdminUsers, get360Auth.Policy);
    }

    [Fact]
    public void PaymentsController_Initiate_RequiresCashWritePolicy()
    {
        var method = typeof(PaymentsController).GetMethod(nameof(PaymentsController.Initiate));
        Assert.NotNull(method);
        var auth = method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(auth);
        Assert.Equal(PermissionKeys.CashWrite, auth.Policy);
    }

    [Fact]
    public void PaymentsController_InitiateFlutterwave_RequiresCashWritePolicy()
    {
        var method = typeof(PaymentsController).GetMethod(nameof(PaymentsController.InitiateFlutterwave));
        Assert.NotNull(method);
        var auth = method.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(auth);
        Assert.Equal(PermissionKeys.CashWrite, auth.Policy);
    }
}
