using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Loyalty;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class LoyaltyCampaignService : ILoyaltyCampaignService
{
    private readonly IUnitOfWork _uow;

    public LoyaltyCampaignService(IUnitOfWork uow) => _uow = uow;

    public async Task<IEnumerable<LoyaltyCampaignDto>> GetAllAsync(bool? activeOnly, CancellationToken ct = default)
    {
        var query = _uow.Repository<LoyaltyCampaign>().Query().AsNoTracking();
        if (activeOnly == true) query = query.Where(c => c.IsActive);
        var campaigns = await query.OrderBy(c => c.StartDate).ToListAsync(ct);
        return campaigns.Select(MapToDto);
    }

    public async Task<LoyaltyCampaignDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var campaign = await _uow.Repository<LoyaltyCampaign>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.LoyaltyCampaignId == id, ct);
        return campaign is null ? null : MapToDto(campaign);
    }

    public async Task<LoyaltyCampaignDto> CreateAsync(CreateCampaignRequest request, CancellationToken ct = default)
    {
        var campaign = new LoyaltyCampaign
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CampaignType = request.CampaignType,
            TargetSegment = request.TargetSegment,
            MultiplierFactor = request.CampaignType == LoyaltyCampaignType.PointMultiplier ? request.MultiplierFactor : 1m,
            BonusPoints = request.CampaignType == LoyaltyCampaignType.FixedBonusPoints ? request.BonusPoints : 0,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive
        };

        await _uow.Repository<LoyaltyCampaign>().AddAsync(campaign, ct);
        await _uow.SaveChangesAsync(ct);
        return MapToDto(campaign);
    }

    public async Task<LoyaltyCampaignDto?> UpdateAsync(int id, UpdateCampaignRequest request, CancellationToken ct = default)
    {
        var campaign = await _uow.Repository<LoyaltyCampaign>().Query()
            .FirstOrDefaultAsync(c => c.LoyaltyCampaignId == id, ct);

        if (campaign is null) return null;

        if (!string.IsNullOrWhiteSpace(request.Name)) campaign.Name = request.Name.Trim();
        if (request.Description is not null) campaign.Description = request.Description.Trim();
        if (request.CampaignType.HasValue) campaign.CampaignType = request.CampaignType.Value;
        if (request.TargetSegment.HasValue) campaign.TargetSegment = request.TargetSegment;
        if (request.MultiplierFactor.HasValue) campaign.MultiplierFactor = request.MultiplierFactor.Value;
        if (request.BonusPoints.HasValue) campaign.BonusPoints = request.BonusPoints.Value;
        if (request.StartDate.HasValue) campaign.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) campaign.EndDate = request.EndDate.Value;
        if (request.IsActive.HasValue) campaign.IsActive = request.IsActive.Value;

        _uow.Repository<LoyaltyCampaign>().Update(campaign);
        await _uow.SaveChangesAsync(ct);
        return MapToDto(campaign);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var campaign = await _uow.Repository<LoyaltyCampaign>().Query()
            .FirstOrDefaultAsync(c => c.LoyaltyCampaignId == id, ct);

        if (campaign is null) return false;

        _uow.Repository<LoyaltyCampaign>().Remove(campaign);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<LoyaltyCampaignDto>> GetActiveCampaignsForSegmentAsync(string segment, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        if (!Enum.TryParse<CustomerSegment>(segment, ignoreCase: true, out var seg))
            return Enumerable.Empty<LoyaltyCampaignDto>();

        var campaigns = await _uow.Repository<LoyaltyCampaign>().Query()
            .AsNoTracking()
            .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now &&
                        (c.TargetSegment == null || c.TargetSegment == seg))
            .ToListAsync(ct);

        return campaigns.Select(MapToDto);
    }

    private static LoyaltyCampaignDto MapToDto(LoyaltyCampaign c)
    {
        var now = DateTime.UtcNow;
        return new LoyaltyCampaignDto
        {
            LoyaltyCampaignId = c.LoyaltyCampaignId,
            Name = c.Name,
            Description = c.Description,
            CampaignType = c.CampaignType.ToString(),
            TargetSegment = c.TargetSegment?.ToString(),
            MultiplierFactor = c.MultiplierFactor,
            BonusPoints = c.BonusPoints,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            IsActive = c.IsActive,
            IsRunning = c.IsActive && c.StartDate <= now && c.EndDate >= now
        };
    }
}
