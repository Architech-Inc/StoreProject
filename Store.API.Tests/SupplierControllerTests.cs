using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Contracts;
using Store.API.Controllers;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Procurement;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

public class SupplierControllerTests
{
    private static SuppliersController CreateController(ISupplierService supplierService)
    {
        var controller = new SuppliersController(supplierService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetMetrics_ReturnsOkResult_WithSupplierMetricsDto()
    {
        var mockService = new Mock<ISupplierService>();
        var metrics = new SupplierMetricsDto
        {
            TotalSuppliers = 15,
            ActiveSuppliers = 12,
            TotalProcurementSpend = 5400000,
            OpenPurchaseOrdersCount = 4,
            PendingDeliveriesCount = 2
        };
        mockService.Setup(s => s.GetMetricsAsync()).ReturnsAsync(metrics);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetMetrics();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<SupplierMetricsDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(15, response.Data.TotalSuppliers);
        Assert.Equal(5400000, response.Data.TotalProcurementSpend);
    }

    [Fact]
    public async Task GetProfile_ReturnsOkResult_WhenSupplierExists()
    {
        var supplierId = Guid.NewGuid();
        var mockService = new Mock<ISupplierService>();
        var profile = new SupplierProfileDto
        {
            SupplierId = supplierId,
            Name = "Cameroon Beverage Co",
            TotalSpend = 2500000,
            TotalPurchaseOrdersCount = 8,
            OpenOrdersCount = 1
        };
        mockService.Setup(s => s.GetProfileAsync(supplierId)).ReturnsAsync(profile);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetProfile(supplierId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<SupplierProfileDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(supplierId, response.Data.SupplierId);
        Assert.Equal("Cameroon Beverage Co", response.Data.Name);
    }

    [Fact]
    public async Task GetProfile_ReturnsNotFound_WhenSupplierDoesNotExist()
    {
        var supplierId = Guid.NewGuid();
        var mockService = new Mock<ISupplierService>();
        mockService.Setup(s => s.GetProfileAsync(supplierId)).ReturnsAsync((SupplierProfileDto?)null);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetProfile(supplierId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsBadRequest_WhenSupplierCannotBeDeleted()
    {
        var supplierId = Guid.NewGuid();
        var mockService = new Mock<ISupplierService>();
        mockService.Setup(s => s.DeleteAsync(supplierId)).ReturnsAsync(false);

        var controller = CreateController(mockService.Object);
        var result = await controller.Delete(supplierId);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
        Assert.Contains("associated orders", response.Message);
    }
}
