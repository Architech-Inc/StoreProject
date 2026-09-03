using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.Entities;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;
using Store.Models.Utils;

namespace Store.DbServices.Services;

public class CountryService : ICountryService
{
    private readonly IUnitOfWork _uow;
    private static IReadOnlyList<CountryDto>? _cachedCountryDtos;
    private static readonly object _cacheLock = new();

    public CountryService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken ct = default)
    {
        if (_cachedCountryDtos != null) return _cachedCountryDtos;

        var entities = await _uow.Repository<Country>().Query()
            .AsNoTracking()
            .OrderBy(c => c.CountryId == 1 ? 0 : 1) // Cameroon always first
            .ThenBy(c => c.Name)
            .ToListAsync(ct);

        var dtos = entities.Select(c =>
        {
            var meta = PhoneNumberHelper.FindCountryByIso(c.IsoCode ?? "") 
                       ?? PhoneNumberHelper.FindCountryByDialCode(c.PhoneCode ?? "");
            return new CountryDto
            {
                CountryId = c.CountryId,
                Name = c.Name,
                IsoCode = c.IsoCode,
                PhoneCode = c.PhoneCode,
                FlagEmoji = meta?.FlagEmoji ?? "🌐"
            };
        }).ToList();

        lock (_cacheLock)
        {
            _cachedCountryDtos = dtos.AsReadOnly();
        }

        return _cachedCountryDtos;
    }

    public async Task<Country?> GetCountryByPhoneCodeAsync(string phoneCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(phoneCode)) return null;
        var clean = phoneCode.Trim();
        if (!clean.StartsWith("+")) clean = "+" + clean;

        return await _uow.Repository<Country>().Query()
            .FirstOrDefaultAsync(c => c.PhoneCode == clean, ct);
    }

    public async Task<Country?> GetCountryByIsoCodeAsync(string isoCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(isoCode)) return null;
        var clean = isoCode.Trim().ToUpperInvariant();

        return await _uow.Repository<Country>().Query()
            .FirstOrDefaultAsync(c => c.IsoCode == clean, ct);
    }

    public async Task<Country> GetDefaultCountryAsync(CancellationToken ct = default)
    {
        var country = await _uow.Repository<Country>().Query()
            .FirstOrDefaultAsync(c => c.CountryId == 1 || c.IsoCode == "CM", ct);

        if (country != null) return country;

        // Fallback first country or generate transient
        return await _uow.Repository<Country>().Query().FirstOrDefaultAsync(ct)
               ?? new Country { CountryId = 1, Name = "Cameroon", IsoCode = "CM", PhoneCode = "+237" };
    }
}
