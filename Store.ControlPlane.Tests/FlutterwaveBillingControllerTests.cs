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

public class FlutterwaveBillingControllerTests
{
    private readonly Mock<IFlutterwavePaymentService> _flutterwaveMock;
    private readonly Mock<ITenantRepository> _tenantRepoMock;
    private readonly Mock<ILogger<FlutterwaveBillingController>> _loggerMock;
    private readonly Mock<ILogger<SubscriptionReconciler>> _reconcilerLoggerMock;
    private readonly SubscriptionReconciler _reconciler;
    private readonly FlutterwaveBillingController _controller;

    public FlutterwaveBillingControllerTests()
    {
        _flutterwaveMock = new Mock<IFlutterwavePaymentService>();
        _tenantRepoMock = new Mock<ITenantRepository>();
        _loggerMock = new Mock<ILogger<FlutterwaveBillingController>>();
        _reconcilerLoggerMock = new Mock<ILogger<SubscriptionReconciler>>();
        _reconciler = new SubscriptionReconciler(_tenantRepoMock.Object, _reconcilerLoggerMock.Object);

        _controller = new FlutterwaveBillingController(
            _flutterwaveMock.Object,
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
            PlanId = "professional",
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
            TotalAmount = 29900
        };

        var result = await _controller.CreateInvoice(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiErrorResponse>(badRequest.Value);
        Assert.Equal(ErrorCode.InvalidRequest, response.Code);
    }

    [Fact]
    public async Task CreateInvoice_Unconfigured_ReturnsServiceUnavailable()
    {
        var request = new CreateInvoiceRequest
        {
            TenantId = Guid.NewGuid(),
            PlanId = "professional",
            TotalAmount = 29900
        };

        _flutterwaveMock
            .Setup(f => f.CreatePaymentLinkAsync(It.IsAny<FlutterwavePaymentLinkRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FlutterwavePaymentLinkResponse
            {
                Success = false,
                ResponseCode = "NOT_CONFIGURED",
                Message = "Not configured"
            });

        var result = await _controller.CreateInvoice(request, CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, statusResult.StatusCode);
        var response = Assert.IsType<ApiErrorResponse>(statusResult.Value);
        Assert.Equal(ErrorCode.PaymentProviderUnavailable, response.Code);
    }

    [Fact]
    public async Task CreateInvoice_Success_ReturnsPaymentLink()
    {
        var request = new CreateInvoiceRequest
        {
            TenantId = Guid.NewGuid(),
            PlanId = "professional",
            TotalAmount = 29900,
            Currency = "USD"
        };

        _flutterwaveMock
            .Setup(f => f.CreatePaymentLinkAsync(It.IsAny<FlutterwavePaymentLinkRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FlutterwavePaymentLinkResponse
            {
                Success = true,
                PaymentLink = "https://checkout.flutterwave.com/v3/hosted/pay/flw12345",
                TxRef = "flw-sub-12345",
                Message = "Link created"
            });

        var result = await _controller.CreateInvoice(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<CreateInvoiceResponse>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("https://checkout.flutterwave.com/v3/hosted/pay/flw12345", response.Data.CheckoutUrl);
    }

    [Fact]
    public async Task Verify_TransactionNotFound_ReturnsNotFound()
    {
        _flutterwaveMock
            .Setup(f => f.VerifyTransactionAsync(9999L, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FlutterwaveVerifyResponse?)null);

        var result = await _controller.Verify(9999L, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiErrorResponse>(notFound.Value);
        Assert.Equal(ErrorCode.NotFound, response.Code);
    }

    [Fact]
    public async Task Webhook_InvalidHash_ReturnsUnauthorized()
    {
        _flutterwaveMock
            .Setup(f => f.VerifyWebhookHash(It.IsAny<string?>()))
            .Returns(false);

        var result = await _controller.Webhook(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result);
    }
}
