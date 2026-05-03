using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Controllers;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.Interfaces.Services;

namespace Store.API.Tests;

public class ItemControllerTests
{
    private static ItemController CreateController(IItemService svc)
    {
        var controller = new ItemController(svc);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        var svc = new Mock<IItemService>();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ItemDto?)null);

        var controller = CreateController(svc.Object);
        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenItemExists()
    {
        var itemId = Guid.NewGuid();
        var dto = new ItemDto
        {
            ItemId = itemId,
            Name = "Test Item",
            UnitPrice = 500m,
            InStock = 10
        };

        var svc = new Mock<IItemService>();
        svc.Setup(s => s.GetByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var controller = CreateController(svc.Object);
        var result = await controller.GetById(itemId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<ItemDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(itemId, response.Data!.ItemId);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithPagedResult()
    {
        var items = new List<ItemDto>
        {
            new() { ItemId = Guid.NewGuid(), Name = "Item A", UnitPrice = 100m, InStock = 5 },
            new() { ItemId = Guid.NewGuid(), Name = "Item B", UnitPrice = 250m, InStock = 15 }
        };
        var pagedResult = new PagedResult<ItemDto>(items, items.Count, 1, 10);

        var svc = new Mock<IItemService>();
        svc.Setup(s => s.GetAllAsync(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var controller = CreateController(svc.Object);
        var result = await controller.GetAll(new PagedRequest { Page = 1, PageSize = 10 }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<PagedResult<ItemDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.TotalCount);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenItemNotFound()
    {
        var svc = new Mock<IItemService>();
        svc.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = CreateController(svc.Object);
        var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsOk_WhenItemDeletedSuccessfully()
    {
        var svc = new Mock<IItemService>();
        svc.Setup(s => s.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = CreateController(svc.Object);
        var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetLowStock_ReturnsOk_WithItemList()
    {
        var items = new List<ItemDto>
        {
            new() { ItemId = Guid.NewGuid(), Name = "Low Stock Item", UnitPrice = 300m, InStock = 1 }
        };

        var svc = new Mock<IItemService>();
        svc.Setup(s => s.GetLowStockAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var controller = CreateController(svc.Object);
        var result = await controller.GetLowStock(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
