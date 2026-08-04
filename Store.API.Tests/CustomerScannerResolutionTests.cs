using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Store.API.Controllers;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;
using Store.Models.DTOs.Employees;
using Store.Models.DTOs.Invoices;
using Store.Models.DTOs.Items;
using Store.Models.DTOs.Scanner;
using Store.Models.DTOs.Suppliers;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

public class CustomerScannerResolutionTests
{
    private static ScannerController CreateScannerController(
        IItemService itemSvc,
        IInvoiceService invSvc,
        IEmployeeService empSvc,
        ICustomerService custSvc,
        ISupplierService supSvc,
        IBatchService batchSvc)
    {
        var controller = new ScannerController(
            itemSvc,
            invSvc,
            empSvc,
            custSvc,
            supSvc,
            batchSvc,
            NullLogger<ScannerController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        return controller;
    }

    [Fact]
    public async Task ScanResolve_ResolvesCustomer_ByCustomerBarcode()
    {
        var custId = Guid.NewGuid();
        var custDto = new CustomerDto
        {
            CustomerId = custId,
            FirstName = "Paul",
            LastName = "Biya",
            Segment = CustomerSegment.Vip,
            LoyaltyTier = LoyaltyTier.Gold,
            LoyaltyPoints = 3500,
            LifetimeValue = 1200000,
            OutstandingBalance = 0,
            PrimaryPhone = "+237699112233",
            PrimaryEmail = "paul@example.com"
        };

        var itemSvc = new Mock<IItemService>();
        var invSvc = new Mock<IInvoiceService>();
        var empSvc = new Mock<IEmployeeService>();
        var supSvc = new Mock<ISupplierService>();
        var batchSvc = new Mock<IBatchService>();
        var custSvc = new Mock<ICustomerService>();

        itemSvc.Setup(s => s.GetAllAsync(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<ItemDto>(new List<ItemDto>(), 0, 1, 10));

        invSvc.Setup(s => s.GetAllAsync(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<InvoiceDto>(new List<InvoiceDto>(), 0, 1, 10));

        empSvc.Setup(s => s.GetAllAsync(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<EmployeeDto>(new List<EmployeeDto>(), 0, 1, 10));

        custSvc.Setup(s => s.GetByIdAsync(custId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(custDto);

        custSvc.Setup(s => s.GetAllAsync(It.IsAny<PagedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<CustomerDto>(new List<CustomerDto> { custDto }, 1, 1, 10));

        var controller = CreateScannerController(
            itemSvc.Object, invSvc.Object, empSvc.Object, custSvc.Object, supSvc.Object, batchSvc.Object);

        var response = await controller.Resolve($"CUST-{custId}", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response);
        var apiRes = Assert.IsType<ApiResponse<ScanResolutionResultDto>>(okResult.Value);
        Assert.NotNull(apiRes.Data);
        Assert.Equal(ScanEntityTypes.Customer, apiRes.Data.EntityType);
        Assert.Equal("Paul Biya", apiRes.Data.Title);
        Assert.Equal(custId.ToString(), apiRes.Data.EntityId);
        Assert.Contains(apiRes.Data.Actions, a => a.ActionId == "pos_sale");
        Assert.Contains(apiRes.Data.Actions, a => a.ActionId == "view_customer");
    }
}
