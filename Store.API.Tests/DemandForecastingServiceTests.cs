using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Store.DbServices.Context;
using Store.DbServices.Services;
using Store.Models.DTOs.Notifications;
using Store.Models.Entities;
using Store.Models.Entities.Inventory;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 33 — Tests for <see cref="DemandForecastingService"/> (GAP-13).
/// Verifies sales velocity forecasting, safety stock calculation,
/// restock recommendations generation and broadcast, warehouse transfer conversion,
/// and supplier picking fallback logic.
/// </summary>
public class DemandForecastingServiceTests : IDisposable
{
    private readonly StoreDbContext _context;
    private readonly Mock<IRealTimeNotificationService> _mockNotifier;
    private readonly DemandForecastingService _service;

    public DemandForecastingServiceTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: $"ForecastingTests_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new StoreDbContext(options);
        _mockNotifier = new Mock<IRealTimeNotificationService>();
        _service = new DemandForecastingService(
            _context,
            _mockNotifier.Object,
            NullLogger<DemandForecastingService>.Instance);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task RunDemandForecastingAsync_CalculatesDailyVelocityAndGeneratesRecommendations()
    {
        // Arrange
        var branch = new Branch { BranchId = 1, Name = "Akwa Superstore", IsActive = true };
        var item = new Item { ItemId = Guid.NewGuid(), Name = "Basmati Rice 1kg", UnitPrice = 10m, CostPrice = 7m, IsActive = true };
        var branchStock = new BranchItemStock
        {
            BranchId = branch.BranchId,
            ItemId = item.ItemId,
            InStock = 10,
            LeadTimeDays = 7,
            ReorderQuantity = 100
        };

        // Create 30 days of sales history (e.g. 60 units sold = 2/day)
        var invoice = new Invoice
        {
            InvoiceId = Guid.NewGuid(),
            BranchId = branch.BranchId,
            TotalAmount = 600m,
            DateCreated = DateTime.UtcNow.AddDays(-10)
        };

        var sale = new Sale
        {
            SaleId = Guid.NewGuid(),
            InvoiceId = invoice.InvoiceId,
            ItemId = item.ItemId,
            ItemName = item.Name,
            Quantity = 60,
            UnitPrice = 10m,
            LineTotal = 600m
        };

        _context.Branches.Add(branch);
        _context.Items.Add(item);
        _context.BranchItemStocks.Add(branchStock);
        _context.Invoices.Add(invoice);
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        // Act
        await _service.RunDemandForecastingAsync();

        // Assert
        var recommendations = await _context.RestockRecommendations.ToListAsync();
        Assert.Single(recommendations);

        var rec = recommendations[0];
        Assert.Equal(branch.BranchId, rec.BranchId);
        Assert.Equal(item.ItemId, rec.ItemId);
        Assert.Equal(100, rec.RecommendedQuantity);
        Assert.Equal(RestockRecommendationStatus.Pending, rec.Status);
        Assert.Contains("Velocity: 2.0/day", rec.Reason);

        _mockNotifier.Verify(n => n.NotifyRestockRecommendationAsync(
            It.Is<RestockRecommendationNotificationDto>(d => d.RecommendationId == rec.RecommendationId && d.RecommendedQuantity == 100),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPendingRecommendationsAsync_FiltersByBranchId()
    {
        // Arrange
        var branch1 = new Branch { BranchId = 1, Name = "Branch 1", IsActive = true };
        var branch2 = new Branch { BranchId = 2, Name = "Branch 2", IsActive = true };
        var item1 = new Item { ItemId = Guid.NewGuid(), Name = "Item 1", UnitPrice = 10m, IsActive = true };
        var item2 = new Item { ItemId = Guid.NewGuid(), Name = "Item 2", UnitPrice = 20m, IsActive = true };

        _context.Branches.AddRange(branch1, branch2);
        _context.Items.AddRange(item1, item2);

        var recBranch1 = new RestockRecommendation
        {
            RecommendationId = Guid.NewGuid(),
            BranchId = branch1.BranchId,
            ItemId = item1.ItemId,
            RecommendedQuantity = 50,
            Status = RestockRecommendationStatus.Pending,
            Reason = "Low stock"
        };

        var recBranch2 = new RestockRecommendation
        {
            RecommendationId = Guid.NewGuid(),
            BranchId = branch2.BranchId,
            ItemId = item2.ItemId,
            RecommendedQuantity = 30,
            Status = RestockRecommendationStatus.Pending,
            Reason = "Low stock"
        };

        _context.RestockRecommendations.AddRange(recBranch1, recBranch2);
        await _context.SaveChangesAsync();

        // Act
        var resultAll = await _service.GetPendingRecommendationsAsync();
        var resultBranch1 = await _service.GetPendingRecommendationsAsync(branchId: 1);

        // Assert
        Assert.Equal(2, resultAll.Count);
        Assert.Single(resultBranch1);
        Assert.Equal(recBranch1.RecommendationId, resultBranch1[0].RecommendationId);
    }

    [Fact]
    public async Task ConvertToStockTransferAsync_CreatesStockTransfer_WhenWarehouseConfigured()
    {
        // Arrange
        var warehouse = new Branch { BranchId = 10, Name = "Central Warehouse", IsActive = true };
        var retailBranch = new Branch
        {
            BranchId = 1,
            Name = "Retail Branch",
            IsActive = true,
            SupplyingWarehouseId = warehouse.BranchId
        };

        var item = new Item { ItemId = Guid.NewGuid(), Name = "Corn Flakes", UnitPrice = 5m, IsActive = true };

        var rec = new RestockRecommendation
        {
            RecommendationId = Guid.NewGuid(),
            BranchId = retailBranch.BranchId,
            ItemId = item.ItemId,
            RecommendedQuantity = 45,
            Status = RestockRecommendationStatus.Pending,
            Reason = "Velocity threshold"
        };

        _context.Branches.AddRange(warehouse, retailBranch);
        _context.Items.Add(item);
        _context.RestockRecommendations.Add(rec);
        await _context.SaveChangesAsync();

        var userId = Guid.NewGuid();

        // Act
        var success = await _service.ConvertToStockTransferAsync(rec.RecommendationId, userId);

        // Assert
        Assert.True(success);

        var updatedRec = await _context.RestockRecommendations.FindAsync(rec.RecommendationId);
        Assert.NotNull(updatedRec);
        Assert.Equal(RestockRecommendationStatus.ConvertedToTransfer, updatedRec.Status);

        var transfer = await _context.StockTransfers.Include(t => t.Items).FirstOrDefaultAsync();
        Assert.NotNull(transfer);
        Assert.Equal(warehouse.BranchId, transfer.FromBranchId);
        Assert.Equal(retailBranch.BranchId, transfer.ToBranchId);
        Assert.Equal(StockTransferStatus.Requested, transfer.Status);
        Assert.Single(transfer.Items);
        Assert.Equal(item.ItemId, transfer.Items.First().ItemId);
        Assert.Equal(45, transfer.Items.First().RequestedQuantity);
    }

    [Fact]
    public async Task PickSupplierForItemAsync_PrefersPreferredSupplier_ThenOrderHistory()
    {
        // Arrange
        var preferredSupplier = new Supplier { SupplierId = Guid.NewGuid(), Name = "Preferred Dist" };
        var historySupplier = new Supplier { SupplierId = Guid.NewGuid(), Name = "Historical Dist" };

        var itemWithPreferred = new Item
        {
            ItemId = Guid.NewGuid(),
            Name = "Item 1",
            PreferredSupplierId = preferredSupplier.SupplierId,
            IsActive = true
        };

        var itemWithHistory = new Item
        {
            ItemId = Guid.NewGuid(),
            Name = "Item 2",
            IsActive = true
        };

        var pastOrder = new ItemsOrder
        {
            ItemsOrderId = Guid.NewGuid(),
            SupplierId = historySupplier.SupplierId,
            OrderNumber = "PO-HIST-1",
            Status = OrderStatus.Received,
            DateCreated = DateTime.UtcNow.AddDays(-20)
        };
        pastOrder.Items.Add(new OrderItem
        {
            OrderItemId = 1,
            ItemId = itemWithHistory.ItemId,
            ItemName = "Item 2",
            QuantityOrdered = 10
        });

        _context.Suppliers.AddRange(preferredSupplier, historySupplier);
        _context.Items.AddRange(itemWithPreferred, itemWithHistory);
        _context.ItemsOrders.Add(pastOrder);
        await _context.SaveChangesAsync();

        // Act
        var pick1 = await _service.PickSupplierForItemAsync(itemWithPreferred.ItemId);
        var pick2 = await _service.PickSupplierForItemAsync(itemWithHistory.ItemId);

        // Assert
        Assert.Equal(preferredSupplier.SupplierId, pick1.SupplierId);
        Assert.True(pick1.HasSupplier);
        Assert.Contains("Preferred", pick1.Reason);

        Assert.Equal(historySupplier.SupplierId, pick2.SupplierId);
        Assert.True(pick2.HasSupplier);
        Assert.Contains("history", pick2.Reason, StringComparison.OrdinalIgnoreCase);
    }
}
