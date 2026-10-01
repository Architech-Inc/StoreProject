using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Store.DbServices.Context;
using Store.DbServices.Services;
using Store.Models.Entities;
using Store.Models.Enums;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 33 — Tests for <see cref="ProcurementAutomationService"/> (GAP-13).
/// Verifies evaluation of branch item stocks against reorder levels,
/// supplier grouping by PreferredSupplierId, and draft PO generation.
/// </summary>
public class ProcurementAutomationServiceTests : IDisposable
{
    private readonly StoreDbContext _context;
    private readonly ProcurementAutomationService _service;

    public ProcurementAutomationServiceTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: $"ProcurementAutoTests_{Guid.NewGuid()}")
            .Options;

        _context = new StoreDbContext(options);
        _service = new ProcurementAutomationService(_context, NullLogger<ProcurementAutomationService>.Instance);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task EvaluateInventoryThresholdsAsync_WhenStockBelowThreshold_GeneratesDraftPurchaseOrders()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = Guid.NewGuid(),
            Name = "Global Food Supplies"
        };

        var item1 = new Item
        {
            ItemId = Guid.NewGuid(),
            Name = "Jasmine Rice 5kg",
            CostPrice = 12m,
            UnitPrice = 18m,
            PreferredSupplierId = supplier.SupplierId,
            IsActive = true
        };

        var item2 = new Item
        {
            ItemId = Guid.NewGuid(),
            Name = "Sunflower Oil 2L",
            CostPrice = 8m,
            UnitPrice = 12m,
            PreferredSupplierId = supplier.SupplierId,
            IsActive = true
        };

        var branchStock1 = new BranchItemStock
        {
            BranchId = 1,
            ItemId = item1.ItemId,
            InStock = 5,
            ReorderLevel = 10,
            ReorderQuantity = 30,
            LeadTimeDays = 5,
            CustomCostPrice = 11.5m
        };

        var branchStock2 = new BranchItemStock
        {
            BranchId = 1,
            ItemId = item2.ItemId,
            InStock = 2,
            ReorderLevel = 15,
            ReorderQuantity = 40,
            LeadTimeDays = 3,
            CustomCostPrice = 8m
        };

        _context.Suppliers.Add(supplier);
        _context.Items.AddRange(item1, item2);
        _context.BranchItemStocks.AddRange(branchStock1, branchStock2);
        await _context.SaveChangesAsync();

        // Act
        var poCount = await _service.EvaluateInventoryThresholdsAsync();

        // Assert
        Assert.Equal(1, poCount);

        var po = await _context.PurchaseOrders.Include(p => p.Items).FirstOrDefaultAsync();
        Assert.NotNull(po);
        Assert.Equal(supplier.SupplierId, po.SupplierId);
        Assert.Equal(PurchaseOrderStatus.Draft, po.Status);
        Assert.Equal(2, po.Items.Count);
        Assert.Contains(po.Items, i => i.ItemId == item1.ItemId && i.OrderedQuantity == 30 && i.UnitCost == 11.5m);
        Assert.Contains(po.Items, i => i.ItemId == item2.ItemId && i.OrderedQuantity == 40 && i.UnitCost == 8m);
    }

    [Fact]
    public async Task EvaluateInventoryThresholdsAsync_WhenStockAboveThreshold_ReturnsZero()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = Guid.NewGuid(),
            Name = "Local Bakery"
        };

        var item = new Item
        {
            ItemId = Guid.NewGuid(),
            Name = "Baguette",
            PreferredSupplierId = supplier.SupplierId,
            IsActive = true
        };

        var branchStock = new BranchItemStock
        {
            BranchId = 1,
            ItemId = item.ItemId,
            InStock = 50,
            ReorderLevel = 20,
            ReorderQuantity = 30
        };

        _context.Suppliers.Add(supplier);
        _context.Items.Add(item);
        _context.BranchItemStocks.Add(branchStock);
        await _context.SaveChangesAsync();

        // Act
        var poCount = await _service.EvaluateInventoryThresholdsAsync();

        // Assert
        Assert.Equal(0, poCount);
        Assert.Empty(_context.PurchaseOrders);
    }

    [Fact]
    public async Task EvaluateInventoryThresholdsAsync_WhenItemInactive_IgnoresItem()
    {
        // Arrange
        var supplier = new Supplier
        {
            SupplierId = Guid.NewGuid(),
            Name = "Phased Out Supplier"
        };

        var item = new Item
        {
            ItemId = Guid.NewGuid(),
            Name = "Discontinued Soda",
            PreferredSupplierId = supplier.SupplierId,
            IsActive = false // INACTIVE
        };

        var branchStock = new BranchItemStock
        {
            BranchId = 1,
            ItemId = item.ItemId,
            InStock = 0,
            ReorderLevel = 10,
            ReorderQuantity = 20
        };

        _context.Suppliers.Add(supplier);
        _context.Items.Add(item);
        _context.BranchItemStocks.Add(branchStock);
        await _context.SaveChangesAsync();

        // Act
        var poCount = await _service.EvaluateInventoryThresholdsAsync();

        // Assert
        Assert.Equal(0, poCount);
        Assert.Empty(_context.PurchaseOrders);
    }
}
