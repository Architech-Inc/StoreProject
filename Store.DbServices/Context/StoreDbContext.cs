using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Store.Models.Entities;
using Store.Models.Entities.Contacts;

namespace Store.DbServices.Context;

public class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options) { }

    // ---- Identity ----
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPassword> UserPasswords => Set<UserPassword>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();

    // ---- Personnel ----
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Salary> Salaries => Set<Salary>();
    public DbSet<Employee> Employees => Set<Employee>();

    // ---- Contacts ----
    public DbSet<Email> Emails => Set<Email>();
    public DbSet<Phone> Phones => Set<Phone>();
    public DbSet<UserEmail> UserEmails => Set<UserEmail>();
    public DbSet<UserPhone> UserPhones => Set<UserPhone>();
    public DbSet<EmployeeEmail> EmployeeEmails => Set<EmployeeEmail>();
    public DbSet<EmployeePhone> EmployeePhones => Set<EmployeePhone>();
    public DbSet<CustomerEmail> CustomerEmails => Set<CustomerEmail>();
    public DbSet<CustomerPhone> CustomerPhones => Set<CustomerPhone>();
    public DbSet<SupplierEmail> SupplierEmails => Set<SupplierEmail>();
    public DbSet<SupplierPhone> SupplierPhones => Set<SupplierPhone>();
    public DbSet<ManufacturerEmail> ManufacturerEmails => Set<ManufacturerEmail>();
    public DbSet<ManufacturerPhone> ManufacturerPhones => Set<ManufacturerPhone>();

    // ---- Geography ----
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<EmployeeLocation> EmployeeLocations => Set<EmployeeLocation>();
    public DbSet<CustomerLocation> CustomerLocations => Set<CustomerLocation>();
    public DbSet<SupplierLocation> SupplierLocations => Set<SupplierLocation>();
    public DbSet<ManufacturerLocation> ManufacturerLocations => Set<ManufacturerLocation>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Language> Languages => Set<Language>();

    // ---- Business Entities ----
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();

    // ---- Inventory ----
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemCategory> ItemCategories => Set<ItemCategory>();
    public DbSet<ItemCode> ItemCodes => Set<ItemCode>();
    public DbSet<ItemExpiry> ItemExpiries => Set<ItemExpiry>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<Discount> Discounts => Set<Discount>();

    // ---- Transactions ----
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceTender> InvoiceTenders => Set<InvoiceTender>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<ItemsOrder> ItemsOrders => Set<ItemsOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // ---- Privileges ----
    public DbSet<Privilege> Privileges => Set<Privilege>();
    public DbSet<UserPrivilege> UserPrivileges => Set<UserPrivilege>();
    public DbSet<UserPrivilegeAction> UserPrivilegeActions => Set<UserPrivilegeAction>();
    public DbSet<EmployeePrivilege> EmployeePrivileges => Set<EmployeePrivilege>();
    public DbSet<EmployeePrivilegeAction> EmployeePrivilegeActions => Set<EmployeePrivilegeAction>();
    public DbSet<CustomerPrivilege> CustomerPrivileges => Set<CustomerPrivilege>();
    public DbSet<CustomerPrivilegeAction> CustomerPrivilegeActions => Set<CustomerPrivilegeAction>();

    // ---- System ----
    public DbSet<Otp> Otps => Set<Otp>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ChangeLog> ChangeLogs => Set<ChangeLog>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically discover and apply all IEntityTypeConfiguration<T> in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreDbContext).Assembly);

        // Keep legacy-style schema naming: lower snake_case for table and column names.
        ApplySnakeCaseNaming(modelBuilder);
    }

    private static void ApplySnakeCaseNaming(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (entity.ClrType is null)
                continue;

            entity.SetTableName(ToSnakeCase(entity.ClrType.Name));

            foreach (var property in entity.GetProperties())
                property.SetColumnName(ToSnakeCase(property.Name));

            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName() ?? $"pk_{entity.ClrType.Name}"));

            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName() ?? $"fk_{entity.ClrType.Name}"));

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? $"ix_{entity.ClrType.Name}"));
        }
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var chars = new List<char>(value.Length + 8);

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (char.IsUpper(c))
            {
                if (i > 0 && (char.IsLower(value[i - 1]) || char.IsDigit(value[i - 1])))
                    chars.Add('_');

                chars.Add(char.ToLowerInvariant(c));
            }
            else
            {
                chars.Add(c);
            }
        }

        return new string(chars.ToArray());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        SetAuditTimestamps();
        return base.SaveChanges();
    }

    private void SetAuditTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Properties.Any(p => p.Metadata.Name == "LastModified"))
                entry.Property("LastModified").CurrentValue = DateTime.UtcNow;

            if (entry.State == EntityState.Added &&
                entry.Properties.Any(p => p.Metadata.Name == "DateCreated"))
                entry.Property("DateCreated").CurrentValue = DateTime.UtcNow;
        }
    }
}
