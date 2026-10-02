using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Store.Models.Entities;
using Store.Models.Entities.Contacts;
using Store.Models.Entities.Finance;
using Store.Models.Entities.HR;
using Store.Models.Entities.Inventory;

namespace Store.DbServices.Context;

public class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options) { }

    // ---- Identity ----
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPassword> UserPasswords => Set<UserPassword>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<FidoCredential> FidoCredentials => Set<FidoCredential>();
    public DbSet<TrustedDevice> TrustedDevices => Set<TrustedDevice>();

    // ---- Personnel ----
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Salary> Salaries => Set<Salary>();
    public DbSet<Employee> Employees => Set<Employee>();

    // ---- Contacts ----
    public DbSet<ContactChangeRequest> ContactChangeRequests => Set<ContactChangeRequest>();
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


    // ---- System ----
    public DbSet<Otp> Otps => Set<Otp>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ChangeLog> ChangeLogs => Set<ChangeLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<CashierShift> CashierShifts => Set<CashierShift>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    // ---- Branches ----
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<UserBranchRole> UserBranchRoles => Set<UserBranchRole>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();
    public DbSet<BranchItemStock> BranchItemStocks => Set<BranchItemStock>();
    public DbSet<PersonnelTransferHistory> PersonnelTransferHistories => Set<PersonnelTransferHistory>();
    public DbSet<DiscountBranch> DiscountBranches => Set<DiscountBranch>();
    public DbSet<LoyaltyCampaignBranch> LoyaltyCampaignBranches => Set<LoyaltyCampaignBranch>();
    public DbSet<RestockRecommendation> RestockRecommendations => Set<RestockRecommendation>();

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

    // ---- Core Finance (General Ledger) ----
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();

    // ---- HR & Payroll ----
    public DbSet<EmployeeContract> EmployeeContracts => Set<EmployeeContract>();
    public DbSet<TaxBracket> TaxBrackets => Set<TaxBracket>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<Payslip> Payslips => Set<Payslip>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically discover and apply all IEntityTypeConfiguration<T> in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreDbContext).Assembly);

        // ─── Soft-delete global query filters ───────────────────────────────────
        // Four entities get the canonical filter; every dependent child entity
        // gets a matching filter automatically so EF Core 10622 warnings go away.
        // See IMPLEMENTATION_LOG.md "Soft-delete migration" for the migration plan.
        var softDeletableRoots = new[]
        {
            typeof(Store.Models.Entities.Item),
            typeof(Store.Models.Entities.Supplier),
            typeof(Store.Models.Entities.Employee),
            typeof(Store.Models.Entities.Customer),
            typeof(Store.Models.Entities.Category),
            typeof(Store.Models.Entities.Department),
            typeof(Store.Models.Entities.Unit),
            typeof(Store.Models.Entities.Salary),
            typeof(Store.Models.Entities.Batch),
            typeof(Store.Models.Entities.Discount),
            typeof(Store.Models.Entities.LoyaltyCampaign),
            typeof(Store.Models.Entities.WastageEntry),
            typeof(Store.Models.Entities.HR.TaxBracket)
        };

        modelBuilder.Entity<Store.Models.Entities.Item>()
            .HasQueryFilter(i => !i.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.Supplier>()
            .HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.Employee>()
            .HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.Customer>()
            .HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.Category>()
            .HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.Department>()
            .HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.Unit>()
            .HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.Salary>()
            .HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.Batch>()
            .HasQueryFilter(b => !b.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.Discount>()
            .HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.LoyaltyCampaign>()
            .HasQueryFilter(l => !l.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.WastageEntry>()
            .HasQueryFilter(w => !w.IsDeleted);
        modelBuilder.Entity<Store.Models.Entities.HR.TaxBracket>()
            .HasQueryFilter(t => !t.IsDeleted);

        AddMatchingChildQueryFilters(modelBuilder, softDeletableRoots);

        ConfigureOperationalRelationships(modelBuilder);

        // HR & Payroll configurations
        modelBuilder.Entity<EmployeeContract>()
            .Property(e => e.EmployeeContractId)
            .UseCollation("utf8mb4_general_ci");
            
        modelBuilder.Entity<EmployeeContract>()
            .Property(e => e.EmployeeId)
            .UseCollation("utf8mb4_general_ci");
            
        modelBuilder.Entity<Payslip>()
            .Property(p => p.PayslipId)
            .UseCollation("utf8mb4_general_ci");
            
        modelBuilder.Entity<Payslip>()
            .Property(p => p.PayrollRunId)
            .UseCollation("utf8mb4_general_ci");
            
        modelBuilder.Entity<Payslip>()
            .Property(p => p.EmployeeId)
            .UseCollation("utf8mb4_general_ci");
            
        modelBuilder.Entity<PayrollRun>()
            .Property(p => p.PayrollRunId)
            .UseCollation("utf8mb4_general_ci");
            
        // Inventory & Restock configurations
        modelBuilder.Entity<RestockRecommendation>()
            .Property(r => r.RecommendationId)
            .UseCollation("utf8mb4_general_ci");
            
        modelBuilder.Entity<RestockRecommendation>()
            .Property(r => r.ItemId)
            .UseCollation("utf8mb4_general_ci");

        modelBuilder.Entity<SystemSetting>().HasData(new SystemSetting
        {
            SettingKey = "Auth:PasswordRecoveryMethod",
            SettingValue = "Both",
            Description = "Determines allowed password recovery methods (OTP, TempPassword, Both)",
            LastModified = DateTime.UtcNow
        });

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

        // ─── Lookup / hot-path indexes ─────────────────────────────────────────
        // Suppliers
        modelBuilder.Entity<Supplier>()
            .HasIndex(x => x.RegistrationNumber)
            .HasFilter("registration_number IS NOT NULL");

        // CashierShift — shift lookup is one of the most frequent queries in POS.
        modelBuilder.Entity<CashierShift>()
            .HasIndex(x => x.CashierShiftId)
            .IsUnique();
        modelBuilder.Entity<CashierShift>()
            .HasIndex(x => new { x.OpenedByUserId, x.OpenedAtUtc });
        modelBuilder.Entity<CashierShift>()
            .HasIndex(x => x.Status);

        // SEC-15 — Mobile money callback idempotency.
        // The provider's transaction reference is the only natural unique
        // key on the wire; the database enforces that no two callbacks for
        // the same provider reference can both update the transaction.
        // UNIQUE-filtered: rows with NULL provider_transaction_id are
        // pre-callback and allowed to be duplicated (status=Pending).
        modelBuilder.Entity<MobileMoneyTransaction>()
            .HasIndex(x => x.ProviderTransactionId)
            .IsUnique()
            .HasFilter("provider_transaction_id IS NOT NULL AND provider_transaction_id <> ''")
            .HasDatabaseName("ux_mobile_money_provider_tx_id");

        // Audit log hot-path indexes for /audit-log filtering & dashboards.
        // Note: AuditLog entity has no Severity column — severity lives on the
        // CreateAuditLogEntryRequest DTO and is used only when writing.
        modelBuilder.Entity<AuditLog>()
            .HasIndex(x => new { x.UserId, x.DateCreated });
        modelBuilder.Entity<AuditLog>()
            .HasIndex(x => new { x.Action, x.DateCreated });

        // MT-05 — tenant-scoped audit routing. The (TenantId, DateCreated)
        // composite index is the hot path for any tenant-isolated read; NULL
        // values are allowed so pre-provisioning / system audits stay visible.
        modelBuilder.Entity<AuditLog>()
            .HasIndex(x => new { x.TenantId, x.DateCreated })
            .HasDatabaseName("ix_audit_log_tenant_date");

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

        // Wave 23.A — POS session + cart fingerprint columns + index.
        // Lookups: "is there an approved override for this POS session?" +
        // the cleanup job's "find Approved overrides older than 15min".
        modelBuilder.Entity<DiscountOverrideRequest>()
            .HasIndex(r => new { r.PosSessionId, r.Status });

        modelBuilder.Entity<DiscountOverrideRequest>()
            .Property(r => r.PosSessionId).HasMaxLength(64);
        modelBuilder.Entity<DiscountOverrideRequest>()
            .Property(r => r.CartFingerprint).HasMaxLength(64);

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

        // Branch self-referencing relationship
        modelBuilder.Entity<Branch>()
            .HasOne(b => b.SupplyingWarehouse)
            .WithMany(b => b.SuppliedBranches)
            .HasForeignKey(b => b.SupplyingWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // RestockRecommendation relationships
        modelBuilder.Entity<RestockRecommendation>()
            .HasOne(r => r.Branch)
            .WithMany()
            .HasForeignKey(r => r.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<RestockRecommendation>()
            .HasOne(r => r.Item)
            .WithMany()
            .HasForeignKey(r => r.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<RestockRecommendation>()
            .HasOne(r => r.GeneratedStockTransfer)
            .WithMany()
            .HasForeignKey(r => r.GeneratedStockTransferId)
            .OnDelete(DeleteBehavior.SetNull);
            
        modelBuilder.Entity<RestockRecommendation>()
            .HasOne(r => r.GeneratedPurchaseOrder)
            .WithMany()
            .HasForeignKey(r => r.GeneratedPurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);

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

        // BranchItemStock composite key & relationships
        modelBuilder.Entity<BranchItemStock>()
            .HasKey(x => new { x.BranchId, x.ItemId });

        modelBuilder.Entity<BranchItemStock>()
            .HasOne(x => x.Branch)
            .WithMany(b => b.ItemStocks)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BranchItemStock>()
            .HasOne(x => x.Item)
            .WithMany(i => i.BranchStocks)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // DiscountBranch composite key & relationships
        modelBuilder.Entity<DiscountBranch>()
            .HasKey(x => new { x.DiscountId, x.BranchId });

        modelBuilder.Entity<DiscountBranch>()
            .HasOne(x => x.Discount)
            .WithMany(d => d.DiscountBranches)
            .HasForeignKey(x => x.DiscountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DiscountBranch>()
            .HasOne(x => x.Branch)
            .WithMany(b => b.DiscountBranches)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // LoyaltyCampaignBranch composite key & relationships
        modelBuilder.Entity<LoyaltyCampaignBranch>()
            .HasKey(x => new { x.LoyaltyCampaignId, x.BranchId });

        modelBuilder.Entity<LoyaltyCampaignBranch>()
            .HasOne(x => x.LoyaltyCampaign)
            .WithMany(c => c.CampaignBranches)
            .HasForeignKey(x => x.LoyaltyCampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LoyaltyCampaignBranch>()
            .HasOne(x => x.Branch)
            .WithMany(b => b.CampaignBranches)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // PersonnelTransferHistory relationships
        modelBuilder.Entity<PersonnelTransferHistory>()
            .HasOne(x => x.Employee)
            .WithMany(e => e.TransferHistories)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PersonnelTransferHistory>()
            .HasOne(x => x.FromBranch)
            .WithMany(b => b.OutgoingTransfers)
            .HasForeignKey(x => x.FromBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PersonnelTransferHistory>()
            .HasOne(x => x.ToBranch)
            .WithMany(b => b.IncomingTransfers)
            .HasForeignKey(x => x.ToBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PersonnelTransferHistory>()
            .HasOne(x => x.ApprovedByUser)
            .WithMany()
            .HasForeignKey(x => x.ApprovedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Employee HomeBranch relationship
        modelBuilder.Entity<Employee>()
            .HasOne(x => x.HomeBranch)
            .WithMany(b => b.Employees)
            .HasForeignKey(x => x.HomeBranchId)
            .OnDelete(DeleteBehavior.SetNull);

        // StockMovement Branch relationship
        modelBuilder.Entity<StockMovement>()
            .HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.SetNull);
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

    /// <summary>
    /// For every entity in the model, find navigations whose target is one of
    /// <paramref name="softDeletableRoots"/> (or has a query filter added in
    /// a prior pass) and whose FK is required. Add a matching query filter on
    /// the child so EF Core 10622 is satisfied. Iterates until convergence
    /// so transitive cases (e.g. LoyaltyTransaction → CustomerLoyaltyAccount →
    /// Customer) all get a filter.
    /// Pattern emitted: <c>e =&gt; !e.Parent.IsDeleted</c>.
    /// </summary>
    private static void AddMatchingChildQueryFilters(ModelBuilder modelBuilder, Type[] softDeletableRoots)
    {
        var rootSet = new HashSet<Type>(softDeletableRoots);

        // Track which entity types currently have a filter (so we can propagate
        // transitively in subsequent passes).
        var filtered = new HashSet<Type>(rootSet);

        // Keep iterating until no new filter is added.
        bool changed;
        do
        {
            changed = false;
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clr = entityType.ClrType;
                if (filtered.Contains(clr)) continue;
                if (clr.IsAbstract || clr.IsGenericTypeDefinition) continue;

                foreach (var navigation in entityType.GetNavigations())
                {
                    if (navigation.IsCollection) continue;
                    if (!navigation.ForeignKey!.IsRequired) continue;
                    var targetType = navigation.TargetEntityType.ClrType;
                    if (!filtered.Contains(targetType)) continue;

                    var navProp = navigation.PropertyInfo;
                    if (navProp is null) continue;

                    // Build the lambda:  e => !e.<Nav>.IsDeleted
                    var eParam = System.Linq.Expressions.Expression.Parameter(clr, "e");
                    var navAccess = System.Linq.Expressions.Expression.Property(eParam, navProp);
                    var isDeleted = System.Linq.Expressions.Expression.Property(navAccess, "IsDeleted");
                    var notExpr = System.Linq.Expressions.Expression.Not(isDeleted);
                    var lambda = System.Linq.Expressions.Expression.Lambda(notExpr, eParam);

                    modelBuilder.Entity(clr).HasQueryFilter(lambda);
                    filtered.Add(clr);
                    changed = true;
                    break;
                }
            }
        } while (changed);
    }
}
