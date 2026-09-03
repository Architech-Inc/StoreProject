using Store.Models.DTOs.Common;
using Store.Models.Entities;

namespace Store.Models.Interfaces.Services;

public interface ICountryService
{
    Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken ct = default);
    Task<Country?> GetCountryByPhoneCodeAsync(string phoneCode, CancellationToken ct = default);
    Task<Country?> GetCountryByIsoCodeAsync(string isoCode, CancellationToken ct = default);
    Task<Country> GetDefaultCountryAsync(CancellationToken ct = default);
}
