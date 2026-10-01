using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Store.ControlPlane.Controllers;
using Store.ControlPlane.Repositories;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Payments;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.ControlPlane.Tests;

public class BillingControllerTests
{
    private readonly Mock<IPayDunyaPaymentService> _paydunyaMock;
    private readonly Mock<ITenantRepository> _tenantRepoMock;
    private readonly Mock<ILogger<BillingController>> _loggerMock;
    private readonly Mock<ILogger<SubscriptionReconciler>> _reconcilerLoggerMock;
    private readonly SubscriptionReconciler _reconciler;
    private readonly BillingController _controller;

    public BillingControllerTests()
    {
        _paydunyaMock = new Mock<IPayDunyaPaymentService>();
        _tenantRepoMock = new Mock<ITenantRepository>();
        _loggerMock = new Mock<ILogger<BillingController>>();
        _reconcilerLoggerMock = new Mock<ILogger<SubscriptionReconciler>>();
        _reconciler = new SubscriptionReconciler(_tenantRepoMock.Object, _reconcilerLoggerMock.Object);

        _controller = new BillingController(
            _paydunyaMock.Object,
            _reconciler,
            _tenantRepoMock.Object,
            _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task CreateInvoice_InvalidAmount_ReturnsBadRequest()
    {
        var request = new CreateInvoiceRequest
        {
            TenantId = Guid.NewGuid(),
            PlanId = "Professional",
            TotalAmount = 0
        };

        var result = await _controller.CreateInvoice(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiErrorResponse>(badRequest.Value);
        Assert.Equal(ErrorCode.InvalidRequest, response.Code);
    }

    [Fact]
    public async Task CreateInvoice_MissingPlanId_ReturnsBadRequest()
    {
        var request = new CreateInvoiceRequest
        {
            TenantId = Guid.NewGuid(),
            PlanId = "",
            TotalAmount = 50000
        };

        var result = await _controller.CreateInvoice(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiErrorResponse>(badRequest.Value);
        Assert.Equal(ErrorCode.InvalidRequest, response.Code);
    }

    [Fact]
    public async Task Confirm_ReturnsOk()
    {
        var resp = new ConfirmPaymentResponse
        {
            Token = "token-123",
            Status = "pending",
            ResponseCode = "00"
        };
        _paydunyaMock
            .Setup(p => p.ConfirmPaymentAsync("token-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(resp);

        var result = await _controller.Confirm("token-123", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<ConfirmPaymentResponse>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("token-123", response.Data.Token);
    }

    [Fact]
    public async Task Ipn_MissingSignature_ReturnsUnauthorized()
    {
        var result = await _controller.Ipn(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }
}
