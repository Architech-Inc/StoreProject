using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.DbServices.Services;
using Store.DbServices.UnitOfWork;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 34 — Unit tests for <see cref="UnitService"/> (GAP-14).
/// Verifies CRUD, sorting, duplicate abbreviation detection, and FK deletion guard.
/// </summary>
public class UnitServiceTests : IDisposable
{
    private readonly StoreDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly UnitService _service;

    public UnitServiceTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: $"UnitServiceTests_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new StoreDbContext(options);
        _uow = new UnitOfWork(_context);
        _service = new UnitService(_uow);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsUnitsOrderedByName()
    {
        // Arrange
        _context.Units.AddRange(
            new Unit { Name = "Kilogram", Abbreviation = "kg" },
            new Unit { Name = "Box", Abbreviation = "bx" },
            new Unit { Name = "Gram", Abbreviation = "g" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Box", result[0].Name);
        Assert.Equal("Gram", result[1].Name);
        Assert.Equal("Kilogram", result[2].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUnit_WhenExists()
    {
        // Arrange
        var unit = new Unit { Name = "Liter", Abbreviation = "L" };
        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(unit.UnitId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Liter", result!.Name);
        Assert.Equal("L", result.Abbreviation);
    }

    [Fact]
    public async Task CreateAsync_PersistsNewUnit()
    {
        // Act
        var result = await _service.CreateAsync("Pack of 6", "pk6", "Multipack");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.UnitId > 0);
        Assert.Equal("Pack of 6", result.Name);
        Assert.Equal("pk6", result.Abbreviation);

        var persisted = await _context.Units.FindAsync(result.UnitId);
        Assert.NotNull(persisted);
        Assert.Equal("pk6", persisted!.Abbreviation);
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_WhenAbbreviationDuplicate()
    {
        // Arrange
        _context.Units.Add(new Unit { Name = "Piece", Abbreviation = "pc" });
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync("Single Piece", "pc", "Duplicate abbr"));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesUnitDetails()
    {
        // Arrange
        var unit = new Unit { Name = "Milliliter", Abbreviation = "ml" };
        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        // Act
        var updated = await _service.UpdateAsync(unit.UnitId, "Millilitre", "mL", "Metric volume");

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("Millilitre", updated!.Name);
        Assert.Equal("mL", updated.Abbreviation);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsInvalidOperationException_WhenAbbreviationConflict()
    {
        // Arrange
        var u1 = new Unit { Name = "Dozen", Abbreviation = "dz" };
        var u2 = new Unit { Name = "Gross", Abbreviation = "gr" };
        _context.Units.AddRange(u1, u2);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(u2.UnitId, "Gross Updated", "dz", null));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_RemovesUnit_WhenNotInUse()
    {
        // Arrange
        var unit = new Unit { Name = "Custom Pack", Abbreviation = "cp" };
        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _service.DeleteAsync(unit.UnitId);

        // Assert
        Assert.True(deleted);
        var found = await _context.Units.FindAsync(unit.UnitId);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsInvalidOperationException_WhenItemsAssigned()
    {
        // Arrange
        var unit = new Unit { Name = "Bottle", Abbreviation = "btl" };
        _context.Units.Add(unit);
        await _context.SaveChangesAsync();

        var item = new Item
        {
            ItemId = Guid.NewGuid(),
            Name = "Mineral Water 500ml",
            UnitId = unit.UnitId,
            IsActive = true
        };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteAsync(unit.UnitId));

        Assert.Contains("Cannot delete unit because it is assigned to one or more items", ex.Message);
    }
}
