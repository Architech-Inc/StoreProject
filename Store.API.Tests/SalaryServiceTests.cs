using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.DbServices.Services;
using Store.DbServices.UnitOfWork;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 34 — Unit tests for <see cref="SalaryService"/> (GAP-14).
/// Verifies CRUD, sorting, duplicate grade detection, and employee FK deletion guard.
/// </summary>
public class SalaryServiceTests : IDisposable
{
    private readonly StoreDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly SalaryService _service;

    public SalaryServiceTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: $"SalaryServiceTests_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new StoreDbContext(options);
        _uow = new UnitOfWork(_context);
        _service = new SalaryService(_uow);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSalariesOrderedByGrade()
    {
        // Arrange
        _context.Salaries.AddRange(
            new Salary { Grade = "L3", BasicAmount = 3000m },
            new Salary { Grade = "L1", BasicAmount = 1000m },
            new Salary { Grade = "L2", BasicAmount = 2000m }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("L1", result[0].Grade);
        Assert.Equal("L2", result[1].Grade);
        Assert.Equal("L3", result[2].Grade);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsSalary_WhenExists()
    {
        // Arrange
        var salary = new Salary { Grade = "Executive", BasicAmount = 10000m, AllowanceAmount = 2500m };
        _context.Salaries.Add(salary);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(salary.SalaryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Executive", result!.Grade);
        Assert.Equal(10000m, result.BasicAmount);
        Assert.Equal(2500m, result.AllowanceAmount);
    }

    [Fact]
    public async Task CreateAsync_PersistsNewSalary()
    {
        // Act
        var result = await _service.CreateAsync("Mid-Level", 4500m, 500m, "Software Engineer II");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.SalaryId > 0);
        Assert.Equal("Mid-Level", result.Grade);
        Assert.Equal(4500m, result.BasicAmount);

        var persisted = await _context.Salaries.FindAsync(result.SalaryId);
        Assert.NotNull(persisted);
        Assert.Equal("Mid-Level", persisted!.Grade);
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_WhenGradeDuplicate()
    {
        // Arrange
        _context.Salaries.Add(new Salary { Grade = "Senior", BasicAmount = 8000m });
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync("  Senior  ", 9000m, null, null));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesSalaryDetails()
    {
        // Arrange
        var salary = new Salary { Grade = "Junior", BasicAmount = 2000m };
        _context.Salaries.Add(salary);
        await _context.SaveChangesAsync();

        // Act
        var updated = await _service.UpdateAsync(salary.SalaryId, "Associate", 2500m, 300m, "Entry role");

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("Associate", updated!.Grade);
        Assert.Equal(2500m, updated.BasicAmount);
        Assert.Equal(300m, updated.AllowanceAmount);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsInvalidOperationException_WhenGradeConflict()
    {
        // Arrange
        var s1 = new Salary { Grade = "Tier 1", BasicAmount = 1500m };
        var s2 = new Salary { Grade = "Tier 2", BasicAmount = 2500m };
        _context.Salaries.AddRange(s1, s2);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(s2.SalaryId, "Tier 1", 3000m, null, null));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesSalary_WhenNotInUse()
    {
        // Arrange
        var salary = new Salary { Grade = "Contractor", BasicAmount = 1200m };
        _context.Salaries.Add(salary);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _service.DeleteAsync(salary.SalaryId);

        // Assert
        Assert.True(deleted);
        var found = await _context.Salaries.FindAsync(salary.SalaryId);
        Assert.NotNull(found);
        Assert.True(found.IsDeleted);
        Assert.NotNull(found.DeletedAt);

        var active = await _context.Salaries.FirstOrDefaultAsync(s => s.SalaryId == salary.SalaryId);
        Assert.Null(active);
    }

    [Fact]
    public async Task RestoreAsync_RestoresSoftDeletedSalary()
    {
        // Arrange
        var salary = new Salary { Grade = "ArchivedGrade", BasicAmount = 1800m };
        _context.Salaries.Add(salary);
        await _context.SaveChangesAsync();
        await _service.DeleteAsync(salary.SalaryId);

        // Act
        var restored = await _service.RestoreAsync(salary.SalaryId);

        // Assert
        Assert.True(restored);
        var active = await _context.Salaries.FirstOrDefaultAsync(s => s.SalaryId == salary.SalaryId);
        Assert.NotNull(active);
        Assert.False(active.IsDeleted);
        Assert.Null(active.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsInvalidOperationException_WhenEmployeesAssigned()
    {
        // Arrange
        var salary = new Salary { Grade = "Principal", BasicAmount = 12000m };
        _context.Salaries.Add(salary);
        await _context.SaveChangesAsync();

        var emp = new Employee
        {
            EmployeeId = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            SalaryId = salary.SalaryId
        };
        _context.Employees.Add(emp);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteAsync(salary.SalaryId));

        Assert.Contains("Cannot delete salary grade because it is assigned to one or more employees", ex.Message);
    }
}
