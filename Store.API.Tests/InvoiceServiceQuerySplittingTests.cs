using Microsoft.EntityFrameworkCore;
using Moq;
using Store.DbServices.Context;
using Store.DbServices.Services;
using Store.DbServices.UnitOfWork;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Invoices;
using Store.Models.Entities;
using Store.Models.Entities.Contacts;
using Store.Models.Entities.HR;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 32 — Tests for <see cref="InvoiceService"/> verifying query splitting,
/// separated line-item and tender fetching, and lean aggregation queries (GAP-12).
/// </summary>
public class InvoiceServiceQuerySplittingTests : IDisposable
{
    private readonly StoreDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly Mock<IDiscountService> _mockDiscountService;
    private readonly Mock<IFinanceService> _mockFinanceService;
    private readonly Mock<IRealTimeNotificationService> _mockNotificationService;
    private readonly InvoiceService _service;

    public InvoiceServiceQuerySplittingTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: $"InvoiceServiceTests_{Guid.NewGuid()}")
            .Options;

        _context = new StoreDbContext(options);
        _uow = new UnitOfWork(_context);
        _mockDiscountService = new Mock<IDiscountService>();
        _mockFinanceService = new Mock<IFinanceService>();
        _mockNotificationService = new Mock<IRealTimeNotificationService>();

        _service = new InvoiceService(
            _uow,
            _mockDiscountService.Object,
            _mockFinanceService.Object,
            _mockNotificationService.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsInvoiceWithSeparatelyFetchedSalesAndTenders()
    {
        // Arrange
        var branch = new Branch { BranchId = 1, Name = "Main Branch", Code = "MB-01" };
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Doe",
            Segment = CustomerSegment.Vip
        };
        var employee = new Employee { EmployeeId = Guid.NewGuid(), FirstName = "Bob", LastName = "Cashier" };
        var user = new User { UserId = Guid.NewGuid(), Username = "bob_c", Employee = employee };

        var item1 = new Item { ItemId = Guid.NewGuid(), Name = "Organic Milk 1L", UnitPrice = 25m, InStock = 100, IsDeleted = false };
        var item2 = new Item { ItemId = Guid.NewGuid(), Name = "Whole Wheat Bread", UnitPrice = 15m, InStock = 100, IsDeleted = false };

        var invoice = new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            Branch = branch,
            Customer = customer,
            User = user,
            PaymentType = PaymentType.Cash,
            AmountTendered = 100m,
            TotalAmount = 80m,
            ChangeGiven = 20m,
            IsPaid = true,
            DateCreated = DateTime.UtcNow
        };

        var sale1 = new Sale
        {
            SaleId = Guid.NewGuid(),
            InvoiceId = invoice.InvoiceId,
            ItemId = item1.ItemId,
            ItemName = "Organic Milk 1L",
            UnitAbbreviation = "L",
            UnitPrice = 25m,
            Quantity = 2,
            LineTotal = 50m
        };

        var sale2 = new Sale
        {
            SaleId = Guid.NewGuid(),
            InvoiceId = invoice.InvoiceId,
            ItemId = item2.ItemId,
            ItemName = "Whole Wheat Bread",
            UnitAbbreviation = "pcs",
            UnitPrice = 15m,
            DiscountAmount = 2m,
            Quantity = 2,
            LineTotal = 26m
        };

        var tender = new InvoiceTender
        {
            InvoiceTenderId = 1,
            InvoiceId = invoice.InvoiceId,
            PaymentType = PaymentType.Cash,
            Amount = 100m,
            Reference = "CASH-001",
            DateCreated = DateTime.UtcNow
        };

        _context.Branches.Add(branch);
        _context.Customers.Add(customer);
        _context.Employees.Add(employee);
        _context.Users.Add(user);
        _context.Items.AddRange(item1, item2);
        _context.Invoices.Add(invoice);
        _context.Sales.AddRange(sale1, sale2);
        _context.InvoiceTenders.Add(tender);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(invoice.InvoiceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(invoice.InvoiceId, result.InvoiceId);
        Assert.Equal("Alice Doe", result.CustomerName);
        Assert.Equal("Bob Cashier", result.ProcessedBy);
        Assert.Equal("Main Branch", result.BranchName);
        Assert.True(result.IsPaid);
        Assert.Equal(2, result.Lines.Count());
        Assert.Equal(2, result.LinesCount);
        Assert.Contains(result.Lines, l => l.ItemName == "Organic Milk 1L" && l.Quantity == 2);
        Assert.Contains(result.Lines, l => l.ItemName == "Whole Wheat Bread" && l.DiscountAmount == 2m);
        Assert.Single(result.Tenders);
        Assert.Equal("Cash", result.Tenders.First().PaymentType);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenInvoiceDoesNotExist()
    {
        // Act
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPublicReceiptAsync_ReturnsReceiptWithCorrectCalculations()
    {
        // Arrange
        var item = new Item { ItemId = Guid.NewGuid(), Name = "Coffee Beans 500g", UnitPrice = 25m, InStock = 50, IsDeleted = false };
        var invoice = new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            PaymentType = PaymentType.MobileMoney,
            AmountTendered = 50m,
            TotalAmount = 50m,
            IsPaid = true,
            DateCreated = DateTime.UtcNow
        };

        var sale = new Sale
        {
            SaleId = Guid.NewGuid(),
            InvoiceId = invoice.InvoiceId,
            ItemId = item.ItemId,
            ItemName = "Coffee Beans 500g",
            UnitPrice = 25m,
            Quantity = 2,
            LineTotal = 50m
        };

        _context.Items.Add(item);
        _context.Invoices.Add(invoice);
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        // Act
        var receipt = await _service.GetPublicReceiptAsync(invoice.InvoiceId);

        // Assert
        Assert.NotNull(receipt);
        Assert.Equal(invoice.InvoiceId, receipt.InvoiceId);
        Assert.Equal(50m, receipt.TotalAmount);
        Assert.Equal("Completed", receipt.Status);
        Assert.Single(receipt.Lines);
        Assert.Equal("Coffee Beans 500g", receipt.Lines.First().ItemName);
        Assert.NotEmpty(receipt.VerificationSignature);
    }

    [Fact]
    public async Task GetAllAsync_PaginatesAndAssociatesLinesAndTendersSeparately()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            var item = new Item { ItemId = Guid.NewGuid(), Name = $"Item {i}", UnitPrice = i * 20m, InStock = 10, IsDeleted = false };
            var inv = new Invoice
            {
                InvoiceId = Guid.NewGuid(),
                PaymentType = PaymentType.Card,
                TotalAmount = i * 20m,
                AmountTendered = i * 20m,
                IsPaid = true,
                DateCreated = DateTime.UtcNow.AddMinutes(-i)
            };

            var sale = new Sale
            {
                SaleId = Guid.NewGuid(),
                InvoiceId = inv.InvoiceId,
                ItemId = item.ItemId,
                ItemName = $"Item for inv {i}",
                UnitPrice = i * 20m,
                Quantity = 1,
                LineTotal = i * 20m
            };

            var tender = new InvoiceTender
            {
                InvoiceTenderId = i,
                InvoiceId = inv.InvoiceId,
                PaymentType = PaymentType.Card,
                Amount = i * 20m,
                Reference = $"CARD-REF-{i}",
                DateCreated = DateTime.UtcNow
            };

            _context.Items.Add(item);
            _context.Invoices.Add(inv);
            _context.Sales.Add(sale);
            _context.InvoiceTenders.Add(tender);
        }
        await _context.SaveChangesAsync();

        var request = new InvoicePagedRequest
        {
            Page = 1,
            PageSize = 3
        };

        // Act
        var result = await _service.GetAllAsync(request);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.Items.Count());
        Assert.All(result.Items, item =>
        {
            Assert.NotEmpty(item.Lines);
            Assert.NotEmpty(item.Tenders);
            Assert.True(item.LinesCount > 0);
        });
    }

    [Fact]
    public async Task GetSummaryMetricsAsync_CalculatesAggregatesAccurately()
    {
        // Arrange
        var paidInv = new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            TotalAmount = 200m,
            AmountTendered = 200m,
            IsPaid = true,
            DateCreated = DateTime.UtcNow
        };
        var unpaidInv = new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            TotalAmount = 150m,
            AmountTendered = 50m,
            IsPaid = false,
            DateCreated = DateTime.UtcNow
        };

        _context.Invoices.AddRange(paidInv, unpaidInv);
        await _context.SaveChangesAsync();

        var request = new InvoicePagedRequest();

        // Act
        var metrics = await _service.GetSummaryMetricsAsync(request);

        // Assert
        Assert.Equal(2, metrics.TotalInvoicesCount);
        Assert.Equal(350m, metrics.GrossSales);
        Assert.Equal(250m, metrics.CollectedRevenue);
        Assert.Equal(100m, metrics.OutstandingReceivables);
        Assert.Equal(1, metrics.PaidCount);
        Assert.Equal(1, metrics.UnpaidCount);
        Assert.Equal(175m, metrics.AverageOrderValue);
    }
}
