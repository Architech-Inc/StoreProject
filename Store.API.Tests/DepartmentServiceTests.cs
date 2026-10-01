using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.DbServices.Services;
using Store.DbServices.UnitOfWork;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 34 — Unit tests for <see cref="DepartmentService"/> (GAP-14).
/// Verifies CRUD, sorting, duplicate name detection, and employee FK deletion guard.
/// </summary>
public class DepartmentServiceTests : IDisposable
{
    private readonly StoreDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: $"DeptServiceTests_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new StoreDbContext(options);
        _uow = new UnitOfWork(_context);
        _service = new DepartmentService(_uow);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsDepartmentsOrderedByName()
    {
        // Arrange
        _context.Departments.AddRange(
            new Department { Name = "Sales" },
            new Department { Name = "Accounting" },
            new Department { Name = "Logistics" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Accounting", result[0].Name);
        Assert.Equal("Logistics", result[1].Name);
        Assert.Equal("Sales", result[2].Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDepartment_WhenExists()
    {
        // Arrange
        var dept = new Department { Name = "Marketing", Description = "Campaigns" };
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(dept.DepartmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Marketing", result!.Name);
    }

    [Fact]
    public async Task CreateAsync_PersistsNewDepartment()
    {
        // Act
        var result = await _service.CreateAsync("Human Resources", "People operations");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.DepartmentId > 0);
        Assert.Equal("Human Resources", result.Name);

        var persisted = await _context.Departments.FindAsync(result.DepartmentId);
        Assert.NotNull(persisted);
        Assert.Equal("Human Resources", persisted!.Name);
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_WhenNameDuplicate()
    {
        // Arrange
        _context.Departments.Add(new Department { Name = "Finance" });
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync("  Finance  ", null));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesDepartmentDetails()
    {
        // Arrange
        var dept = new Department { Name = "IT", Description = "Tech support" };
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        // Act
        var updated = await _service.UpdateAsync(dept.DepartmentId, "Information Technology", "Infrastructure and software");

        // Assert
        Assert.NotNull(updated);
        Assert.Equal("Information Technology", updated!.Name);
        Assert.Equal("Infrastructure and software", updated.Description);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsInvalidOperationException_WhenNameConflict()
    {
        // Arrange
        var d1 = new Department { Name = "Legal" };
        var d2 = new Department { Name = "Compliance" };
        _context.Departments.AddRange(d1, d2);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(d2.DepartmentId, "Legal", null));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_RemovesDepartment_WhenNotInUse()
    {
        // Arrange
        var dept = new Department { Name = "R&D" };
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _service.DeleteAsync(dept.DepartmentId);

        // Assert
        Assert.True(deleted);
        var found = await _context.Departments.FindAsync(dept.DepartmentId);
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsInvalidOperationException_WhenEmployeesAssigned()
    {
        // Arrange
        var dept = new Department { Name = "Operations" };
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();

        var emp = new Employee
        {
            EmployeeId = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            DepartmentId = dept.DepartmentId
        };
        _context.Employees.Add(emp);
        await _context.SaveChangesAsync();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteAsync(dept.DepartmentId));

        Assert.Contains("Cannot delete department because it is assigned to one or more employees", ex.Message);
    }
}
