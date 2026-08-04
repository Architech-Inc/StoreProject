using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Controllers;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

public class CustomerControllerTests
{
    private static CustomersController CreateController(ICustomerService custSvc, ILoyaltyService loyaltySvc, IUnitOfWork uow)
    {
        var controller = new CustomersController(custSvc, loyaltySvc, uow);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        var custSvc = new Mock<ICustomerService>();
        var loyaltySvc = new Mock<ILoyaltyService>();
        var uow = new Mock<IUnitOfWork>();

        custSvc.Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerDto?)null);

        var controller = CreateController(custSvc.Object, loyaltySvc.Object, uow.Object);
        var result = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenCustomerExists()
    {
        var custId = Guid.NewGuid();
        var custDto = new CustomerDto
        {
            CustomerId = custId,
            FirstName = "Jean",
            LastName = "Dupont",
            Segment = CustomerSegment.Vip,
            LoyaltyTier = LoyaltyTier.Gold,
            LoyaltyPoints = 1250,
            LifetimeValue = 450000,
            OutstandingBalance = 0,
            PrimaryPhone = "+237690000000",
            PrimaryEmail = "jean.dupont@example.com"
        };

        var custSvc = new Mock<ICustomerService>();
        var loyaltySvc = new Mock<ILoyaltyService>();
        var uow = new Mock<IUnitOfWork>();

        custSvc.Setup(s => s.GetByIdAsync(custId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(custDto);

        var controller = CreateController(custSvc.Object, loyaltySvc.Object, uow.Object);
        var result = await controller.GetById(custId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiRes = Assert.IsType<ApiResponse<CustomerDto>>(okResult.Value);
        Assert.NotNull(apiRes.Data);
        Assert.Equal("Jean Dupont", apiRes.Data.FullName);
        Assert.Equal(CustomerSegment.Vip, apiRes.Data.Segment);
        Assert.Equal(1250, apiRes.Data.LoyaltyPoints);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var req = new CreateCustomerRequest
        {
            FirstName = "Marie",
            LastName = "Claire",
            Phone = "+237670000000",
            Email = "marie@example.com",
            Segment = CustomerSegment.Standard
        };

        var custId = Guid.NewGuid();
        var createdDto = new CustomerDto
        {
            CustomerId = custId,
            FirstName = "Marie",
            LastName = "Claire",
            PrimaryPhone = "+237670000000",
            PrimaryEmail = "marie@example.com"
        };

        var custSvc = new Mock<ICustomerService>();
        var loyaltySvc = new Mock<ILoyaltyService>();
        var uow = new Mock<IUnitOfWork>();

        custSvc.Setup(s => s.CreateAsync(req, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        var controller = CreateController(custSvc.Object, loyaltySvc.Object, uow.Object);
        var result = await controller.Create(req, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(CustomersController.GetById), createdResult.ActionName);
    }
}
