using Store.Models.DTOs.Inventory;

namespace Store.Models.Interfaces.Services;

public interface IWastageService
{
    /// <summary>List all wastage entries, optionally filtered by item or type.</summary>
    Task<List<WastageEntryDto>> GetAllAsync(Guid? itemId = null, string? wastageType = null);

    Task<WastageEntryDto?> GetByIdAsync(int id);

    /// <summary>
    /// Records a wastage event: decrements item stock and writes a StockMovement
    /// record of type Adjustment with a negative delta.
    /// </summary>
    Task<WastageEntryDto> RecordAsync(RecordWastageRequest request, Guid recordedByUserId);

    Task<bool> DeleteAsync(int id);
}
