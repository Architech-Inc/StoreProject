using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Controllers;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

public class PublicReceiptTests
{
    private static InvoicesController CreateController(IInvoiceService invoiceService)
    {
        var controller = new InvoicesController(invoiceService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetPublicReceipt_ReturnsOk_WithSanitizedReceiptData()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var expectedReceipt = new PublicReceiptDto
        {
            InvoiceId = invoiceId,
            StoreName = "ClexAn Supermarket & Retail",
            StoreTaxId = "M052014125896P",
            BranchName = "Main Store - Akwa Douala",
            CashierName = "Marie",
            CustomerName = "Jean Dupont",
            SubtotalAmount = 25000m,
            DiscountAmount = 2500m,
            TotalAmount = 22500m,
            AmountTendered = 25000m,
            ChangeGiven = 2500m,
            PaymentMethod = "Cash",
            Status = "Completed",
            DateCreated = DateTime.UtcNow,
            VerificationSignature = "E9A4B3C2D1F08765",
            Lines = new List<PublicReceiptLineDto>
            {
                new() { ItemName = "Basmati Rice 5kg", Quantity = 2, UnitPrice = 10000m, LineTotal = 20000m },
                new() { ItemName = "Olive Oil 1L", Quantity = 1, UnitPrice = 5000m, LineTotal = 5000m }
            }
        };

        var invoiceServiceMock = new Mock<IInvoiceService>();
        invoiceServiceMock.Setup(s => s.GetPublicReceiptAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReceipt);

        var controller = CreateController(invoiceServiceMock.Object);

        // Act
        var result = await controller.GetPublicReceipt(invoiceId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResp = Assert.IsType<ApiResponse<PublicReceiptDto>>(okResult.Value);
        Assert.True(apiResp.Success);
        Assert.Equal(invoiceId, apiResp.Data!.InvoiceId);
        Assert.Equal("ClexAn Supermarket & Retail", apiResp.Data.StoreName);
        Assert.Equal("M052014125896P", apiResp.Data.StoreTaxId);
        Assert.Equal("E9A4B3C2D1F08765", apiResp.Data.VerificationSignature);
        Assert.Equal(2, apiResp.Data.Lines.Count);
        Assert.Equal(22500m, apiResp.Data.TotalAmount);
    }

    [Fact]
    public async Task GetPublicReceipt_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var invoiceId = Guid.NewGuid();
        var invoiceServiceMock = new Mock<IInvoiceService>();
        invoiceServiceMock.Setup(s => s.GetPublicReceiptAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PublicReceiptDto?)null);

        var controller = CreateController(invoiceServiceMock.Object);

        // Act
        var result = await controller.GetPublicReceipt(invoiceId, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var apiResp = Assert.IsType<ApiResponse<object>>(notFoundResult.Value);
        Assert.False(apiResp.Success);
    }
}
