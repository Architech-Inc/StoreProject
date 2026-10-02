using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models.Entities;

namespace Store.DbServices.Configurations;

/// <summary>
/// SEC-23 — EF configuration for <see cref="TrustedDevice"/>.
/// Snake_case column naming follows the codebase convention. The
/// (UserId, FingerprintHash) unique index makes "register-or-update"
/// lookups cheap + race-free; the UserId-only index powers
/// "list user's devices" reads.
/// </summary>
public class TrustedDeviceConfiguration : IEntityTypeConfiguration<TrustedDevice>
{
    public void Configure(EntityTypeBuilder<TrustedDevice> builder)
    {
        builder.HasKey(d => d.TrustedDeviceId);
        builder.Property(d => d.TrustedDeviceId).ValueGeneratedOnAdd();

        builder.Property(d => d.UserId).IsRequired();
        builder.Property(d => d.DeviceId).IsRequired().HasMaxLength(64);
        builder.Property(d => d.FingerprintHash).IsRequired().HasMaxLength(64);
        builder.Property(d => d.DeviceName).IsRequired().HasMaxLength(100);
        builder.Property(d => d.UserAgent).HasMaxLength(500);
        builder.Property(d => d.IpAddressCidr).HasMaxLength(64);

        builder.Property(d => d.FirstSeenAtUtc).IsRequired();
        builder.Property(d => d.LastSeenAtUtc).IsRequired();
        builder.Property(d => d.IsRevoked).IsRequired().HasDefaultValue(false);
        builder.Property(d => d.IsTrusted).IsRequired().HasDefaultValue(false);

        // (user, fingerprint) must be unique — a device fingerprint is the
        // canonical identity. The DeviceId is a stable public alias; the
        // FingerprintHash is server-only and never leaves the server.
        builder.HasIndex(d => new { d.UserId, d.FingerprintHash }).IsUnique();
        builder.HasIndex(d => new { d.UserId, d.DeviceId }).IsUnique();
        builder.HasIndex(d => d.UserId);

        // Cascade delete with the user — orphaned device rows have no value.
        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // TrustedDevice uses IsRevoked for device revocation rather than soft deletion;
        // ignore BaseEntity soft-delete columns not present in the trusted_device database table.
        builder.Ignore(d => d.DeletedAt);
        builder.Ignore(d => d.DeletedById);
        builder.Ignore(d => d.IsDeleted);
    }
}
