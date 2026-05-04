using Store.Models.DTOs.Loyalty;

namespace Store.Models.Interfaces.Services;

public interface ILoyaltyCampaignService
{
    Task<IEnumerable<LoyaltyCampaignDto>> GetAllAsync(bool? activeOnly, CancellationToken ct = default);
    Task<LoyaltyCampaignDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<LoyaltyCampaignDto> CreateAsync(CreateCampaignRequest request, CancellationToken ct = default);
    Task<LoyaltyCampaignDto?> UpdateAsync(int id, UpdateCampaignRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Returns active campaigns that match the given segment at the given point in time.</summary>
    Task<IEnumerable<LoyaltyCampaignDto>> GetActiveCampaignsForSegmentAsync(string segment, CancellationToken ct = default);
}
