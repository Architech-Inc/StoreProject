using Store.Models.Utils;
using Xunit;

namespace Store.API.Tests;

public class PhoneNumberHelperTests
{
    [Fact]
    public void Parse_CameroonLocalNumber_ResolvesToCameroonWithDefaults()
    {
        var result = PhoneNumberHelper.Parse("678787878", "+237");

        Assert.True(result.IsValid);
        Assert.Equal("+237", result.DialCode);
        Assert.Equal("678787878", result.NationalNumber);
        Assert.Equal("+237678787878", result.E164Number);
        Assert.Equal("CM", result.IsoCode);
        Assert.Equal("🇨🇲", result.FlagEmoji);
        Assert.Equal("+237 6 78 78 78 78", result.FormattedNumber);
    }

    [Fact]
    public void Parse_CameroonInternationalWithPlus_ResolvesCorrectly()
    {
        var result = PhoneNumberHelper.Parse("+237 6 99 11 22 33");

        Assert.True(result.IsValid);
        Assert.Equal("+237", result.DialCode);
        Assert.Equal("699112233", result.NationalNumber);
        Assert.Equal("+237699112233", result.E164Number);
        Assert.Equal("CM", result.IsoCode);
        Assert.Equal("+237 6 99 11 22 33", result.FormattedNumber);
    }

    [Fact]
    public void Parse_CameroonInternationalWithDoubleZero_ResolvesCorrectly()
    {
        var result = PhoneNumberHelper.Parse("00237699112233");

        Assert.True(result.IsValid);
        Assert.Equal("+237", result.DialCode);
        Assert.Equal("699112233", result.NationalNumber);
        Assert.Equal("+237699112233", result.E164Number);
        Assert.Equal("CM", result.IsoCode);
    }

    [Fact]
    public void Parse_NigeriaNumber_ResolvesToNigeria()
    {
        var result = PhoneNumberHelper.Parse("+234 803 123 4567");

        Assert.True(result.IsValid);
        Assert.Equal("+234", result.DialCode);
        Assert.Equal("8031234567", result.NationalNumber);
        Assert.Equal("+2348031234567", result.E164Number);
        Assert.Equal("NG", result.IsoCode);
        Assert.Equal("🇳🇬", result.FlagEmoji);
    }

    [Fact]
    public void Parse_FrenchNumber_ResolvesToFrance()
    {
        var result = PhoneNumberHelper.Parse("+33 6 12 34 56 78");

        Assert.True(result.IsValid);
        Assert.Equal("+33", result.DialCode);
        Assert.Equal("612345678", result.NationalNumber);
        Assert.Equal("+33612345678", result.E164Number);
        Assert.Equal("FR", result.IsoCode);
        Assert.Equal("🇫🇷", result.FlagEmoji);
    }

    [Fact]
    public void Parse_USNumber_ResolvesToUS()
    {
        var result = PhoneNumberHelper.Parse("+1 415 555 2671");

        Assert.True(result.IsValid);
        Assert.Equal("+1", result.DialCode);
        Assert.Equal("4155552671", result.NationalNumber);
        Assert.Equal("+14155552671", result.E164Number);
        Assert.Equal("US", result.IsoCode);
        Assert.Equal("🇺🇸", result.FlagEmoji);
    }

    [Fact]
    public void Parse_EmptyOrNull_ReturnsInvalid()
    {
        var result = PhoneNumberHelper.Parse("");
        Assert.False(result.IsValid);
        Assert.Equal(string.Empty, result.NationalNumber);

        var nullResult = PhoneNumberHelper.Parse(null);
        Assert.False(nullResult.IsValid);
    }
}
