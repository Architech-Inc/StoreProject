using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Repositories;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _uow;

    public CustomerService(IUnitOfWork uow) => _uow = uow;

    public async Task<CustomerDto?> GetByIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);

        return customer is null ? null : MapToDto(customer);
    }

    public async Task<PagedResult<CustomerDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<Customer>().Query().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(c => c.FirstName.Contains(request.SearchTerm) ||
                                     c.LastName.Contains(request.SearchTerm));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => MapToDto(c))
            .ToListAsync(ct);

        return new PagedResult<CustomerDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            MiddleName = request.MiddleName?.Trim(),
            LastName = request.LastName.Trim(),
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            Notes = request.Notes?.Trim()
        };

        await _uow.Repository<Customer>().AddAsync(customer, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToDto(customer);
    }

    public async Task<CustomerDto?> UpdateAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>().Query()
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);

        if (customer is null) return null;

        if (!string.IsNullOrWhiteSpace(request.FirstName)) customer.FirstName = request.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(request.LastName)) customer.LastName = request.LastName.Trim();
        if (request.MiddleName is not null) customer.MiddleName = request.MiddleName.Trim();
        if (request.Gender.HasValue) customer.Gender = request.Gender.Value;
        if (request.DateOfBirth.HasValue) customer.DateOfBirth = request.DateOfBirth;
        if (request.Notes is not null) customer.Notes = request.Notes.Trim();

        _uow.Repository<Customer>().Update(customer);
        await _uow.SaveChangesAsync(ct);

        return MapToDto(customer);
    }

    public async Task<bool> DeleteAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>().Query()
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);

        if (customer is null) return false;

        _uow.Repository<Customer>().Remove(customer);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private static CustomerDto MapToDto(Customer c) => new()
    {
        CustomerId = c.CustomerId,
        FirstName = c.FirstName,
        MiddleName = c.MiddleName,
        LastName = c.LastName,
        Gender = c.Gender,
        DateOfBirth = c.DateOfBirth,
        Notes = c.Notes,
        ImagePath = c.ImagePath,
        DateCreated = c.DateCreated
    };
}
