using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models.Entities;
using Store.Models.Enums;

namespace Store.DbServices.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.InvoiceId);
        builder.Property(i => i.InvoiceId).ValueGeneratedOnAdd();
        builder.Property(i => i.TotalAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(i => i.AmountTendered).HasPrecision(18, 2);
        builder.Property(i => i.ChangeGiven).HasPrecision(18, 2);
        builder.Property(i => i.Notes).HasMaxLength(1000);

        builder.Property(i => i.PaymentType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(PaymentType.Cash);

        builder.HasOne(i => i.User)
            .WithMany(u => u.Invoices)
            .HasForeignKey(i => i.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.HasKey(s => s.SaleId);
        builder.Property(s => s.SaleId).ValueGeneratedOnAdd();
        builder.Property(s => s.ItemName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.UnitAbbreviation).HasMaxLength(20);
        builder.Property(s => s.UnitPrice).IsRequired().HasPrecision(18, 4);
        builder.Property(s => s.DiscountAmount).HasPrecision(18, 4);
        builder.Property(s => s.LineTotal).IsRequired().HasPrecision(18, 2);

        builder.HasOne(s => s.Invoice)
            .WithMany(i => i.Sales)
            .HasForeignKey(s => s.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Item)
            .WithMany(i => i.Sales)
            .HasForeignKey(s => s.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class InvoiceTenderConfiguration : IEntityTypeConfiguration<InvoiceTender>
{
    public void Configure(EntityTypeBuilder<InvoiceTender> builder)
    {
        builder.HasKey(t => t.InvoiceTenderId);
        builder.Property(t => t.Amount).IsRequired().HasPrecision(18, 2);
        builder.Property(t => t.Reference).HasMaxLength(200);

        builder.Property(t => t.PaymentType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(t => t.Invoice)
            .WithMany(i => i.Tenders)
            .HasForeignKey(t => t.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ItemsOrderConfiguration : IEntityTypeConfiguration<ItemsOrder>
{
    public void Configure(EntityTypeBuilder<ItemsOrder> builder)
    {
        builder.HasKey(o => o.ItemsOrderId);
        builder.Property(o => o.ItemsOrderId).ValueGeneratedOnAdd();
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(100);
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
        builder.Property(o => o.Notes).HasMaxLength(1000);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(OrderStatus.Draft);

        builder.HasOne(o => o.Supplier)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.SupplierId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.CreatedByUser)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.CreatedByUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.OrderItemId);
        builder.Property(oi => oi.ItemName).IsRequired().HasMaxLength(200);
        builder.Property(oi => oi.UnitCost).HasPrecision(18, 4);
        builder.Property(oi => oi.LineTotal).HasPrecision(18, 2);

        builder.HasOne(oi => oi.ItemsOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.ItemsOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Item)
            .WithMany(i => i.OrderItems)
            .HasForeignKey(oi => oi.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
