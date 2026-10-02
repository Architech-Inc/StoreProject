using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Store.DbServices.Services;
using Store.Models.DTOs.Payments;
using Xunit;

namespace Store.API.Tests;

public class FlutterwavePaymentServiceTests
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }

    private static FlutterwavePaymentService BuildService(
        string secretKey = "FLWSECK_TEST-valid-secret-key-12345678",
        string secretHash = "flutterwave-webhook-secret-hash-12345",
        Func<HttpRequestMessage, HttpResponseMessage>? responder = null)
    {
        var opts = new FlutterwaveOptions
        {
            SecretKey = secretKey,
            PublicKey = "FLWPUBK_TEST-valid-public-key",
            SecretHash = secretHash,
            BaseUrl = "https://api.flutterwave.com/v3/"
        };

        var handler = responder != null
            ? new MockHttpMessageHandler(responder)
            : new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"success\",\"message\":\"ok\"}")
            });

        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.flutterwave.com/v3/") };
        return new FlutterwavePaymentService(http, NullLogger<FlutterwavePaymentService>.Instance, Options.Create(opts));
    }

    [Fact]
    public void VerifyWebhookHash_ValidHash_ReturnsTrue()
    {
        var secretHash = "my-secure-flw-secret-hash";
        var svc = BuildService(secretHash: secretHash);

        Assert.True(svc.VerifyWebhookHash(secretHash));
    }

    [Fact]
    public void VerifyWebhookHash_MismatchedHash_ReturnsFalse()
    {
        var svc = BuildService(secretHash: "expected-hash-1234");

        Assert.False(svc.VerifyWebhookHash("wrong-hash-5678"));
    }

    [Fact]
    public void VerifyWebhookHash_EmptyOrNull_ReturnsFalse()
    {
        var svc = BuildService(secretHash: "expected-hash-1234");

        Assert.False(svc.VerifyWebhookHash(null));
        Assert.False(svc.VerifyWebhookHash(string.Empty));
        Assert.False(svc.VerifyWebhookHash("   "));
    }

    [Fact]
    public void VerifyWebhookHash_UnconfiguredSecretHash_ReturnsFalse()
    {
        var svc = BuildService(secretHash: "");

        Assert.False(svc.VerifyWebhookHash("some-hash"));
    }

    [Fact]
    public async Task CreatePaymentLinkAsync_Unconfigured_ReturnsNotConfigured()
    {
        var svc = BuildService(secretKey: "");

        var resp = await svc.CreatePaymentLinkAsync(new FlutterwavePaymentLinkRequest
        {
            TxRef = "tx-123",
            Amount = 1000,
            Currency = "USD",
            CustomerEmail = "test@example.com"
        });

        Assert.False(resp.Success);
        Assert.Equal("NOT_CONFIGURED", resp.ResponseCode);
    }

    [Fact]
    public async Task CreatePaymentLinkAsync_SuccessfulResponse_ReturnsPaymentLink()
    {
        var svc = BuildService(responder: req =>
        {
            var json = JsonSerializer.Serialize(new
            {
                status = "success",
                message = "Hosted Link",
                data = new { link = "https://checkout.flutterwave.com/v3/hosted/pay/flw12345" }
            });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        });

        var resp = await svc.CreatePaymentLinkAsync(new FlutterwavePaymentLinkRequest
        {
            TxRef = "tx-123",
            Amount = 5000,
            Currency = "XAF",
            CustomerEmail = "test@example.com"
        });

        Assert.True(resp.Success);
        Assert.Equal("https://checkout.flutterwave.com/v3/hosted/pay/flw12345", resp.PaymentLink);
        Assert.Equal("tx-123", resp.TxRef);
    }

    [Fact]
    public async Task CreatePaymentLinkAsync_ApiError_ReturnsFailure()
    {
        var svc = BuildService(responder: req =>
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"status\":\"error\",\"message\":\"Currency not supported\"}")
            };
        });

        var resp = await svc.CreatePaymentLinkAsync(new FlutterwavePaymentLinkRequest
        {
            TxRef = "tx-err",
            Amount = 50,
            Currency = "INVALID",
            CustomerEmail = "test@example.com"
        });

        Assert.False(resp.Success);
        Assert.Equal("400", resp.ResponseCode);
    }

    [Fact]
    public async Task VerifyTransactionAsync_SuccessfulVerification_ReturnsParsedDto()
    {
        var tenantId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        var svc = BuildService(responder: req =>
        {
            var json = JsonSerializer.Serialize(new
            {
                status = "success",
                message = "Transaction fetched successfully",
                data = new
                {
                    id = 987654L,
                    tx_ref = "tx-987",
                    flw_ref = "FLW-MOCK-REF",
                    amount = 29900m,
                    charged_amount = 29900m,
                    currency = "NGN",
                    status = "successful",
                    payment_type = "card",
                    customer = new
                    {
                        id = 111L,
                        name = "Ada Lovelace",
                        email = "ada@example.com",
                        phone_number = "+2348000000000"
                    },
                    meta = new
                    {
                        tenant_id = tenantId.ToString(),
                        plan_id = "professional",
                        invoice_id = invoiceId.ToString()
                    }
                }
            });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        });

        var result = await svc.VerifyTransactionAsync(987654L);

        Assert.NotNull(result);
        Assert.Equal(987654L, result.Id);
        Assert.Equal("tx-987", result.TxRef);
        Assert.Equal(29900m, result.Amount);
        Assert.Equal("NGN", result.Currency);
        Assert.Equal("successful", result.Status);
        Assert.Equal("card", result.PaymentType);
        Assert.Equal("ada@example.com", result.CustomerEmail);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal("professional", result.PlanId);
        Assert.Equal(invoiceId, result.InvoiceId);
    }

    [Fact]
    public async Task VerifyTransactionAsync_FailedVerification_ReturnsNull()
    {
        var svc = BuildService(responder: req =>
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("{\"status\":\"error\",\"message\":\"No transaction was found\"}")
            };
        });

        var result = await svc.VerifyTransactionAsync(999999L);
        Assert.Null(result);
    }
}
