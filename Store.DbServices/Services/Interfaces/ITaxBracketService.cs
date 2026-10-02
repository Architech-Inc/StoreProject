using Store.Models.Entities.HR;

namespace Store.DbServices.Services.Interfaces;

public interface ITaxBracketService
{
    Task<IEnumerable<TaxBracket>> GetAllAsync(CancellationToken ct = default);
    Task<TaxBracket?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<TaxBracket> CreateAsync(decimal minAmount, decimal? maxAmount, decimal taxPercentage, decimal fixedTaxAmount, bool isActive = true, CancellationToken ct = default);
    Task<TaxBracket?> UpdateAsync(int id, decimal minAmount, decimal? maxAmount, decimal taxPercentage, decimal fixedTaxAmount, bool isActive, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, Guid? deletedById = null, CancellationToken ct = default);
    Task<bool> RestoreAsync(int id, CancellationToken ct = default);
}
