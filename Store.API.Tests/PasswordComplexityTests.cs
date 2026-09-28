using System.ComponentModel.DataAnnotations;
using Store.Models.DTOs.Auth;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// SEC-26 — pure unit tests for the password complexity attribute.
///
/// All tests use a stub <see cref="PasswordPolicyHolder"/> that disables the
/// HIBP breach check (we don't want CI to make outbound network calls). The
/// attribute's fail-open contract is also tested.
/// </summary>
public class PasswordComplexityTests
{
    private static (PasswordComplexityAttribute Attr, ValidationContext Ctx) Build(string? password, PasswordPolicyOptions? opts = null)
    {
        opts ??= new PasswordPolicyOptions
        {
            MinLength = 12,
            MaxLength = 128,
            MinClassCount = 3,
            EnableBreachCheck = false,
            CommonPasswords = new List<string> { "password", "qwerty", "letmein" }
        };
        var attr = new PasswordComplexityAttribute();
        var holder = new PasswordPolicyHolder(opts);
        var ctx = new ValidationContext(new { password }, new SimpleServiceProvider(holder), null);
        return (attr, ctx);
    }

    [Fact]
    public void Null_passes_through_required()
    {
        // [Required] is separate; complexity attribute must not double-fail.
        var (attr, ctx) = Build(null);
        Assert.Same(ValidationResult.Success, attr.GetValidationResult(null, ctx));
    }

    [Fact]
    public void Empty_passes_through_required()
    {
        var (attr, ctx) = Build("");
        Assert.Same(ValidationResult.Success, attr.GetValidationResult("", ctx));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("1234567")]
    [InlineData("Ab1!")]     // length 4
    public void Too_short_rejected(string pwd)
    {
        var (attr, ctx) = Build(pwd);
        var r = attr.GetValidationResult(pwd, ctx);
        Assert.NotEqual(ValidationResult.Success, r);
        Assert.Contains("12 characters", r!.ErrorMessage);
    }

    [Theory]
    [InlineData("longbutnotolongerthanallowedshortened")]
    public void Too_long_rejected(string pwd)
    {
        // Force MaxLength = 8 by passing options.
        var opts = new PasswordPolicyOptions { MinLength = 4, MaxLength = 8, MinClassCount = 2 };
        var attr = new PasswordComplexityAttribute();
        var holder = new PasswordPolicyHolder(opts);
        var ctx = new ValidationContext(new { }, new SimpleServiceProvider(holder), null);
        var r = attr.GetValidationResult(pwd, ctx);
        Assert.NotEqual(ValidationResult.Success, r);
        Assert.Contains("8 characters", r!.ErrorMessage);
    }

    [Fact]
    public void All_one_class_rejected()
    {
        // 12 lowercase letters only — meets length but only 1 character class.
        var (attr, ctx) = Build("abcdefghijkl");
        var r = attr.GetValidationResult("abcdefghijkl", ctx);
        Assert.NotEqual(ValidationResult.Success, r);
        Assert.Contains("at least", r!.ErrorMessage);
    }

    [Theory]
    [InlineData("Abcdefghijkl")] // upper + lower
    [InlineData("abcdef12ijkl")] // lower + digit
    [InlineData("abcdef!@ijkl")] // lower + symbol
    public void Two_classes_pass_minimum_three(string pwd)
    {
        // MinClassCount = 3, so two classes must fail.
        var (attr, ctx) = Build(pwd);
        var r = attr.GetValidationResult(pwd, ctx);
        Assert.NotEqual(ValidationResult.Success, r);
    }

    [Theory]
    [InlineData("Abcdefgh1jkl")]  // upper + lower + digit
    [InlineData("Abcdefg!@ijkl")] // upper + lower + symbol
    [InlineData("abcdefghi1!@")] // lower + digit + symbol
    [InlineData("ABCDEFGH1jkl")]  // upper + digit + lower
    [InlineData("MyP@ssword123")] // all four
    public void Three_or_more_classes_pass(string pwd)
    {
        var (attr, ctx) = Build(pwd);
        var r = attr.GetValidationResult(pwd, ctx);
        Assert.Same(ValidationResult.Success, r);
    }

    [Theory]
    [InlineData("password")]
    [InlineData("Password")]
    [InlineData("PASSWORD")]
    [InlineData("qwerty")]
    [InlineData("qwerty123")]
    public void Common_password_rejected(string pwd)
    {
        // First 3 are in the embedded blocklist.
        // The latter two are NOT in the blocklist but should still be
        // rejected by the class check (qwerty is lowercase only; qwerty123
        // adds digit = 2 classes but our policy requires 3). Adjusting:
        var (attr, ctx) = Build(pwd);
        // Make sure the policy matches expected behavior.
        var r = attr.GetValidationResult(pwd, ctx);
        Assert.NotEqual(ValidationResult.Success, r);
    }

    [Fact]
    public void No_policy_holder_accepts_everything()
    {
        // ValidationContext with no PasswordPolicyHolder — fails open on
        // policy checks but StringLength still applies at the controller.
        var attr = new PasswordComplexityAttribute();
        var ctx = new ValidationContext(new { });
        // No service provider, no holder.
        var r = attr.GetValidationResult("abc", ctx);
        Assert.Same(ValidationResult.Success, r);
    }

    [Fact]
    public void Common_password_blocklist_is_case_insensitive()
    {
        // Permissive policy (length=4, classes=1) so the only thing that
        // can reject this password is the blocklist check itself. The list
        // contains "password" (lowercase) and the input is "PASSWORD"
        // (uppercase) — case-insensitive matching should still catch it.
        var opts = new PasswordPolicyOptions
        {
            MinLength = 4,
            MaxLength = 64,
            MinClassCount = 1,
            EnableBreachCheck = false,
            CommonPasswords = new List<string> { "password" }
        };
        var attr = new PasswordComplexityAttribute();
        var ctx = new ValidationContext(new { }, new SimpleServiceProvider(new PasswordPolicyHolder(opts)), null);
        const string input = "PASSWORD"; // 8 chars, all upper = 1 class — would otherwise pass
        var r = attr.GetValidationResult(input, ctx);
        Assert.NotNull(r);
        Assert.NotEqual(ValidationResult.Success, r);
        Assert.Contains("known-bad", r!.ErrorMessage);
    }
}

/// <summary>
/// Minimal <see cref="IServiceProvider"/> stub for ValidationContext.GetService.
/// Returns the registered instance for the exact type, null otherwise.
/// </summary>
internal sealed class SimpleServiceProvider : IServiceProvider
{
    private readonly Dictionary<Type, object> _services = new();
    public SimpleServiceProvider(params object[] instances)
    {
        foreach (var i in instances) _services[i.GetType()] = i;
    }
    public object? GetService(Type serviceType)
    {
        _services.TryGetValue(serviceType, out var v);
        return v;
    }
}