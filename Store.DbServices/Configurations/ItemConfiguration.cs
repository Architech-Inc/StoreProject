using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models.Entities;
using Store.Models.Enums;

namespace Store.DbServices.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(i => i.ItemId);
        builder.Property(i => i.ItemId).ValueGeneratedOnAdd();

        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Description).HasMaxLength(2000);
        builder.Property(i => i.UnitPrice).IsRequired().HasPrecision(18, 4);
        builder.Property(i => i.CostPrice).HasPrecision(18, 4);
        builder.Property(i => i.Barcode).HasMaxLength(100);
        builder.Property(i => i.ImagePath).HasMaxLength(500);
        builder.HasIndex(i => i.Barcode).IsUnique().HasFilter("barcode IS NOT NULL");

        builder.Property(i => i.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ItemType.Product);

        builder.HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Unit)
            .WithMany(u => u.Items)
            .HasForeignKey(i => i.UnitId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Manufacturer)
            .WithMany(m => m.Items)
            .HasForeignKey(i => i.ManufacturerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.ItemExpiry)
            .WithOne(e => e.Item)
            .HasForeignKey<ItemExpiry>(e => e.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Discount)
            .WithOne(d => d.Item)
            .HasForeignKey<Discount>(d => d.ItemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.CategoryId);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.ImagePath).HasMaxLength(500);
        builder.HasIndex(c => c.Name).IsUnique();
    }
}

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.HasKey(u => u.UnitId);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Abbreviation).IsRequired().HasMaxLength(20);
        builder.Property(u => u.Description).HasMaxLength(300);
        builder.HasIndex(u => u.Abbreviation).IsUnique();
    }
}

public class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.HasKey(b => b.BatchId);
        builder.Property(b => b.BatchId).ValueGeneratedOnAdd();
        builder.Property(b => b.BatchNumber).IsRequired().HasMaxLength(100);
        builder.Property(b => b.CostPrice).HasPrecision(18, 4);
        builder.Property(b => b.Notes).HasMaxLength(500);

        builder.HasOne(b => b.Item)
            .WithMany(i => i.Batches)
            .HasForeignKey(b => b.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.HasKey(d => d.DiscountId);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(150);
        builder.Property(d => d.Percentage).HasPrecision(5, 2);

        builder.HasOne(d => d.ManagedByUser)
            .WithMany(u => u.ManagedDiscounts)
            .HasForeignKey(d => d.ManagedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ItemCodeConfiguration : IEntityTypeConfiguration<ItemCode>
{
    public void Configure(EntityTypeBuilder<ItemCode> builder)
    {
        builder.HasKey(c => c.ItemCodeId);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(100);
        builder.Property(c => c.CodeType).HasMaxLength(50);
        builder.HasIndex(c => c.Code).IsUnique();

        builder.HasOne(c => c.Item)
            .WithMany(i => i.ItemCodes)
            .HasForeignKey(c => c.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
