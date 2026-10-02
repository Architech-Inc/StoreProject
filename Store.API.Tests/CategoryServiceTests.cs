using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.DbServices.Services;
using Store.DbServices.UnitOfWork;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 34 — Unit tests for <see cref="CategoryService"/> (GAP-14).
/// Verifies CRUD, sorting, duplicate name detection on create/update,
/// and foreign-key dependency rejection on delete.
/// </summary>
public class CategoryServiceTests : IDisposable
{
    private readonly StoreDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: $"CategoryServiceTests_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new StoreDbContext(options);
        _uow = new UnitOfWork(_context);
        _service = new CategoryService(_uow);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCategoriesOrderedByName()
    {
        // Arrange
        _context.Categories.AddRange(
            new Category { Name = "Produce", Description = "Fresh vegetables" },
            new Category { Name = "Bakery", Description = "Fresh bread" },
            new Category { Name = "Dairy", Description = "Milk and cheese" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Bakery", result[0].Name);
        Assert.Equal("Dairy", result[1].Name);
        Assert.Equal("Produce", result[2].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCategory_WhenExists()
    {
        // Arrange
        var cat = new Category { Name = "Beverages", Description = "Drinks and juices" };
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(cat.CategoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Beverages", result!.Name);
    }

    [Fact]
    public async Task CreateAsync_PersistsNewCategory()
    {
        // Act
        var result = await _service.CreateAsync("Snacks", "Chips & cookies", "thumb.jpg", "full.jpg");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.CategoryId > 0);
        Assert.Equal("Snacks", result.Name);
        Assert.Equal("Chips & cookies", result.Description);

        var persisted = await _context.Categories.FindAsync(result.CategoryId);
        Assert.NotNull(persisted);
        Assert.Equal("Snacks", persisted!.Name);
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_WhenNameDuplicate()
    {
        // Arrange
        _context.Categories.Add(new Category { Name = "Frozen" });
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync("  Frozen  ", "Duplicate name"));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCategoryDetails()
    {
        // Arrange
        var cat = new Category { Name = "Pantry", Description = "Canned goods" };
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        // Act
        var updated = await _service.UpdateAsync(cat.CategoryId, "Pantry & Canned", "Expanded pantry", "new_thumb.jpg", "new_full.jpg");

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("Pantry & Canned", updated!.Name);
        Assert.Equal("Expanded pantry", updated.Description);
        Assert.Equal("new_thumb.jpg", updated.ThumbnailUrl);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsInvalidOperationException_WhenUpdatingToExistingName()
    {
        // Arrange
        var cat1 = new Category { Name = "Meat" };
        var cat2 = new Category { Name = "Seafood" };
        _context.Categories.AddRange(cat1, cat2);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(cat2.CategoryId, "Meat", "Rename conflict"));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesCategory_WhenNotInUse()
    {
        // Arrange
        var cat = new Category { Name = "Seasonal" };
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _service.DeleteAsync(cat.CategoryId);

        // Assert
        Assert.True(deleted);
        var found = await _context.Categories.FindAsync(cat.CategoryId);
        Assert.NotNull(found);
        Assert.True(found.IsDeleted);
        Assert.NotNull(found.DeletedAt);

        var active = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == cat.CategoryId);
        Assert.Null(active);
    }

    [Fact]
    public async Task RestoreAsync_RestoresSoftDeletedCategory()
    {
        // Arrange
        var cat = new Category { Name = "ArchivedSeasonal" };
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();
        await _service.DeleteAsync(cat.CategoryId);

        // Act
        var restored = await _service.RestoreAsync(cat.CategoryId);

        // Assert
        Assert.True(restored);
        var active = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == cat.CategoryId);
        Assert.NotNull(active);
        Assert.False(active.IsDeleted);
        Assert.Null(active.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsInvalidOperationException_WhenItemsAssigned()
    {
        // Arrange
        var cat = new Category { Name = "Household" };
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();

        var item = new Item
        {
            ItemId = Guid.NewGuid(),
            Name = "Detergent 1L",
            CategoryId = cat.CategoryId,
            IsActive = true
        };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteAsync(cat.CategoryId));

        Assert.Contains("Cannot delete category because it is assigned to one or more items", ex.Message);
    }
}
