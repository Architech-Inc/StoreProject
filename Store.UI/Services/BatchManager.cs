using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class BatchManager : IBatchManager
{
    private readonly IBatchService _batchService;

    public BatchManager(IBatchService batchService)
    {
        _batchService = batchService;
    }

    public async Task<BatchMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _batchService.GetBatchMetricsAsync(ct);

    public async Task<PagedResult<BatchDto>> GetBatchesPagedAsync(BatchFilterRequest request, CancellationToken ct = default)
        => await _batchService.GetBatchesPagedAsync(request, ct);

    public async Task<List<BatchDto>> GetExpiringAsync(int withinDays = 30, CancellationToken ct = default)
        => await _batchService.GetExpiringAsync(withinDays);

    public async Task<BatchDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _batchService.GetByIdAsync(id);

    public async Task<BatchDto> CreateAsync(CreateBatchRequest request, CancellationToken ct = default)
        => await _batchService.CreateAsync(request);

    public async Task<BatchDto?> UpdateAsync(Guid id, UpdateBatchRequest request, CancellationToken ct = default)
        => await _batchService.UpdateAsync(id, request);

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => await _batchService.DeleteAsync(id);

    public async Task<bool> WriteOffAsync(WriteOffBatchRequest request, CancellationToken ct = default)
        => await _batchService.WriteOffBatchAsync(request, null, ct);
}
