using System;
using Store.DbServices.Services;
using Xunit;

namespace Store.API.Tests;

public class Argon2idPasswordHasherTests
{
    private readonly Argon2idPasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ProducesStandardModularCryptFormat()
    {
        // Arrange
        const string password = "SuperSecretPassword123!";

        // Act
        var hash = _hasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.StartsWith("$argon2id$v=19$m=19456,t=2,p=1$", hash);

        var parts = hash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(5, parts.Length);
        Assert.Equal("argon2id", parts[0]);
        Assert.Equal("v=19", parts[1]);
        Assert.Equal("m=19456,t=2,p=1", parts[2]);

        // Salt and hash base64 payloads
        var salt = Convert.FromBase64String(parts[3]);
        var hashBytes = Convert.FromBase64String(parts[4]);
        Assert.Equal(Argon2idPasswordHasher.DefaultSaltSize, salt.Length);
        Assert.Equal(Argon2idPasswordHasher.DefaultHashSize, hashBytes.Length);

        // Fits comfortably in varchar(256)
        Assert.True(hash.Length <= 256);
    }

    [Fact]
    public void VerifyPassword_ValidPassword_ReturnsTrue()
    {
        // Arrange
        const string password = "CorrectHorseBatteryStaple!";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        const string password = "CorrectPassword123!";
        const string wrongPassword = "WrongPassword123!";
        var hash = _hasher.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void VerifyPassword_LegacyBcryptHash_ReturnsTrue_AndNeedsRehash()
    {
        // Arrange: generate legacy BCrypt Enhanced hash (cost factor 12)
        const string password = "LegacyEmployeePassword123!";
        var legacyHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 12);

        // Act: verify legacy hash transparently through Argon2idPasswordHasher
        var isValid = _hasher.VerifyPassword(password, legacyHash);
        var needsRehash = _hasher.NeedsRehash(legacyHash);

        // Assert
        Assert.True(isValid);
        Assert.True(needsRehash);
    }

    [Fact]
    public void VerifyPassword_LegacyStandardBcryptHash_ReturnsTrue()
    {
        // Arrange: generate standard BCrypt hash
        const string password = "StandardBcryptPassword123!";
        var legacyHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Act
        var isValid = _hasher.VerifyPassword(password, legacyHash);
        var needsRehash = _hasher.NeedsRehash(legacyHash);

        // Assert
        Assert.True(isValid);
        Assert.True(needsRehash);
    }

    [Fact]
    public void VerifyPassword_LegacyBcryptHash_WrongPassword_ReturnsFalse()
    {
        // Arrange
        const string password = "LegacyPassword123!";
        const string wrongPassword = "IncorrectPassword123!";
        var legacyHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 12);

        // Act
        var isValid = _hasher.VerifyPassword(wrongPassword, legacyHash);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void NeedsRehash_Argon2idHash_ReturnsFalse()
    {
        // Arrange
        const string password = "ModernPassword123!";
        var hash = _hasher.HashPassword(password);

        // Act
        var needsRehash = _hasher.NeedsRehash(hash);

        // Assert
        Assert.False(needsRehash);
    }

    [Fact]
    public void HashPassword_GeneratesUniqueSaltsForIdenticalPasswords()
    {
        // Arrange
        const string password = "IdenticalPassword123!";

        // Act
        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.True(_hasher.VerifyPassword(password, hash1));
        Assert.True(_hasher.VerifyPassword(password, hash2));
    }

    [Theory]
    [InlineData(null, "hash")]
    [InlineData("", "hash")]
    [InlineData("password", null)]
    [InlineData("password", "")]
    [InlineData("password", "not_a_valid_hash")]
    [InlineData("password", "$argon2id$corrupt$payload")]
    public void VerifyPassword_MalformedOrNullInputs_FailsSafely(string? password, string? hash)
    {
        // Act
        var isValid = _hasher.VerifyPassword(password!, hash!);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void StaticHelpers_ExecuteCorrectly()
    {
        // Arrange
        const string password = "StaticHelperTest123!";

        // Act
        var hash = Argon2idPasswordHasher.Hash(password);
        var isValid = Argon2idPasswordHasher.Verify(password, hash);
        var isInvalid = Argon2idPasswordHasher.Verify("wrong", hash);

        // Assert
        Assert.True(isValid);
        Assert.False(isInvalid);
    }
}
