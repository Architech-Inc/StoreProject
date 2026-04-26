using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models.Entities;
using Store.Models.Enums;

namespace Store.DbServices.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId).ValueGeneratedOnAdd();

        builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(UserStatus.NotVerified);

        builder.Property(u => u.ImagePath).HasMaxLength(500);
        builder.Property(u => u.DateCreated).IsRequired();
        builder.Property(u => u.LastModified).IsRequired();

        // One-to-one: User → Password
        builder.HasOne(u => u.Password)
            .WithOne(p => p.User)
            .HasForeignKey<UserPassword>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-one: User → Token
        builder.HasOne(u => u.UserToken)
            .WithOne(t => t.User)
            .HasForeignKey<UserToken>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-one: User → Role
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-one: User → Employee (optional)
        builder.HasOne(u => u.Employee)
            .WithMany(e => e.Users)
            .HasForeignKey(u => u.EmployeeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class UserPasswordConfiguration : IEntityTypeConfiguration<UserPassword>
{
    public void Configure(EntityTypeBuilder<UserPassword> builder)
    {
        builder.HasKey(p => p.UserPasswordId);
        builder.Property(p => p.PasswordHash).IsRequired().HasMaxLength(256);
        builder.HasIndex(p => p.UserId).IsUnique();
    }
}

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.HasKey(t => t.UserTokenId);
        builder.HasIndex(t => t.UserId).IsUnique();

        builder.Property(t => t.Token).IsRequired().HasMaxLength(2000);
        builder.Property(t => t.RefreshTokenHash).IsRequired().HasMaxLength(256);
        builder.Property(t => t.ExpiryDate).IsRequired();
        builder.Property(t => t.RefreshTokenExpiryDate).IsRequired();
    }
}
