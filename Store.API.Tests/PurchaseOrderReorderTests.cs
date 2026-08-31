using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Controllers;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Procurement;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

public class PurchaseOrderReorderTests
{
    private static PurchaseOrdersController CreateController(IPurchaseOrderService poService, Guid? userId = null)
    {
        var controller = new PurchaseOrdersController(poService);
        var claims = new List<Claim>();
        if (userId.HasValue)
            claims.Add(new Claim("uid", userId.Value.ToString()));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };
        return controller;
    }

    [Fact]
    public async Task TriggerAutoReorder_ReturnsOk_WithReplenishmentSummary()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedResult = new AutomatedReorderResultDto
        {
            DepletedItemsDetected = 4,
            OrdersCreatedCount = 1,
            OrdersUpdatedCount = 0,
            TotalEstimatedValuationXaf = 125000m,
            GeneratedReferences = new List<string> { "PO-AUTO-20260831-554" },
            Message = "Auto-reorder evaluated 4 depleted items. Created 1 new POs."
        };

        var poServiceMock = new Mock<IPurchaseOrderService>();
        poServiceMock.Setup(s => s.ExecuteAutomatedReorderAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var controller = CreateController(poServiceMock.Object, userId);

        // Act
        var result = await controller.TriggerAutoReorder(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResp = Assert.IsType<ApiResponse<AutomatedReorderResultDto>>(okResult.Value);
        Assert.True(apiResp.Success);
        Assert.Equal(4, apiResp.Data!.DepletedItemsDetected);
        Assert.Equal(1, apiResp.Data.OrdersCreatedCount);
        Assert.Equal(125000m, apiResp.Data.TotalEstimatedValuationXaf);
        Assert.Contains("PO-AUTO-20260831-554", apiResp.Data.GeneratedReferences);
    }

    [Fact]
    public async Task TriggerAutoReorder_WhenStockIsAdequate_ReturnsZeroOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedResult = new AutomatedReorderResultDto
        {
            DepletedItemsDetected = 0,
            OrdersCreatedCount = 0,
            OrdersUpdatedCount = 0,
            TotalEstimatedValuationXaf = 0m,
            Message = "Inventory check complete: all items are adequately stocked above reorder levels."
        };

        var poServiceMock = new Mock<IPurchaseOrderService>();
        poServiceMock.Setup(s => s.ExecuteAutomatedReorderAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var controller = CreateController(poServiceMock.Object, userId);

        // Act
        var result = await controller.TriggerAutoReorder(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResp = Assert.IsType<ApiResponse<AutomatedReorderResultDto>>(okResult.Value);
        Assert.True(apiResp.Success);
        Assert.Equal(0, apiResp.Data!.DepletedItemsDetected);
        Assert.Equal(0, apiResp.Data.OrdersCreatedCount);
    }
}
