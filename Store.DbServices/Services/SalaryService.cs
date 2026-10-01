using Microsoft.EntityFrameworkCore;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class SalaryService : ISalaryService
{
    private readonly IUnitOfWork _uow;

    public SalaryService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<Salary>> GetAllAsync(CancellationToken ct = default) =>
        await _uow.Repository<Salary>().Query().AsNoTracking().OrderBy(s => s.Grade).ToListAsync(ct);

    public async Task<Salary?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _uow.Repository<Salary>().GetByIdAsync(id, ct);

    public async Task<Salary> CreateAsync(string grade, decimal basicAmount, decimal? allowance, string? description, CancellationToken ct = default)
    {
        var trimmedGrade = grade.Trim();
        if (await _uow.Repository<Salary>().ExistsAsync(s => s.Grade == trimmedGrade, ct))
            throw new InvalidOperationException($"Salary grade '{grade}' already exists.");

        var salary = new Salary
        {
            Grade = trimmedGrade,
            BasicAmount = basicAmount,
            AllowanceAmount = allowance,
            Description = description?.Trim()
        };

        await _uow.Repository<Salary>().AddAsync(salary, ct);
        await _uow.SaveChangesAsync(ct);
        return salary;
    }

    public async Task<Salary?> UpdateAsync(int id, string grade, decimal basicAmount, decimal? allowance, string? description, CancellationToken ct = default)
    {
        var salary = await _uow.Repository<Salary>().GetByIdAsync(id, ct);
        if (salary is null) return null;

        var trimmedGrade = grade.Trim();
        if (await _uow.Repository<Salary>().ExistsAsync(s => s.Grade == trimmedGrade && s.SalaryId != id, ct))
            throw new InvalidOperationException($"Salary grade '{grade}' already exists.");

        salary.Grade = trimmedGrade;
        salary.BasicAmount = basicAmount;
        salary.AllowanceAmount = allowance;
        salary.Description = description?.Trim();

        _uow.Repository<Salary>().Update(salary);
        await _uow.SaveChangesAsync(ct);
        return salary;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var salary = await _uow.Repository<Salary>().GetByIdAsync(id, ct);
        if (salary is null) return false;

        var hasEmployees = await _uow.Repository<Employee>().ExistsAsync(e => e.SalaryId == id, ct);
        if (hasEmployees)
            throw new InvalidOperationException("Cannot delete salary grade because it is assigned to one or more employees.");

        _uow.Repository<Salary>().Remove(salary);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
