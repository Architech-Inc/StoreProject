using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.DbServices.Services;
using Store.DbServices.UnitOfWork;
using Store.Models.Entities;
using Store.Models.Entities.HR;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 48 — Comprehensive Soft-Delete & Reversible Recovery Tests (GAP-18 Follow-Up).
/// Verifies soft-delete flag, deleted timestamp, deleted-by audit trail,
/// EF Core global query filter exclusion, and complete reversible restoration across entities.
/// </summary>
public class SoftDeleteAndRestoreTests : IDisposable
{
    private readonly StoreDbContext _context;
    private readonly IUnitOfWork _uow;

    public SoftDeleteAndRestoreTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: $"SoftDeleteTests_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new StoreDbContext(options);
        _uow = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task ItemService_DeleteAndRestore_MaintainsAuditTrailAndQueryFilter()
    {
        var service = new ItemService(_uow);
        var itemId = Guid.NewGuid();
        var deleterId = Guid.NewGuid();

        var item = new Item
        {
            ItemId = itemId,
            Name = "Soft Deletable Juice",
            CostPrice = 500,
            UnitPrice = 800,
            InStock = 50,
            IsActive = true
        };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        // 1. Delete
        var deleted = await service.DeleteAsync(itemId, deleterId);
        Assert.True(deleted);

        // Tracked entity state
        var tracked = await _context.Items.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.ItemId == itemId);
        Assert.NotNull(tracked);
        Assert.True(tracked.IsDeleted);
        Assert.NotNull(tracked.DeletedAt);
        Assert.Equal(deleterId, tracked.DeletedById);
        Assert.False(tracked.IsActive);

        // Global query filter excludes it
        var queryable = await _context.Items.FirstOrDefaultAsync(i => i.ItemId == itemId);
        Assert.Null(queryable);

        // 2. Restore
        var restored = await service.RestoreAsync(itemId);
        Assert.True(restored);

        var restoredItem = await _context.Items.FirstOrDefaultAsync(i => i.ItemId == itemId);
        Assert.NotNull(restoredItem);
        Assert.False(restoredItem.IsDeleted);
        Assert.Null(restoredItem.DeletedAt);
        Assert.Null(restoredItem.DeletedById);
        Assert.True(restoredItem.IsActive);
    }

    [Fact]
    public async Task BatchService_DeleteAndRestore_SetsFlagsAndRestoresProperly()
    {
        var service = new BatchService(_uow);
        var batchId = Guid.NewGuid();
        var deleterId = Guid.NewGuid();

        var batch = new Batch
        {
            BatchId = batchId,
            BatchNumber = "BATCH-SOFT-001",
            ItemId = Guid.NewGuid(),
            Quantity = 100,
            CostPrice = 200,
            ReceivedDate = DateTime.UtcNow
        };
        _context.Batches.Add(batch);
        await _context.SaveChangesAsync();

        // Delete
        var deleted = await service.DeleteAsync(batchId, deleterId);
        Assert.True(deleted);

        var tracked = await _context.Batches.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.BatchId == batchId);
        Assert.NotNull(tracked);
        Assert.True(tracked.IsDeleted);
        Assert.NotNull(tracked.DeletedAt);
        Assert.Equal(deleterId, tracked.DeletedById);

        // Query filter exclusion
        var filtered = await _context.Batches.FirstOrDefaultAsync(b => b.BatchId == batchId);
        Assert.Null(filtered);

        // Restore
        var restored = await service.RestoreAsync(batchId);
        Assert.True(restored);

        var activeBatch = await _context.Batches.FirstOrDefaultAsync(b => b.BatchId == batchId);
        Assert.NotNull(activeBatch);
        Assert.False(activeBatch.IsDeleted);
        Assert.Null(activeBatch.DeletedAt);
        Assert.Null(activeBatch.DeletedById);
    }

    [Fact]
    public async Task DiscountService_DeleteAndRestore_SetsFlagsAndRestoresProperly()
    {
        var service = new DiscountService(_uow);
        var deleterId = Guid.NewGuid();

        var discount = new Discount
        {
            Name = "Flash Promo",
            DiscountType = DiscountType.Percentage,
            Percentage = 15m,
            IsActive = true
        };
        _context.Discounts.Add(discount);
        await _context.SaveChangesAsync();
        var discountId = discount.DiscountId;

        // Delete
        var deleted = await service.DeleteAsync(discountId, deleterId);
        Assert.True(deleted);

        var tracked = await _context.Discounts.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.DiscountId == discountId);
        Assert.NotNull(tracked);
        Assert.True(tracked.IsDeleted);
        Assert.NotNull(tracked.DeletedAt);
        Assert.Equal(deleterId, tracked.DeletedById);
        Assert.False(tracked.IsActive);

        // Query filter exclusion
        var filtered = await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountId == discountId);
        Assert.Null(filtered);

        // Restore
        var restored = await service.RestoreAsync(discountId);
        Assert.True(restored);

        var activeDiscount = await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountId == discountId);
        Assert.NotNull(activeDiscount);
        Assert.False(activeDiscount.IsDeleted);
        Assert.Null(activeDiscount.DeletedAt);
        Assert.Null(activeDiscount.DeletedById);
        Assert.True(activeDiscount.IsActive);
    }

    [Fact]
    public async Task TaxBracketService_DeleteAndRestore_SetsFlagsAndRestoresProperly()
    {
        var service = new TaxBracketService(_context);
        var deleterId = Guid.NewGuid();

        var bracket = new TaxBracket
        {
            MinAmount = 100000m,
            MaxAmount = 250000m,
            TaxPercentage = 10m,
            FixedTaxAmount = 0m,
            IsActive = true
        };
        _context.TaxBrackets.Add(bracket);
        await _context.SaveChangesAsync();
        var bracketId = bracket.TaxBracketId;

        // Delete
        var deleted = await service.DeleteAsync(bracketId, deleterId);
        Assert.True(deleted);

        var tracked = await _context.TaxBrackets.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.TaxBracketId == bracketId);
        Assert.NotNull(tracked);
        Assert.True(tracked.IsDeleted);
        Assert.NotNull(tracked.DeletedAt);
        Assert.Equal(deleterId, tracked.DeletedById);
        Assert.False(tracked.IsActive);

        // Query filter exclusion
        var filtered = await _context.TaxBrackets.FirstOrDefaultAsync(t => t.TaxBracketId == bracketId);
        Assert.Null(filtered);

        // Restore
        var restored = await service.RestoreAsync(bracketId);
        Assert.True(restored);

        var activeBracket = await _context.TaxBrackets.FirstOrDefaultAsync(t => t.TaxBracketId == bracketId);
        Assert.NotNull(activeBracket);
        Assert.False(activeBracket.IsDeleted);
        Assert.Null(activeBracket.DeletedAt);
        Assert.Null(activeBracket.DeletedById);
        Assert.True(activeBracket.IsActive);
    }

    [Fact]
    public async Task WastageService_DeleteAndRestore_SetsFlagsAndRestoresProperly()
    {
        var service = new WastageService(_uow);
        var deleterId = Guid.NewGuid();

        var entry = new WastageEntry
        {
            ItemId = Guid.NewGuid(),
            WastageType = WastageType.Damage,
            Quantity = 5,
            Notes = "Broken bottle"
        };
        _context.WastageEntries.Add(entry);
        await _context.SaveChangesAsync();
        var entryId = entry.WastageEntryId;

        // Delete
        var deleted = await service.DeleteAsync(entryId, deleterId);
        Assert.True(deleted);

        var tracked = await _context.WastageEntries.IgnoreQueryFilters().FirstOrDefaultAsync(w => w.WastageEntryId == entryId);
        Assert.NotNull(tracked);
        Assert.True(tracked.IsDeleted);
        Assert.NotNull(tracked.DeletedAt);
        Assert.Equal(deleterId, tracked.DeletedById);

        // Query filter exclusion
        var filtered = await _context.WastageEntries.FirstOrDefaultAsync(w => w.WastageEntryId == entryId);
        Assert.Null(filtered);

        // Restore
        var restored = await service.RestoreAsync(entryId);
        Assert.True(restored);

        var activeEntry = await _context.WastageEntries.FirstOrDefaultAsync(w => w.WastageEntryId == entryId);
        Assert.NotNull(activeEntry);
        Assert.False(activeEntry.IsDeleted);
        Assert.Null(activeEntry.DeletedAt);
        Assert.Null(activeEntry.DeletedById);
    }

    [Fact]
    public async Task EmployeeService_DeleteAndRestore_UpdatesStatusAndSoftDeleteFlags()
    {
        var service = new EmployeeService(_uow);
        var employeeId = Guid.NewGuid();
        var deleterId = Guid.NewGuid();

        var emp = new Employee
        {
            EmployeeId = employeeId,
            FirstName = "Albert",
            LastName = "Camus",
            Status = EmployeeStatus.Active
        };
        _context.Employees.Add(emp);
        await _context.SaveChangesAsync();

        // Delete
        var deleted = await service.DeleteAsync(employeeId, deleterId);
        Assert.True(deleted);

        var tracked = await _context.Employees.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        Assert.NotNull(tracked);
        Assert.True(tracked.IsDeleted);
        Assert.NotNull(tracked.DeletedAt);
        Assert.Equal(deleterId, tracked.DeletedById);
        Assert.Equal(EmployeeStatus.Fired, tracked.Status);

        // Query filter exclusion
        var filtered = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        Assert.Null(filtered);

        // Restore
        var restored = await service.RestoreAsync(employeeId);
        Assert.True(restored);

        var activeEmp = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        Assert.NotNull(activeEmp);
        Assert.False(activeEmp.IsDeleted);
        Assert.Null(activeEmp.DeletedAt);
        Assert.Null(activeEmp.DeletedById);
        Assert.Equal(EmployeeStatus.Active, activeEmp.Status);
    }
}
