using System.Text.RegularExpressions;

namespace Store.Models.Utils;

public record CountryPhoneMeta(string IsoCode, string Name, string DialCode, string FlagEmoji, int TypicalLength = 9);

public class ParsedPhoneNumber
{
    public string RawInput { get; init; } = string.Empty;
    public string DialCode { get; init; } = "+237";
    public string NationalNumber { get; init; } = string.Empty;
    public string E164Number { get; init; } = string.Empty;
    public string FormattedNumber { get; init; } = string.Empty;
    public string IsoCode { get; init; } = "CM";
    public string CountryName { get; init; } = "Cameroon";
    public string FlagEmoji { get; init; } = "🇨🇲";
    public bool IsValid { get; init; }

    public override string ToString() => string.IsNullOrWhiteSpace(FormattedNumber) ? RawInput : FormattedNumber;
}

public static class PhoneNumberHelper
{
    private static readonly List<CountryPhoneMeta> SupportedCountries = new()
    {
        new("CM", "Cameroon", "+237", "🇨🇲", 9),
        new("NG", "Nigeria", "+234", "🇳🇬", 10),
        new("GA", "Gabon", "+241", "🇬🇦", 8),
        new("CG", "Congo", "+242", "🇨🇬", 9),
        new("CD", "Democratic Republic of the Congo", "+243", "🇨🇩", 9),
        new("TD", "Chad", "+235", "🇹🇩", 8),
        new("CF", "Central African Republic", "+236", "🇨🇫", 8),
        new("GQ", "Equatorial Guinea", "+240", "🇬🇶", 9),
        new("CI", "Ivory Coast", "+225", "🇨🇮", 10),
        new("GH", "Ghana", "+233", "🇬🇭", 9),
        new("SN", "Senegal", "+221", "🇸🇳", 9),
        new("ML", "Mali", "+223", "🇲🇱", 8),
        new("BF", "Burkina Faso", "+226", "🇧🇫", 8),
        new("BJ", "Benin", "+229", "🇧🇯", 8),
        new("TG", "Togo", "+228", "🇹🇬", 8),
        new("NE", "Niger", "+227", "🇳🇪", 8),
        new("KE", "Kenya", "+254", "🇰🇪", 9),
        new("RW", "Rwanda", "+250", "🇷🇼", 9),
        new("UG", "Uganda", "+256", "🇺🇬", 9),
        new("TZ", "Tanzania", "+255", "🇹🇿", 9),
        new("ZA", "South Africa", "+27", "🇿🇦", 9),
        new("ET", "Ethiopia", "+251", "🇪🇹", 9),
        new("EG", "Egypt", "+20", "🇪🇬", 10),
        new("MA", "Morocco", "+212", "🇲🇦", 9),
        new("DZ", "Algeria", "+213", "🇩🇿", 9),
        new("TN", "Tunisia", "+216", "🇹🇳", 8),
        new("FR", "France", "+33", "🇫🇷", 9),
        new("GB", "United Kingdom", "+44", "🇬🇧", 10),
        new("US", "United States", "+1", "🇺🇸", 10),
        new("CA", "Canada", "+1", "🇨🇦", 10),
        new("DE", "Germany", "+49", "🇩🇪", 10),
        new("BE", "Belgium", "+32", "🇧🇪", 9),
        new("CH", "Switzerland", "+41", "🇨🇭", 9),
        new("ES", "Spain", "+34", "🇪🇸", 9),
        new("IT", "Italy", "+39", "🇮🇹", 10),
        new("NL", "Netherlands", "+31", "🇳🇱", 9),
        new("PT", "Portugal", "+351", "🇵🇹", 9),
        new("TR", "Turkey", "+90", "🇹🇷", 10),
        new("AE", "United Arab Emirates", "+971", "🇦🇪", 9),
        new("SA", "Saudi Arabia", "+966", "🇸🇦", 9),
        new("CN", "China", "+86", "🇨🇳", 11),
        new("IN", "India", "+91", "🇮🇳", 10),
        new("BR", "Brazil", "+55", "🇧🇷", 11)
    };

    public static IReadOnlyList<CountryPhoneMeta> GetAllCountries() => SupportedCountries;

    public static CountryPhoneMeta? FindCountryByDialCode(string dialCode)
    {
        var clean = CleanDialCode(dialCode);
        return SupportedCountries.FirstOrDefault(c => c.DialCode == clean);
    }

    public static CountryPhoneMeta? FindCountryByIso(string isoCode)
    {
        if (string.IsNullOrWhiteSpace(isoCode)) return null;
        var clean = isoCode.Trim().ToUpperInvariant();
        return SupportedCountries.FirstOrDefault(c => c.IsoCode == clean);
    }

    public static ParsedPhoneNumber Parse(string? rawPhone, string defaultDialCode = "+237")
    {
        if (string.IsNullOrWhiteSpace(rawPhone))
        {
            return new ParsedPhoneNumber
            {
                RawInput = string.Empty,
                DialCode = CleanDialCode(defaultDialCode),
                NationalNumber = string.Empty,
                E164Number = string.Empty,
                FormattedNumber = string.Empty,
                IsValid = false
            };
        }

        var trimmed = rawPhone.Trim();
        var cleanDigits = Regex.Replace(trimmed, @"[^\d+]", "");

        // Convert leading 00 to +
        if (cleanDigits.StartsWith("00"))
        {
            cleanDigits = "+" + cleanDigits[2..];
        }

        string dialCode = CleanDialCode(defaultDialCode);
        string nationalNumber = "";
        CountryPhoneMeta? matchedCountry = null;

        if (cleanDigits.StartsWith("+"))
        {
            // Try matching longest dial code first
            var dialCodesDescending = SupportedCountries
                .OrderByDescending(c => c.DialCode.Length)
                .ToList();

            foreach (var c in dialCodesDescending)
            {
                if (cleanDigits.StartsWith(c.DialCode))
                {
                    matchedCountry = c;
                    dialCode = c.DialCode;
                    nationalNumber = cleanDigits[c.DialCode.Length..];
                    break;
                }
            }

            // Fallback if not in predefined list
            if (matchedCountry == null)
            {
                // Take up to 4 digits after + as dial code
                var match = Regex.Match(cleanDigits, @"^\+(\d{1,4})(\d+)$");
                if (match.Success)
                {
                    dialCode = "+" + match.Groups[1].Value;
                    nationalNumber = match.Groups[2].Value;
                }
                else
                {
                    nationalNumber = cleanDigits.TrimStart('+');
                }
            }
        }
        else
        {
            // No leading +: check if starts with known dial code without plus (e.g. 237678...)
            var dialCodesDescending = SupportedCountries
                .OrderByDescending(c => c.DialCode.Length)
                .ToList();

            bool foundWithoutPlus = false;
            foreach (var c in dialCodesDescending)
            {
                var codeWithoutPlus = c.DialCode.TrimStart('+');
                if (cleanDigits.StartsWith(codeWithoutPlus) && cleanDigits.Length > codeWithoutPlus.Length + 4)
                {
                    matchedCountry = c;
                    dialCode = c.DialCode;
                    nationalNumber = cleanDigits[codeWithoutPlus.Length..];
                    foundWithoutPlus = true;
                    break;
                }
            }

            if (!foundWithoutPlus)
            {
                matchedCountry = FindCountryByDialCode(dialCode);
                nationalNumber = cleanDigits.TrimStart('0'); // remove domestic trunk prefix if present
            }
        }

        matchedCountry ??= FindCountryByDialCode(dialCode) ?? new CountryPhoneMeta("XX", "Unknown", dialCode, "🌐", 9);

        // Sanitize national number
        nationalNumber = Regex.Replace(nationalNumber, @"\D", "");
        var e164 = $"{dialCode}{nationalNumber}";

        var formatted = FormatNationalNumber(dialCode, nationalNumber, matchedCountry.IsoCode);
        bool isValid = nationalNumber.Length >= 6 && nationalNumber.Length <= 15;

        return new ParsedPhoneNumber
        {
            RawInput = trimmed,
            DialCode = dialCode,
            NationalNumber = nationalNumber,
            E164Number = e164,
            FormattedNumber = formatted,
            IsoCode = matchedCountry.IsoCode,
            CountryName = matchedCountry.Name,
            FlagEmoji = matchedCountry.FlagEmoji,
            IsValid = isValid
        };
    }

    public static string Format(string? rawOrE164, string defaultDialCode = "+237")
    {
        var parsed = Parse(rawOrE164, defaultDialCode);
        return parsed.FormattedNumber;
    }

    private static string CleanDialCode(string dialCode)
    {
        if (string.IsNullOrWhiteSpace(dialCode)) return "+237";
        var cleaned = Regex.Replace(dialCode, @"[^\d]", "");
        return "+" + cleaned;
    }

    private static string FormatNationalNumber(string dialCode, string national, string iso)
    {
        if (string.IsNullOrEmpty(national)) return dialCode;

        // Specialized formatting for Cameroon (e.g. 6 78 78 78 78 or 2 33 44 55 66)
        if (iso == "CM" && national.Length == 9)
        {
            return $"{dialCode} {national[0]} {national.Substring(1, 2)} {national.Substring(3, 2)} {national.Substring(5, 2)} {national.Substring(7, 2)}";
        }

        // France / Ivory Coast / Senegal (pairs)
        if ((iso == "FR" || iso == "CI" || iso == "SN") && national.Length >= 9)
        {
            var parts = new List<string>();
            for (int i = 0; i < national.Length; i += 2)
            {
                var len = Math.Min(2, national.Length - i);
                parts.Add(national.Substring(i, len));
            }
            return $"{dialCode} {string.Join(" ", parts)}";
        }

        // US / Canada / Nigeria / UK (groups of 3-3-4 or similar)
        if (national.Length == 10)
        {
            return $"{dialCode} {national[..3]} {national.Substring(3, 3)} {national[6..]}";
        }

        // General fallback chunking into groups of 3
        var chunks = new List<string>();
        for (int i = 0; i < national.Length; i += 3)
        {
            var len = Math.Min(3, national.Length - i);
            chunks.Add(national.Substring(i, len));
        }
        return $"{dialCode} {string.Join(" ", chunks)}";
    }
}
