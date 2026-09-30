using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Store.API.Attributes;
using Store.API.Controllers;
using Store.Models.DTOs.Audit;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

public class AuditAttributeTests
{
    [Fact]
    public void Constructor_SetsSummary_AndDefaults()
    {
        var attr = new AuditAttribute("cash.shift.open")
        {
            Category = "Financial",
            Severity = "Info"
        };

        Assert.Equal("cash.shift.open", attr.Summary);
        Assert.Equal("Financial", attr.Category);
        Assert.Equal("Info", attr.Severity);
    }

    [Theory]
    [InlineData(nameof(CashManagementController.OpenShift), "cash.shift.open", "Financial")]
    [InlineData(nameof(CashManagementController.CloseShift), "cash.shift.close", "Financial")]
    [InlineData(nameof(CashManagementController.DailyZReport), "cash.report.z", "Financial")]
    [InlineData(nameof(CashManagementController.DayEndReconciliation), "cash.reconciliation", "Financial")]
    public void CashManagementController_Endpoints_HaveAuditAttribute(string methodName, string expectedSummary, string expectedCategory)
    {
        var method = typeof(CashManagementController).GetMethod(methodName);
        Assert.NotNull(method);

        var auditAttr = method.GetCustomAttribute<AuditAttribute>();
        Assert.NotNull(auditAttr);
        Assert.Equal(expectedSummary, auditAttr.Summary);
        Assert.Equal(expectedCategory, auditAttr.Category);
    }

    [Fact]
    public async Task OnActionExecutionAsync_LogsAuditEntry_WhenSuccessful()
    {
        var mockAuditService = new Mock<IAuditLogService>();
        CreateAuditLogEntryRequest? loggedRequest = null;
        mockAuditService.Setup(a => a.LogAsync(It.IsAny<CreateAuditLogEntryRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CreateAuditLogEntryRequest, CancellationToken>((req, _) => loggedRequest = req)
            .ReturnsAsync(new AuditLogDto());

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IAuditLogService)))
            .Returns(mockAuditService.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProviderMock.Object
        };
        httpContext.Request.Method = "POST";
        httpContext.Request.Path = "/api/cash/shift/open";

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var filterMetadata = new List<IFilterMetadata>();
        var executingContext = new ActionExecutingContext(actionContext, filterMetadata, new Dictionary<string, object?>(), new object());

        var executedContext = new ActionExecutedContext(actionContext, filterMetadata, new object())
        {
            Result = new OkResult()
        };

        var attribute = new AuditAttribute("cash.shift.open")
        {
            Category = "Financial",
            Severity = "Info"
        };

        await attribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(executedContext));

        Assert.NotNull(loggedRequest);
        Assert.Equal("cash.shift.open", loggedRequest.Summary);
        Assert.Equal("Financial", loggedRequest.Category);
        Assert.Equal("POST /api/cash/shift/open", loggedRequest.Action);
    }
}
