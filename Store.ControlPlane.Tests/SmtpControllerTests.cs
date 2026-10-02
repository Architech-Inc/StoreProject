using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Store.ControlPlane.Controllers;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Tenant;
using Xunit;

namespace Store.ControlPlane.Tests;

public class SmtpControllerTests
{
    private readonly Mock<ITenantSmtpService> _smtpServiceMock;
    private readonly Mock<ILogger<SmtpController>> _loggerMock;
    private readonly SmtpController _controller;

    public SmtpControllerTests()
    {
        _smtpServiceMock = new Mock<ITenantSmtpService>();
        _loggerMock = new Mock<ILogger<SmtpController>>();
        _controller = new SmtpController(_smtpServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetSmtpConfig_WhenFound_ReturnsOkWithMaskedPassword()
    {
        var tenantId = Guid.NewGuid();
        var configDto = new TenantSmtpConfigDto(
            TenantId: tenantId,
            Slug: "bastos-store",
            Host: "smtp.mailgun.org",
            Port: 587,
            Username: "postmaster@bastos.cm",
            PasswordMasked: "••••••••",
            HasPassword: true,
            FromEmail: "orders@bastos.cm",
            FromName: "Bastos Store",
            EnableSsl: true,
            IsEnabled: true,
            LastTestedAt: DateTime.UtcNow,
            LastTestStatus: "Success"
        );

        _smtpServiceMock
            .Setup(s => s.GetSmtpConfigAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(configDto);

        var result = await _controller.GetSmtpConfig(tenantId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<TenantSmtpConfigDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("smtp.mailgun.org", response.Data.Host);
        Assert.Equal("••••••••", response.Data.PasswordMasked);
        Assert.True(response.Data.HasPassword);
    }

    [Fact]
    public async Task GetSmtpConfig_WhenNotFound_ReturnsNotFound()
    {
        var tenantId = Guid.NewGuid();
        _smtpServiceMock
            .Setup(s => s.GetSmtpConfigAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantSmtpConfigDto?)null);

        var result = await _controller.GetSmtpConfig(tenantId, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(notFoundResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task UpdateSmtpConfig_EnterpriseTier_ReturnsOk()
    {
        var tenantId = Guid.NewGuid();
        var request = new UpdateTenantSmtpRequest
        {
            Host = "smtp.sendgrid.net",
            Port = 587,
            Username = "apikey",
            Password = "SG.secret_key_123",
            FromEmail = "receipts@clexanfoods.cm",
            FromName = "ClexAn Receipts",
            EnableSsl = true,
            IsEnabled = true
        };

        var updatedDto = new TenantSmtpConfigDto(
            TenantId: tenantId,
            Slug: "bastos-store",
            Host: request.Host,
            Port: request.Port,
            Username: request.Username,
            PasswordMasked: "••••••••",
            HasPassword: true,
            FromEmail: request.FromEmail,
            FromName: request.FromName,
            EnableSsl: true,
            IsEnabled: true,
            LastTestedAt: null,
            LastTestStatus: null
        );

        _smtpServiceMock
            .Setup(s => s.UpdateSmtpConfigAsync(tenantId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        var result = await _controller.UpdateSmtpConfig(tenantId, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<TenantSmtpConfigDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("smtp.sendgrid.net", response.Data!.Host);
    }

    [Fact]
    public async Task UpdateSmtpConfig_StarterTierQuotaExceeded_Returns402PaymentRequired()
    {
        var tenantId = Guid.NewGuid();
        var request = new UpdateTenantSmtpRequest
        {
            Host = "smtp.sendgrid.net",
            Port = 587,
            FromEmail = "test@bastos.cm"
        };

        _smtpServiceMock
            .Setup(s => s.UpdateSmtpConfigAsync(tenantId, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Custom SMTP Relay is an Enterprise plan feature."));

        var result = await _controller.UpdateSmtpConfig(tenantId, request, CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(402, statusResult.StatusCode);
        var response = Assert.IsType<ApiErrorResponse>(statusResult.Value);
        Assert.False(response.Success);
        Assert.Equal(ErrorCode.QuotaExceeded, response.Code);
        Assert.Contains("Enterprise plan feature", response.Message);
    }

    [Fact]
    public async Task TestSmtpConnection_WhenSuccessful_ReturnsOk()
    {
        var tenantId = Guid.NewGuid();
        var request = new TestSmtpRequest { RecipientEmail = "admin@bastos.cm" };
        var testResponse = new TestSmtpResponse(true, "Test email sent successfully in 120 ms.", 120);

        _smtpServiceMock
            .Setup(s => s.TestSmtpConnectionAsync(tenantId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testResponse);

        var result = await _controller.TestSmtpConnection(tenantId, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<TestSmtpResponse>>(okResult.Value);
        Assert.True(response.Success);
        Assert.True(response.Data!.Success);
        Assert.Equal(120, response.Data.LatencyMs);
    }

    [Fact]
    public async Task TestSmtpConnection_WhenFailed_ReturnsBadRequest()
    {
        var tenantId = Guid.NewGuid();
        var request = new TestSmtpRequest { RecipientEmail = "admin@bastos.cm" };
        var testResponse = new TestSmtpResponse(false, "SMTP connection failed: Host unreachable.", 0);

        _smtpServiceMock
            .Setup(s => s.TestSmtpConnectionAsync(tenantId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testResponse);

        var result = await _controller.TestSmtpConnection(tenantId, request, CancellationToken.None);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<TestSmtpResponse>>(badResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Host unreachable", response.Message);
    }

    [Fact]
    public async Task ResetSmtpConfig_WhenFound_ReturnsOk()
    {
        var tenantId = Guid.NewGuid();
        _smtpServiceMock
            .Setup(s => s.ResetSmtpConfigAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.ResetSmtpConfig(tenantId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(response.Success);
    }
}
