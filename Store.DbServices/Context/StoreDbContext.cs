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
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<TaxProfile> TaxProfiles => Set<TaxProfile>();
    public DbSet<BundleRule> BundleRules => Set<BundleRule>();
    public DbSet<CustomerSegmentPrice> CustomerSegmentPrices => Set<CustomerSegmentPrice>();

    // ---- Transactions ----
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceTender> InvoiceTenders => Set<InvoiceTender>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<ItemsOrder> ItemsOrders => Set<ItemsOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<MobileMoneyTransaction> MobileMoneyTransactions => Set<MobileMoneyTransaction>();

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
    public DbSet<CashierShift> CashierShifts => Set<CashierShift>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // ---- Branches ----
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<UserBranchRole> UserBranchRoles => Set<UserBranchRole>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();

    // ---- Wastage & Override ----
    public DbSet<WastageEntry> WastageEntries => Set<WastageEntry>();
    public DbSet<DiscountOverrideRequest> DiscountOverrideRequests => Set<DiscountOverrideRequest>();

    // ---- Procurement ----
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    // ---- Cash Variance ----
    public DbSet<CashVarianceRecord> CashVarianceRecords => Set<CashVarianceRecord>();

    // ---- Loyalty ----
    public DbSet<CustomerLoyaltyAccount> CustomerLoyaltyAccounts => Set<CustomerLoyaltyAccount>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<LoyaltyCampaign> LoyaltyCampaigns => Set<LoyaltyCampaign>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically discover and apply all IEntityTypeConfiguration<T> in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreDbContext).Assembly);

        ConfigureOperationalRelationships(modelBuilder);

        // Keep legacy-style schema naming: lower snake_case for table and column names.
        ApplySnakeCaseNaming(modelBuilder);
    }

    private static void ConfigureOperationalRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CashierShift>()
            .HasOne(x => x.OpenedByUser)
            .WithMany(u => u.OpenedShifts)
            .HasForeignKey(x => x.OpenedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CashierShift>()
            .HasOne(x => x.ClosedByUser)
            .WithMany(u => u.ClosedShifts)
            .HasForeignKey(x => x.ClosedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BundleRule>()
            .HasOne(x => x.TriggerItem)
            .WithMany()
            .HasForeignKey(x => x.TriggerItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BundleRule>()
            .HasOne(x => x.RewardItem)
            .WithMany()
            .HasForeignKey(x => x.RewardItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RolePermission>()
            .HasIndex(x => new { x.RoleId, x.PermissionKey })
            .IsUnique();

        modelBuilder.Entity<CustomerSegmentPrice>()
            .HasIndex(x => new { x.ItemId, x.Segment, x.IsActive });

        modelBuilder.Entity<StockMovement>()
            .HasIndex(x => new { x.ItemId, x.DateCreated });

        // Loyalty
        modelBuilder.Entity<CustomerLoyaltyAccount>()
            .HasOne(x => x.Customer)
            .WithOne(c => c.LoyaltyAccount)
            .HasForeignKey<CustomerLoyaltyAccount>(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CustomerLoyaltyAccount>()
            .HasIndex(x => x.CustomerId)
            .IsUnique();

        modelBuilder.Entity<LoyaltyTransaction>()
            .HasOne(x => x.LoyaltyAccount)
            .WithMany(a => a.Transactions)
            .HasForeignKey(x => x.LoyaltyAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LoyaltyTransaction>()
            .HasOne(x => x.Invoice)
            .WithMany(i => i.LoyaltyTransactions)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<LoyaltyCampaign>()
            .HasIndex(x => new { x.IsActive, x.StartDate, x.EndDate });

        // Discount indexes
        modelBuilder.Entity<Discount>()
            .HasIndex(x => new { x.IsActive, x.ValidFrom, x.ValidTo });

        modelBuilder.Entity<Discount>()
            .HasIndex(x => x.CouponCode)
            .IsUnique()
            .HasFilter("coupon_code IS NOT NULL");

        // StockTransfer relationships
        modelBuilder.Entity<StockTransfer>()
            .HasOne(x => x.FromBranch)
            .WithMany()
            .HasForeignKey(x => x.FromBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockTransfer>()
            .HasOne(x => x.ToBranch)
            .WithMany()
            .HasForeignKey(x => x.ToBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockTransfer>()
            .HasOne(x => x.RequestedByUser)
            .WithMany()
            .HasForeignKey(x => x.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockTransferItem>()
            .HasOne(x => x.Transfer)
            .WithMany(t => t.Items)
            .HasForeignKey(x => x.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StockTransferItem>()
            .HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockTransfer>()
            .HasIndex(x => new { x.Status, x.DateCreated });

        // WastageEntry relationships
        modelBuilder.Entity<WastageEntry>()
            .HasOne(w => w.Item)
            .WithMany()
            .HasForeignKey(w => w.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WastageEntry>()
            .HasOne(w => w.RecordedByUser)
            .WithMany()
            .HasForeignKey(w => w.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WastageEntry>()
            .HasIndex(w => new { w.ItemId, w.DateCreated });

        // DiscountOverrideRequest relationships
        modelBuilder.Entity<DiscountOverrideRequest>()
            .HasOne(r => r.RequestedByUser)
            .WithMany()
            .HasForeignKey(r => r.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DiscountOverrideRequest>()
            .HasOne(r => r.ReviewedByUser)
            .WithMany()
            .HasForeignKey(r => r.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DiscountOverrideRequest>()
            .HasIndex(r => new { r.Status, r.DateCreated });

        // PurchaseOrder relationships
        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(p => p.RequestedByUser)
            .WithMany()
            .HasForeignKey(p => p.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(p => p.ApprovedByUser)
            .WithMany()
            .HasForeignKey(p => p.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrder>()
            .HasMany(p => p.Items)
            .WithOne(i => i.PurchaseOrder)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseOrderItem>()
            .HasOne(i => i.Item)
            .WithMany()
            .HasForeignKey(i => i.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrder>()
            .HasIndex(p => new { p.Status, p.DateCreated });

        modelBuilder.Entity<PurchaseOrder>()
            .HasIndex(p => p.ReferenceNumber)
            .IsUnique()
            .HasFilter("reference_number IS NOT NULL");

        // CashVarianceRecord relationships
        modelBuilder.Entity<CashVarianceRecord>()
            .HasOne(v => v.CashierShift)
            .WithMany()
            .HasForeignKey(v => v.CashierShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CashVarianceRecord>()
            .HasOne(v => v.RecordedByUser)
            .WithMany()
            .HasForeignKey(v => v.RecordedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CashVarianceRecord>()
            .HasOne(v => v.ReviewedByUser)
            .WithMany()
            .HasForeignKey(v => v.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CashVarianceRecord>()
            .HasIndex(v => new { v.Status, v.DateCreated });
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
