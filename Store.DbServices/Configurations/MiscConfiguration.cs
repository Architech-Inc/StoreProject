using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models.Entities;
using Store.Models.Entities.Contacts;
using Store.Models.Enums;

namespace Store.DbServices.Configurations;

public class EmailConfiguration : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.HasKey(e => e.EmailId);
        builder.Property(e => e.Address).IsRequired().HasMaxLength(254);
        builder.HasIndex(e => e.Address).IsUnique();
        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(20);
    }
}

public class PhoneConfiguration : IEntityTypeConfiguration<Phone>
{
    public void Configure(EntityTypeBuilder<Phone> builder)
    {
        builder.HasKey(p => p.PhoneId);
        builder.Property(p => p.Number).IsRequired().HasMaxLength(30);
        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(b => new { b.CountryId, b.Number }).IsUnique();
    }
}

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.CustomerId);
        builder.Property(c => c.CustomerId).ValueGeneratedOnAdd();
        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.MiddleName).HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Notes).HasMaxLength(1000);
        builder.Property(c => c.ImagePath).HasMaxLength(500);
        builder.Property(c => c.Gender).HasConversion<string>().HasMaxLength(20);
    }
}

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(s => s.SupplierId);
        builder.Property(s => s.SupplierId).ValueGeneratedOnAdd();
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.RegistrationNumber).HasMaxLength(100);
        builder.Property(s => s.Notes).HasMaxLength(1000);
        builder.Property(s => s.ImagePath).HasMaxLength(500);
    }
}

public class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        builder.HasKey(m => m.ManufacturerId);
        builder.Property(m => m.ManufacturerId).ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
        builder.Property(m => m.RegistrationNumber).HasMaxLength(100);
        builder.Property(m => m.Website).HasMaxLength(300);
        builder.Property(m => m.Notes).HasMaxLength(1000);
        builder.Property(m => m.ImagePath).HasMaxLength(500);
    }
}

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasKey(c => c.CountryId);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.IsoCode).HasMaxLength(3);
        builder.Property(c => c.PhoneCode).HasMaxLength(10);
        builder.HasIndex(c => c.IsoCode).IsUnique().HasFilter("iso_code IS NOT NULL");
    }
}

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.HasKey(r => r.RegionId);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.HasOne(r => r.Country).WithMany(c => c.Regions)
            .HasForeignKey(r => r.CountryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasKey(c => c.CityId);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.HasOne(c => c.Region).WithMany(r => r.Cities)
            .HasForeignKey(c => c.RegionId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(l => l.LocationId);
        builder.Property(l => l.StreetAddress).HasMaxLength(300);
        builder.Property(l => l.PostalCode).HasMaxLength(20);
        builder.Property(l => l.Latitude).HasMaxLength(20);
        builder.Property(l => l.Longitude).HasMaxLength(20);
        builder.HasOne(l => l.City).WithMany(c => c.Locations)
            .HasForeignKey(l => l.CityId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
    }
}

public class OtpConfiguration : IEntityTypeConfiguration<Otp>
{
    public void Configure(EntityTypeBuilder<Otp> builder)
    {
        builder.HasKey(o => o.OtpId);
        builder.Property(o => o.Code).IsRequired().HasMaxLength(10);
        builder.Property(o => o.Purpose).HasConversion<string>().HasMaxLength(30);
        builder.HasOne(o => o.User).WithMany(u => u.Otps)
            .HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.NotificationId);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(2000);
        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(20);
        builder.HasOne(n => n.User).WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChangeLogConfiguration : IEntityTypeConfiguration<ChangeLog>
{
    public void Configure(EntityTypeBuilder<ChangeLog> builder)
    {
        builder.HasKey(c => c.ChangeLogId);
        builder.Property(c => c.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.EntityId).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Action).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.OldValues).HasColumnType("longtext");
        builder.Property(c => c.NewValues).HasColumnType("longtext");
        builder.Property(c => c.IpAddress).HasMaxLength(50);
        builder.HasOne(c => c.User).WithMany(u => u.ChangeLogs)
            .HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.RoleId);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Description).HasMaxLength(300);
        builder.HasIndex(r => r.Name).IsUnique();

        // Seed default roles
        builder.HasData(
            new Role { RoleId = 1, Name = "Admin", Description = "Full system access", DateCreated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastModified = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { RoleId = 2, Name = "Manager", Description = "Management level access", DateCreated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastModified = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { RoleId = 3, Name = "User", Description = "Standard user access", DateCreated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), LastModified = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}

public class PrivilegeConfiguration : IEntityTypeConfiguration<Privilege>
{
    public void Configure(EntityTypeBuilder<Privilege> builder)
    {
        builder.HasKey(p => p.PrivilegeId);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Module).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);
    }
}
