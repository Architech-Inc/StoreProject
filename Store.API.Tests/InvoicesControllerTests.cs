using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Controllers;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace Store.API.Tests;

public class InvoicesControllerTests
{
    private static InvoicesController CreateController(IInvoiceService svc, Guid? userId = null)
    {
        var controller = new InvoicesController(svc);
        var claims = new List<Claim>();
        if (userId.HasValue)
            claims.Add(new Claim("uid", userId.Value.ToString()));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenInvoiceDoesNotExist()
    {
        var svc = new Mock<IInvoiceService>();
        svc.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InvoiceDto?)null);

        var controller = CreateController(svc.Object);
        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenInvoiceExists()
    {
        var invoiceId = Guid.NewGuid();
        var dto = new InvoiceDto
        {
            InvoiceId = invoiceId,
            TotalAmount = 1500m,
            IsPaid = true
        };

        var svc = new Mock<IInvoiceService>();
        svc.Setup(s => s.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var controller = CreateController(svc.Object);
        var result = await controller.GetById(invoiceId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<InvoiceDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(invoiceId, response.Data!.InvoiceId);
    }

    [Fact]
    public async Task Void_ReturnsNotFound_WhenInvoiceNotFoundOrAlreadyVoided()
    {
        var svc = new Mock<IInvoiceService>();
        svc.Setup(s => s.VoidInvoiceAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = CreateController(svc.Object, Guid.NewGuid());
        var result = await controller.Void(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Void_ReturnsOk_WhenInvoiceVoidedSuccessfully()
    {
        var svc = new Mock<IInvoiceService>();
        svc.Setup(s => s.VoidInvoiceAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = CreateController(svc.Object, Guid.NewGuid());
        var result = await controller.Void(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenInvoiceCreatedSuccessfully()
    {
        var userId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var dto = new InvoiceDto { InvoiceId = invoiceId, TotalAmount = 2000m };
        var request = new CreateInvoiceRequest
        {
            PaymentType = PaymentType.Cash,
            AmountTendered = 2000m,
            Lines = new[] { new CreateSaleLineRequest { ItemId = Guid.NewGuid(), Quantity = 2 } }
        };

        var svc = new Mock<IInvoiceService>();
        svc.Setup(s => s.CreateInvoiceAsync(request, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var controller = CreateController(svc.Object, userId);
        var result = await controller.Create(request, CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task GetAll_ReturnsPagedResult()
    {
        var pagedRequest = new PagedRequest { Page = 1, PageSize = 10 };
        var pagedResult = new PagedResult<InvoiceDto>(new List<InvoiceDto>(), 0, 1, 10);

        var svc = new Mock<IInvoiceService>();
        svc.Setup(s => s.GetAllAsync(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var controller = CreateController(svc.Object);
        var result = await controller.GetAll(pagedRequest, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
