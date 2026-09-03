namespace Store.Models.DTOs.Common;

public class CountryDto
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IsoCode { get; set; }
    public string? PhoneCode { get; set; }
    public string FlagEmoji { get; set; } = string.Empty;
}
