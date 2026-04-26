using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models.Entities;
using Store.Models.Enums;

namespace Store.DbServices.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.EmployeeId);
        builder.Property(e => e.EmployeeId).ValueGeneratedOnAdd();

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.MiddleName).HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.NidNumber).HasMaxLength(50);
        builder.Property(e => e.PlaceOfBirth).HasMaxLength(200);
        builder.Property(e => e.ImagePath).HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(EmployeeStatus.Pending);

        builder.Property(e => e.Gender)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(Gender.NotSpecified);

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Salary)
            .WithMany(s => s.Employees)
            .HasForeignKey(e => e.SalaryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.DepartmentId);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(150);
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.HasIndex(d => d.Name).IsUnique();
    }
}

public class SalaryConfiguration : IEntityTypeConfiguration<Salary>
{
    public void Configure(EntityTypeBuilder<Salary> builder)
    {
        builder.HasKey(s => s.SalaryId);
        builder.Property(s => s.Grade).IsRequired().HasMaxLength(50);
        builder.Property(s => s.BasicAmount).HasPrecision(18, 2);
        builder.Property(s => s.AllowanceAmount).HasPrecision(18, 2);
        builder.Property(s => s.Description).HasMaxLength(500);
    }
}
