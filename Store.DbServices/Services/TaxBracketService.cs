using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.DbServices.Services.Interfaces;
using Store.Models.Entities.HR;

namespace Store.DbServices.Services;

public class TaxBracketService : ITaxBracketService
{
    private readonly StoreDbContext _context;

    public TaxBracketService(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaxBracket>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.TaxBrackets
            .OrderBy(t => t.MinAmount)
            .ToListAsync(ct);
    }

    public async Task<TaxBracket?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.TaxBrackets.FirstOrDefaultAsync(t => t.TaxBracketId == id, ct);
    }

    public async Task<TaxBracket> CreateAsync(decimal minAmount, decimal? maxAmount, decimal taxPercentage, decimal fixedTaxAmount, bool isActive = true, CancellationToken ct = default)
    {
        var bracket = new TaxBracket
        {
            MinAmount = minAmount,
            MaxAmount = maxAmount,
            TaxPercentage = taxPercentage,
            FixedTaxAmount = fixedTaxAmount,
            IsActive = isActive
        };

        _context.TaxBrackets.Add(bracket);
        await _context.SaveChangesAsync(ct);
        return bracket;
    }

    public async Task<TaxBracket?> UpdateAsync(int id, decimal minAmount, decimal? maxAmount, decimal taxPercentage, decimal fixedTaxAmount, bool isActive, CancellationToken ct = default)
    {
        var bracket = await _context.TaxBrackets.FirstOrDefaultAsync(t => t.TaxBracketId == id, ct);
        if (bracket is null) return null;

        bracket.MinAmount = minAmount;
        bracket.MaxAmount = maxAmount;
        bracket.TaxPercentage = taxPercentage;
        bracket.FixedTaxAmount = fixedTaxAmount;
        bracket.IsActive = isActive;

        await _context.SaveChangesAsync(ct);
        return bracket;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var bracket = await _context.TaxBrackets.FirstOrDefaultAsync(t => t.TaxBracketId == id, ct);
        if (bracket is null) return false;

        _context.TaxBrackets.Remove(bracket);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
