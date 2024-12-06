using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using StoreProjectModels.DatabaseModels;

#nullable disable

namespace StoreProjectModels.DbContexts
{
    public partial class StoreDbContext : DbContext
    {
        public StoreDbContext()
        {
        }

        public StoreDbContext(DbContextOptions<StoreDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Batch> Batches { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<ChangeLog> ChangeLogs { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<CustomerCity> CustomerCities { get; set; }
        public virtual DbSet<CustomerCountry> CustomerCountries { get; set; }
        public virtual DbSet<CustomerEmail> CustomerEmails { get; set; }
        public virtual DbSet<CustomerLocation> CustomerLocations { get; set; }
        public virtual DbSet<CustomerPhone> CustomerPhones { get; set; }
        public virtual DbSet<CustomerPrivilege> CustomerPrivileges { get; set; }
        public virtual DbSet<CustomerPrivilegeAction> CustomerPrivilegeActions { get; set; }
        public virtual DbSet<CustomerRegion> CustomerRegions { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<Dsicount> Dsicounts { get; set; }
        public virtual DbSet<Email> Emails { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<EmployeeCity> EmployeeCities { get; set; }
        public virtual DbSet<EmployeeCountry> EmployeeCountries { get; set; }
        public virtual DbSet<EmployeeEmail> EmployeeEmails { get; set; }
        public virtual DbSet<EmployeeLocation> EmployeeLocations { get; set; }
        public virtual DbSet<EmployeePhone> EmployeePhones { get; set; }
        public virtual DbSet<EmployeePrivilege> EmployeePrivileges { get; set; }
        public virtual DbSet<EmployeePrivilegeAction> EmployeePrivilegeActions { get; set; }
        public virtual DbSet<EmployeeRegion> EmployeeRegions { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceTender> InvoiceTenders { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<ItemCategory> ItemCategories { get; set; }
        public virtual DbSet<ItemCode> ItemCodes { get; set; }
        public virtual DbSet<ItemExpiry> ItemExpiries { get; set; }
        public virtual DbSet<ItemsOrder> ItemsOrders { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Manufacturer> Manufacturers { get; set; }
        public virtual DbSet<ManufacturerCity> ManufacturerCities { get; set; }
        public virtual DbSet<ManufacturerCountry> ManufacturerCountries { get; set; }
        public virtual DbSet<ManufacturerEmail> ManufacturerEmails { get; set; }
        public virtual DbSet<ManufacturerLocation> ManufacturerLocations { get; set; }
        public virtual DbSet<ManufacturerPhone> ManufacturerPhones { get; set; }
        public virtual DbSet<ManufacturerRegion> ManufacturerRegions { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<Otp> Otps { get; set; }
        public virtual DbSet<Password> Passwords { get; set; }
        public virtual DbSet<Phone> Phones { get; set; }
        public virtual DbSet<Privilege> Privileges { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Salary> Salaries { get; set; }
        public virtual DbSet<Sale> Sales { get; set; }
        public virtual DbSet<Sentmail> Sentmails { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<SupplierCity> SupplierCities { get; set; }
        public virtual DbSet<SupplierCountry> SupplierCountries { get; set; }
        public virtual DbSet<SupplierEmail> SupplierEmails { get; set; }
        public virtual DbSet<SupplierLocation> SupplierLocations { get; set; }
        public virtual DbSet<SupplierPhone> SupplierPhones { get; set; }
        public virtual DbSet<SupplierRegion> SupplierRegions { get; set; }
        public virtual DbSet<Unit> Units { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserCity> UserCities { get; set; }
        public virtual DbSet<UserCountry> UserCountries { get; set; }
        public virtual DbSet<UserEmail> UserEmails { get; set; }
        public virtual DbSet<UserPhone> UserPhones { get; set; }
        public virtual DbSet<UserPrivilege> UserPrivileges { get; set; }
        public virtual DbSet<UserPrivilegeAction> UserPrivilegeActions { get; set; }
        public virtual DbSet<UserRegion> UserRegions { get; set; }
        public virtual DbSet<UserToken> UserTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySQL("server=localhost;port=3306;database=store_db;user=root;password=");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Batch>(entity =>
            {
                entity.ToTable("batch");

                entity.HasIndex(e => e.BatchCode, "batch_code")
                    .IsUnique();

                entity.HasIndex(e => e.Code, "code")
                    .IsUnique();

                entity.HasIndex(e => e.ItemId, "itemId");

                entity.Property(e => e.BatchId)
                    .HasMaxLength(36)
                    .HasColumnName("batchId");

                entity.Property(e => e.BatchCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("batch_code");

                entity.Property(e => e.BatchCodeType)
                    .IsRequired()
                    .HasColumnType("enum('BarCode','QrCode')")
                    .HasColumnName("batch_code_type");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("code");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");

                entity.Property(e => e.ImagePath)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("image_path");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("itemId");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.ManufacturedDate).HasColumnName("manufactured_date");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.Batches)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("batch_ibfk_1");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("category");

                entity.HasIndex(e => e.Guid, "guid")
                    .IsUnique();

                entity.Property(e => e.CategoryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("categoryId");

                entity.Property(e => e.Guid)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("guid");

                entity.Property(e => e.IconPath)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("icon_path");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("name");

                entity.Property(e => e.NumberOfProducts)
                    .HasColumnType("int(11)")
                    .HasColumnName("number_of_products");
            });

            modelBuilder.Entity<ChangeLog>(entity =>
            {
                entity.ToTable("change_log");

                entity.HasIndex(e => e.UserId, "userId");

                entity.Property(e => e.ChangeLogId)
                    .HasMaxLength(36)
                    .HasColumnName("change_logId");

                entity.Property(e => e.DateLogged)
                    .HasColumnType("int(11)")
                    .HasColumnName("date_logged");

                entity.Property(e => e.Details)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnName("details");

                entity.Property(e => e.Link)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("link");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ChangeLogs)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("change_log_ibfk_1");
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.ToTable("city");

                entity.HasIndex(e => e.CountryId, "countryId");

                entity.HasIndex(e => e.Guid, "guid")
                    .IsUnique();

                entity.Property(e => e.CityId)
                    .HasColumnType("int(11)")
                    .HasColumnName("cityId");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("countryId");

                entity.Property(e => e.Guid)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("guid");

                entity.Property(e => e.Latitude).HasColumnName("latitude");

                entity.Property(e => e.Longitude).HasColumnName("longitude");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("name");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Cities)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("city_ibfk_1");
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("country");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("countryId");

                entity.Property(e => e.Alpha3)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("alpha_3");

                entity.Property(e => e.Capital)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("capital");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasColumnName("code");

                entity.Property(e => e.Continent)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("continent");

                entity.Property(e => e.ContinentCode)
                    .IsRequired()
                    .HasMaxLength(2)
                    .HasColumnName("continent_code");

                entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("currency");

                entity.Property(e => e.DialCode)
                    .HasColumnType("int(11)")
                    .HasColumnName("dial_code");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Symbol)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("symbol");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customer");

                entity.HasIndex(e => e.NidNumber, "nid_number")
                    .IsUnique();

                entity.Property(e => e.CustomerId)
                    .HasMaxLength(36)
                    .HasColumnName("customerId");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("address");

                entity.Property(e => e.Contact)
                    .IsRequired()
                    .HasMaxLength(9)
                    .HasColumnName("contact");

                entity.Property(e => e.DateRegistered).HasColumnName("date_registered");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(25)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(25)
                    .HasColumnName("last_name");

                entity.Property(e => e.NidNumber)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("nid_number");

                entity.Property(e => e.ProfileImagePath)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("profile_image_path");
            });

            modelBuilder.Entity<CustomerCity>(entity =>
            {
                entity.ToTable("customer_city");

                entity.HasIndex(e => e.CityId, "cityId");

                entity.HasIndex(e => new { e.CustomerId, e.CityId }, "customerId")
                    .IsUnique();

                entity.HasIndex(e => e.CustomerId, "customerId_2")
                    .IsUnique();

                entity.Property(e => e.CustomerCityId)
                    .HasColumnType("int(11)")
                    .HasColumnName("customer_cityId");

                entity.Property(e => e.CityId)
                    .HasColumnType("int(11)")
                    .HasColumnName("cityId");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("customerId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.CustomerCities)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("customer_city_ibfk_2");

                entity.HasOne(d => d.Customer)
                    .WithOne(p => p.CustomerCity)
                    .HasForeignKey<CustomerCity>(d => d.CustomerId)
                    .HasConstraintName("customer_city_ibfk_1");
            });

            modelBuilder.Entity<CustomerCountry>(entity =>
            {
                entity.ToTable("customer_country");

                entity.HasIndex(e => new { e.CustomerId, e.CountryId }, "customerId")
                    .IsUnique();

                entity.HasIndex(e => e.CountryId, "customer_country_ibfk_2")
                    .IsUnique();

                entity.Property(e => e.CustomerCountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("customer_countryId");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("countryId");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("customerId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.HasOne(d => d.Country)
                    .WithOne(p => p.CustomerCountry)
                    .HasForeignKey<CustomerCountry>(d => d.CountryId)
                    .HasConstraintName("customer_country_ibfk_2");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerCountries)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("customer_country_ibfk_1");
            });

            modelBuilder.Entity<CustomerEmail>(entity =>
            {
                entity.ToTable("customer_email");

                entity.HasIndex(e => e.CustomerId, "customerId");

                entity.HasIndex(e => new { e.CustomerId, e.EmailId }, "customerId_2")
                    .IsUnique();

                entity.HasIndex(e => e.EmailId, "emailId");

                entity.Property(e => e.CustomerEmailId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("customer_emailId");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("customerId");

                entity.Property(e => e.EmailId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("emailId");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerEmails)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("customer_email_ibfk_1");

                entity.HasOne(d => d.Email)
                    .WithMany(p => p.CustomerEmails)
                    .HasForeignKey(d => d.EmailId)
                    .HasConstraintName("customer_email_ibfk_2");
            });

            modelBuilder.Entity<CustomerLocation>(entity =>
            {
                entity.ToTable("customer_location");

                entity.HasIndex(e => new { e.CustomerId, e.LocationId }, "customerId")
                    .IsUnique();

                entity.HasIndex(e => e.LocationId, "locationId");

                entity.HasIndex(e => e.CustomerId, "supplerId");

                entity.Property(e => e.CustomerLocationId)
                    .HasColumnType("int(11)")
                    .HasColumnName("customer_locationId");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("customerId");

                entity.Property(e => e.LocationId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("locationId");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerLocations)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("customer_location_ibfk_1");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.CustomerLocations)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("customer_location_ibfk_2");
            });

            modelBuilder.Entity<CustomerPhone>(entity =>
            {
                entity.ToTable("customer_phone");

                entity.HasIndex(e => e.CustomerId, "customerId");

                entity.HasIndex(e => new { e.CustomerId, e.PhoneId }, "customerId_2")
                    .IsUnique();

                entity.HasIndex(e => e.PhoneId, "phoneId");

                entity.Property(e => e.CustomerPhoneId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("customer_phoneId");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("customerId");

                entity.Property(e => e.PhoneId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("phoneId");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerPhones)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("customer_phone_ibfk_1");

                entity.HasOne(d => d.Phone)
                    .WithMany(p => p.CustomerPhones)
                    .HasForeignKey(d => d.PhoneId)
                    .HasConstraintName("customer_phone_ibfk_2");
            });

            modelBuilder.Entity<CustomerPrivilege>(entity =>
            {
                entity.ToTable("customer_privilege");

                entity.HasIndex(e => e.CustomerId, "customerId");

                entity.HasIndex(e => e.PrivilegeId, "privilegeId");

                entity.Property(e => e.CustomerPrivilegeId)
                    .HasColumnType("int(11)")
                    .HasColumnName("customer_privilegeId");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("customerId");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.DateGranted).HasColumnName("date_granted");

                entity.Property(e => e.DateRevoked).HasColumnName("date_revoked");

                entity.Property(e => e.IsRevoked).HasColumnName("is_revoked");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.PrivilegeId)
                    .HasColumnType("int(11)")
                    .HasColumnName("privilegeId");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerPrivileges)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("customer_privilege_ibfk_1");

                entity.HasOne(d => d.Privilege)
                    .WithMany(p => p.CustomerPrivileges)
                    .HasForeignKey(d => d.PrivilegeId)
                    .HasConstraintName("customer_privilege_ibfk_2");
            });

            modelBuilder.Entity<CustomerPrivilegeAction>(entity =>
            {
                entity.ToTable("customer_privilege_action");

                entity.HasIndex(e => e.ActionAuthorUserId, "action_author_userId");

                entity.HasIndex(e => e.CustomerPrivilegeId, "customer_privilegeId");

                entity.Property(e => e.CustomerPrivilegeActionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("customer_privilege_actionId");

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasColumnType("enum('Grant','Revoke')")
                    .HasColumnName("action");

                entity.Property(e => e.ActionAuthorUserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("action_author_userId");

                entity.Property(e => e.ActionDate).HasColumnName("action_date");

                entity.Property(e => e.CustomerPrivilegeId)
                    .HasColumnType("int(11)")
                    .HasColumnName("customer_privilegeId");

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("reason");

                entity.HasOne(d => d.ActionAuthorUser)
                    .WithMany(p => p.CustomerPrivilegeActions)
                    .HasForeignKey(d => d.ActionAuthorUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("customer_privilege_action_ibfk_2");

                entity.HasOne(d => d.CustomerPrivilege)
                    .WithMany(p => p.CustomerPrivilegeActions)
                    .HasForeignKey(d => d.CustomerPrivilegeId)
                    .HasConstraintName("customer_privilege_action_ibfk_1");
            });

            modelBuilder.Entity<CustomerRegion>(entity =>
            {
                entity.ToTable("customer_region");

                entity.HasIndex(e => new { e.CustomerId, e.RegionId }, "customerId")
                    .IsUnique();

                entity.HasIndex(e => e.CustomerId, "customerId_2")
                    .IsUnique();

                entity.HasIndex(e => e.RegionId, "regionId");

                entity.Property(e => e.CustomerRegionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("customer_regionId");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("customerId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.RegionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("regionId");

                entity.HasOne(d => d.Customer)
                    .WithOne(p => p.CustomerRegion)
                    .HasForeignKey<CustomerRegion>(d => d.CustomerId)
                    .HasConstraintName("customer_region_ibfk_1");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.CustomerRegions)
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("customer_region_ibfk_2");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("department");

                entity.Property(e => e.DepartmentId)
                    .HasColumnType("int(11)")
                    .HasColumnName("departmentId");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.DepartmentCode)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("department_code");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("description");

                entity.Property(e => e.IconPath)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("icon_path");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("document");

                entity.HasIndex(e => e.DocumentGuid, "document_guid_unique")
                    .IsUnique();

                entity.Property(e => e.DocumentId)
                    .HasColumnType("int(11)")
                    .HasColumnName("documentId");

                entity.Property(e => e.AccessLogs)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("access_logs");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.DocumentGuid)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("document_guid");

                entity.Property(e => e.DocumentName)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("document_name");

                entity.Property(e => e.DocumentPath)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("document_path");

                entity.Property(e => e.DocumentType)
                    .IsRequired()
                    .HasColumnType("enum('Invoice','Receipt','Contract','PaySlip')")
                    .HasColumnName("document_type");

                entity.Property(e => e.EntityId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("entityId");

                entity.Property(e => e.FileExtension)
                    .IsRequired()
                    .HasColumnType("enum('.jpg','.png','.docx','.doc','.xlst','.pdf','.xml','.csv','.json','.jpeg','.xlsx','.accdb')")
                    .HasColumnName("file_extension");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");
            });

            modelBuilder.Entity<Dsicount>(entity =>
            {
                entity.HasKey(e => e.DiscountId)
                    .HasName("PRIMARY");

                entity.ToTable("dsicount");

                entity.HasIndex(e => e.Code, "code")
                    .IsUnique();

                entity.HasIndex(e => e.ItemId, "itemId")
                    .IsUnique();

                entity.HasIndex(e => e.UserId, "userId");

                entity.Property(e => e.DiscountId)
                    .HasMaxLength(36)
                    .HasColumnName("discountId");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("code");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.DueDate).HasColumnName("due_date");

                entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("itemId");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.Percentage)
                    .HasColumnType("int(11)")
                    .HasColumnName("percentage");

                entity.Property(e => e.Purchase)
                    .HasColumnType("int(11)")
                    .HasColumnName("purchase");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnType("enum('Open','Closed','Suspended')")
                    .HasColumnName("status");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.Item)
                    .WithOne(p => p.Dsicount)
                    .HasForeignKey<Dsicount>(d => d.ItemId)
                    .HasConstraintName("dsicount_ibfk_1");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Dsicounts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("dsicount_ibfk_2");
            });

            modelBuilder.Entity<Email>(entity =>
            {
                entity.ToTable("email");

                entity.Property(e => e.EmailId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("emailId");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("address");

                entity.Property(e => e.CanMailTo).HasColumnName("can_mail_to");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.IsPrimary).HasColumnName("is_primary");

                entity.Property(e => e.IsVerified).HasColumnName("is_verified");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnType("enum('Personal','School','Work')")
                    .HasColumnName("type");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("employee");

                entity.HasIndex(e => e.DepartmentId, "departmentId");

                entity.HasIndex(e => e.Email, "email")
                    .IsUnique();

                entity.HasIndex(e => e.SalaryId, "salaryId");

                entity.Property(e => e.EmployeeId)
                    .HasMaxLength(36)
                    .HasColumnName("employeeId");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("address");

                entity.Property(e => e.Contact)
                    .IsRequired()
                    .HasMaxLength(9)
                    .HasColumnName("contact");

                entity.Property(e => e.DateEmployed).HasColumnName("date_employed");

                entity.Property(e => e.DateOfBirth)
                    .HasColumnType("date")
                    .HasColumnName("date_of_birth");

                entity.Property(e => e.DepartmentId)
                    .HasColumnType("int(11)")
                    .HasColumnName("departmentId")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("first_name");

                entity.Property(e => e.ImagePath)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("image_path");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("last_name");

                entity.Property(e => e.NidNumber)
                    .HasColumnType("int(20)")
                    .HasColumnName("nid_number");

                entity.Property(e => e.PlaceOfBirth)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("place_of_birth");

                entity.Property(e => e.SalaryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("salaryId")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnType("enum('active','suspended','fired','sanctioned','volunteered')")
                    .HasColumnName("status")
                    .HasDefaultValueSql("'''active'''");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("employee_ibfk_2");

                entity.HasOne(d => d.Salary)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.SalaryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("employee_ibfk_1");
            });

            modelBuilder.Entity<EmployeeCity>(entity =>
            {
                entity.ToTable("employee_city");

                entity.HasIndex(e => e.CityId, "cityId");

                entity.HasIndex(e => new { e.EmployeeId, e.CityId }, "employeeId")
                    .IsUnique();

                entity.HasIndex(e => e.EmployeeId, "employeeId_2")
                    .IsUnique();

                entity.Property(e => e.EmployeeCityId)
                    .HasColumnType("int(11)")
                    .HasColumnName("employee_cityId");

                entity.Property(e => e.CityId)
                    .HasColumnType("int(11)")
                    .HasColumnName("cityId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("employeeId");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.EmployeeCities)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("employee_city_ibfk_2");

                entity.HasOne(d => d.Employee)
                    .WithOne(p => p.EmployeeCity)
                    .HasForeignKey<EmployeeCity>(d => d.EmployeeId)
                    .HasConstraintName("employee_city_ibfk_1");
            });

            modelBuilder.Entity<EmployeeCountry>(entity =>
            {
                entity.ToTable("employee_country");

                entity.HasIndex(e => new { e.EmployeeId, e.CountryId }, "employeeId")
                    .IsUnique();

                entity.HasIndex(e => e.CountryId, "employee_country_ibfk_2")
                    .IsUnique();

                entity.Property(e => e.EmployeeCountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("employee_countryId");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("countryId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("employeeId");

                entity.HasOne(d => d.Country)
                    .WithOne(p => p.EmployeeCountry)
                    .HasForeignKey<EmployeeCountry>(d => d.CountryId)
                    .HasConstraintName("employee_country_ibfk_2");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.EmployeeCountries)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("employee_country_ibfk_1");
            });

            modelBuilder.Entity<EmployeeEmail>(entity =>
            {
                entity.ToTable("employee_email");

                entity.HasIndex(e => e.EmailId, "emailId");

                entity.HasIndex(e => e.EmployeeId, "employeeId");

                entity.HasIndex(e => new { e.EmployeeId, e.EmailId }, "employeeId_2")
                    .IsUnique();

                entity.Property(e => e.EmployeeEmailId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("employee_emailId");

                entity.Property(e => e.EmailId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("emailId");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("employeeId");

                entity.HasOne(d => d.Email)
                    .WithMany(p => p.EmployeeEmails)
                    .HasForeignKey(d => d.EmailId)
                    .HasConstraintName("employee_email_ibfk_2");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.EmployeeEmails)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("employee_email_ibfk_1");
            });

            modelBuilder.Entity<EmployeeLocation>(entity =>
            {
                entity.ToTable("employee_location");

                entity.HasIndex(e => e.EmployeeId, "employeeId");

                entity.HasIndex(e => new { e.EmployeeId, e.LocationId }, "employeeId_2")
                    .IsUnique();

                entity.HasIndex(e => e.LocationId, "locationId");

                entity.Property(e => e.EmployeeLocationId)
                    .HasColumnType("int(11)")
                    .HasColumnName("employee_locationId");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("employeeId");

                entity.Property(e => e.LocationId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("locationId");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.EmployeeLocations)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("employee_location_ibfk_1");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.EmployeeLocations)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("employee_location_ibfk_2");
            });

            modelBuilder.Entity<EmployeePhone>(entity =>
            {
                entity.ToTable("employee_phone");

                entity.HasIndex(e => e.EmployeeId, "employeeId");

                entity.HasIndex(e => new { e.EmployeeId, e.PhoneId }, "employeeId_2")
                    .IsUnique();

                entity.HasIndex(e => e.PhoneId, "phoneId");

                entity.Property(e => e.EmployeePhoneId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("employee_phoneId");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("employeeId");

                entity.Property(e => e.PhoneId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("phoneId");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.EmployeePhones)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("employee_phone_ibfk_1");

                entity.HasOne(d => d.Phone)
                    .WithMany(p => p.EmployeePhones)
                    .HasForeignKey(d => d.PhoneId)
                    .HasConstraintName("employee_phone_ibfk_2");
            });

            modelBuilder.Entity<EmployeePrivilege>(entity =>
            {
                entity.ToTable("employee_privilege");

                entity.HasIndex(e => e.EmployeeId, "employeeId");

                entity.HasIndex(e => e.PrivilegeId, "privilegeId");

                entity.Property(e => e.EmployeePrivilegeId)
                    .HasColumnType("int(11)")
                    .HasColumnName("employee_privilegeId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("employeeId");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.PrivilegeId)
                    .HasColumnType("int(11)")
                    .HasColumnName("privilegeId");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.EmployeePrivileges)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("employee_privilege_ibfk_1");

                entity.HasOne(d => d.Privilege)
                    .WithMany(p => p.EmployeePrivileges)
                    .HasForeignKey(d => d.PrivilegeId)
                    .HasConstraintName("employee_privilege_ibfk_2");
            });

            modelBuilder.Entity<EmployeePrivilegeAction>(entity =>
            {
                entity.ToTable("employee_privilege_action");

                entity.HasIndex(e => e.ActionAuthorUserId, "action_author_userId");

                entity.HasIndex(e => e.EmployeePrivilegeId, "employee_privilegeId");

                entity.Property(e => e.EmployeePrivilegeActionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("employee_privilege_actionId");

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasColumnType("enum('Grant','Revoke')")
                    .HasColumnName("action");

                entity.Property(e => e.ActionAuthorUserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("action_author_userId");

                entity.Property(e => e.ActionDate).HasColumnName("action_date");

                entity.Property(e => e.EmployeePrivilegeId)
                    .HasColumnType("int(11)")
                    .HasColumnName("employee_privilegeId");

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("reason");

                entity.HasOne(d => d.ActionAuthorUser)
                    .WithMany(p => p.EmployeePrivilegeActions)
                    .HasForeignKey(d => d.ActionAuthorUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("employee_privilege_action_ibfk_2");

                entity.HasOne(d => d.EmployeePrivilege)
                    .WithMany(p => p.EmployeePrivilegeActions)
                    .HasForeignKey(d => d.EmployeePrivilegeId)
                    .HasConstraintName("employee_privilege_action_ibfk_1");
            });

            modelBuilder.Entity<EmployeeRegion>(entity =>
            {
                entity.ToTable("employee_region");

                entity.HasIndex(e => new { e.EmployeeId, e.RegionId }, "employeeId")
                    .IsUnique();

                entity.HasIndex(e => e.EmployeeId, "employeeId_2")
                    .IsUnique();

                entity.HasIndex(e => e.RegionId, "regionId");

                entity.Property(e => e.EmployeeRegionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("employee_regionId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.EmployeeId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("employeeId");

                entity.Property(e => e.RegionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("regionId");

                entity.HasOne(d => d.Employee)
                    .WithOne(p => p.EmployeeRegion)
                    .HasForeignKey<EmployeeRegion>(d => d.EmployeeId)
                    .HasConstraintName("employee_region_ibfk_1");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.EmployeeRegions)
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("employee_region_ibfk_2");
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("invoice");

                entity.HasIndex(e => e.CustomerId, "customerId");

                entity.HasIndex(e => e.UserId, "userId");

                entity.Property(e => e.InvoiceId)
                    .HasMaxLength(36)
                    .HasColumnName("invoiceId");

                entity.Property(e => e.AmountTendered).HasColumnName("amount_tendered");

                entity.Property(e => e.BankAccountName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("bank_account_name");

                entity.Property(e => e.BankAccountNumber)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("bank_account_number");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("customerId");

                entity.Property(e => e.DateRecorded)
                    .HasColumnName("date_recorded")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.Paid).HasColumnName("paid");

                entity.Property(e => e.PaymentType)
                    .IsRequired()
                    .HasColumnType("enum('Cash','MTN_Momo','Orange_Momo')")
                    .HasColumnName("payment_type")
                    .HasDefaultValueSql("'''Cash'''");

                entity.Property(e => e.SalesIdList)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("sales_id_list");

                entity.Property(e => e.TotalAmount).HasColumnName("total_amount");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("invoice_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("invoice_ibfk_1");
            });

            modelBuilder.Entity<InvoiceTender>(entity =>
            {
                entity.ToTable("invoice_tender");

                entity.HasIndex(e => e.InvoiceId, "invoiceId");

                entity.Property(e => e.InvoiceTenderId)
                    .HasMaxLength(36)
                    .HasColumnName("invoice_tenderId");

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.DateTendered).HasColumnName("date_tendered");

                entity.Property(e => e.InvoiceId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("invoiceId");

                entity.Property(e => e.TenderCode)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("tender_code");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.InvoiceTenders)
                    .HasForeignKey(d => d.InvoiceId)
                    .HasConstraintName("invoice_tender_ibfk_1");
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("item");

                entity.HasIndex(e => e.CategoryId, "categoryId");

                entity.HasIndex(e => e.ManufacturerId, "manufacturerId");

                entity.HasIndex(e => e.UnitId, "unitId");

                entity.Property(e => e.ItemId)
                    .HasMaxLength(36)
                    .HasColumnName("itemId");

                entity.Property(e => e.CategoryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("categoryId")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Code)
                    .HasMaxLength(8)
                    .HasColumnName("code")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.DateCreated)
                    .HasColumnName("date_created")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.DiscountPercentage)
                    .HasColumnType("int(11)")
                    .HasColumnName("discount_percentage")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.ImagePath)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("image_path");

                entity.Property(e => e.InStock)
                    .HasColumnType("int(11)")
                    .HasColumnName("in_stock");

                entity.Property(e => e.ItemName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("item_name");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.ManufacturerId)
                    .HasMaxLength(36)
                    .HasColumnName("manufacturerId")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.ReorderLevel)
                    .HasColumnType("int(11)")
                    .HasColumnName("reorder_level")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnType("enum('OnShelf','OffShelf')")
                    .HasColumnName("status");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("type");

                entity.Property(e => e.UnitId)
                    .HasColumnType("int(11)")
                    .HasColumnName("unitId")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.UnitPrice).HasColumnName("unit_price");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("item_ibfk_1");

                entity.HasOne(d => d.Manufacturer)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.ManufacturerId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("item_ibfk_3");

                entity.HasOne(d => d.Unit)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("item_ibfk_2");
            });

            modelBuilder.Entity<ItemCategory>(entity =>
            {
                entity.ToTable("item_category");

                entity.HasIndex(e => e.CategoryId, "categoryId");

                entity.HasIndex(e => new { e.ItemId, e.CategoryId }, "itemId")
                    .IsUnique();

                entity.Property(e => e.ItemCategoryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("item_categoryId");

                entity.Property(e => e.CategoryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("categoryId");

                entity.Property(e => e.DateAdded).HasColumnName("date_added");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("itemId");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.ItemCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("item_category_ibfk_2");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemCategories)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("item_category_ibfk_1");
            });

            modelBuilder.Entity<ItemCode>(entity =>
            {
                entity.ToTable("item_code");

                entity.HasIndex(e => e.Code, "code")
                    .IsUnique();

                entity.HasIndex(e => e.ItemId, "itemId");

                entity.Property(e => e.ItemCodeId)
                    .HasColumnType("int(11)")
                    .HasColumnName("item_codeId");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("code");

                entity.Property(e => e.ImagePath)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("image_path");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("itemId");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnType("enum('BarCode','QrCode')")
                    .HasColumnName("type");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemCodes)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("item_code_ibfk_1");
            });

            modelBuilder.Entity<ItemExpiry>(entity =>
            {
                entity.ToTable("item_expiry");

                entity.HasIndex(e => e.ItemId, "itemId")
                    .IsUnique();

                entity.Property(e => e.ItemExpiryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("item_expiryId");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");

                entity.Property(e => e.IsEnforced).HasColumnName("is_enforced");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("itemId");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.HasOne(d => d.Item)
                    .WithOne(p => p.ItemExpiry)
                    .HasForeignKey<ItemExpiry>(d => d.ItemId)
                    .HasConstraintName("item_expiry_ibfk_1");
            });

            modelBuilder.Entity<ItemsOrder>(entity =>
            {
                entity.HasKey(e => e.OrderId)
                    .HasName("PRIMARY");

                entity.ToTable("items_order");

                entity.HasIndex(e => e.ItemId, "items_order_ibfk_1");

                entity.HasIndex(e => e.SupplierId, "supplierId");

                entity.HasIndex(e => e.UserId, "userId");

                entity.Property(e => e.OrderId)
                    .HasMaxLength(36)
                    .HasColumnName("orderId");

                entity.Property(e => e.Cost).HasColumnName("cost");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("itemId");

                entity.Property(e => e.OrderDate).HasColumnName("order_date");

                entity.Property(e => e.Quantity)
                    .HasColumnType("int(11)")
                    .HasColumnName("quantity");

                entity.Property(e => e.ReceivedDate).HasColumnName("received_date");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnType("enum('Received','NotReceived')")
                    .HasColumnName("status");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("supplierId");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemsOrders)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("items_order_ibfk_1");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.ItemsOrders)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("items_order_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ItemsOrders)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("items_order_ibfk_3");
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.ToTable("language");

                entity.HasIndex(e => e.CountryId, "language_country");

                entity.Property(e => e.LanguageId)
                    .HasColumnType("int(11)")
                    .HasColumnName("languageId");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("countryId")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("name");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Languages)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("language_country");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("location");

                entity.Property(e => e.LocationId)
                    .HasMaxLength(36)
                    .HasColumnName("locationId");

                entity.Property(e => e.Latitude).HasColumnName("latitude");

                entity.Property(e => e.Longitude).HasColumnName("longitude");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.ToTable("manufacturer");

                entity.HasIndex(e => e.ManufacturerCode, "code")
                    .IsUnique();

                entity.Property(e => e.ManufacturerId)
                    .HasMaxLength(36)
                    .HasColumnName("manufacturerId");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("address");

                entity.Property(e => e.Company)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("company");

                entity.Property(e => e.Contact)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("contact");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("email");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.LogoPath)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("logo_path");

                entity.Property(e => e.ManufacturerCode)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("manufacturer_code");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Website)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("website");
            });

            modelBuilder.Entity<ManufacturerCity>(entity =>
            {
                entity.ToTable("manufacturer_city");

                entity.HasIndex(e => e.CityId, "cityId");

                entity.HasIndex(e => new { e.ManufacturerId, e.CityId }, "manufacturerId")
                    .IsUnique();

                entity.HasIndex(e => e.ManufacturerId, "manufacturerId_2")
                    .IsUnique();

                entity.Property(e => e.ManufacturerCityId)
                    .HasColumnType("int(11)")
                    .HasColumnName("manufacturer_cityId");

                entity.Property(e => e.CityId)
                    .HasColumnType("int(11)")
                    .HasColumnName("cityId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.ManufacturerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("manufacturerId");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.ManufacturerCities)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("manufacturer_city_ibfk_2");

                entity.HasOne(d => d.Manufacturer)
                    .WithOne(p => p.ManufacturerCity)
                    .HasForeignKey<ManufacturerCity>(d => d.ManufacturerId)
                    .HasConstraintName("manufacturer_city_ibfk_1");
            });

            modelBuilder.Entity<ManufacturerCountry>(entity =>
            {
                entity.ToTable("manufacturer_country");

                entity.HasIndex(e => new { e.ManufacturerId, e.CountryId }, "manufacturerId")
                    .IsUnique();

                entity.HasIndex(e => e.CountryId, "manufacturer_country_ibfk_2")
                    .IsUnique();

                entity.Property(e => e.ManufacturerCountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("manufacturer_countryId");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("countryId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.ManufacturerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("manufacturerId");

                entity.HasOne(d => d.Country)
                    .WithOne(p => p.ManufacturerCountry)
                    .HasForeignKey<ManufacturerCountry>(d => d.CountryId)
                    .HasConstraintName("manufacturer_country_ibfk_2");

                entity.HasOne(d => d.Manufacturer)
                    .WithMany(p => p.ManufacturerCountries)
                    .HasForeignKey(d => d.ManufacturerId)
                    .HasConstraintName("manufacturer_country_ibfk_1");
            });

            modelBuilder.Entity<ManufacturerEmail>(entity =>
            {
                entity.ToTable("manufacturer_email");

                entity.HasIndex(e => e.EmailId, "emailId");

                entity.HasIndex(e => e.ManufacturerId, "manufacturerId");

                entity.HasIndex(e => new { e.ManufacturerId, e.EmailId }, "manufacturerId_2")
                    .IsUnique();

                entity.Property(e => e.ManufacturerEmailId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("manufacturer_emailId");

                entity.Property(e => e.EmailId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("emailId");

                entity.Property(e => e.ManufacturerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("manufacturerId");

                entity.HasOne(d => d.Email)
                    .WithMany(p => p.ManufacturerEmails)
                    .HasForeignKey(d => d.EmailId)
                    .HasConstraintName("manufacturer_email_ibfk_2");

                entity.HasOne(d => d.Manufacturer)
                    .WithMany(p => p.ManufacturerEmails)
                    .HasForeignKey(d => d.ManufacturerId)
                    .HasConstraintName("manufacturer_email_ibfk_1");
            });

            modelBuilder.Entity<ManufacturerLocation>(entity =>
            {
                entity.ToTable("manufacturer_location");

                entity.HasIndex(e => e.LocationId, "locationId");

                entity.HasIndex(e => new { e.ManufacturerId, e.LocationId }, "manufacturerId")
                    .IsUnique();

                entity.Property(e => e.ManufacturerLocationId)
                    .HasColumnType("int(11)")
                    .HasColumnName("manufacturer_locationId");

                entity.Property(e => e.LocationId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("locationId");

                entity.Property(e => e.ManufacturerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("manufacturerId");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.ManufacturerLocations)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("manufacturer_location_ibfk_2");

                entity.HasOne(d => d.Manufacturer)
                    .WithMany(p => p.ManufacturerLocations)
                    .HasForeignKey(d => d.ManufacturerId)
                    .HasConstraintName("manufacturer_location_ibfk_1");
            });

            modelBuilder.Entity<ManufacturerPhone>(entity =>
            {
                entity.ToTable("manufacturer_phone");

                entity.HasIndex(e => e.ManufacturerId, "manufacturerId");

                entity.HasIndex(e => new { e.ManufacturerId, e.PhoneId }, "manufacturerId_2")
                    .IsUnique();

                entity.HasIndex(e => e.PhoneId, "phoneId");

                entity.Property(e => e.ManufacturerPhoneId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("manufacturer_phoneId");

                entity.Property(e => e.ManufacturerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("manufacturerId");

                entity.Property(e => e.PhoneId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("phoneId");

                entity.HasOne(d => d.Manufacturer)
                    .WithMany(p => p.ManufacturerPhones)
                    .HasForeignKey(d => d.ManufacturerId)
                    .HasConstraintName("manufacturer_phone_ibfk_1");

                entity.HasOne(d => d.Phone)
                    .WithMany(p => p.ManufacturerPhones)
                    .HasForeignKey(d => d.PhoneId)
                    .HasConstraintName("manufacturer_phone_ibfk_2");
            });

            modelBuilder.Entity<ManufacturerRegion>(entity =>
            {
                entity.ToTable("manufacturer_region");

                entity.HasIndex(e => new { e.ManufacturerId, e.RegionId }, "manufacturerId")
                    .IsUnique();

                entity.HasIndex(e => e.ManufacturerId, "manufacturerId_2")
                    .IsUnique();

                entity.HasIndex(e => e.RegionId, "regionId");

                entity.Property(e => e.ManufacturerRegionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("manufacturer_regionId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.ManufacturerId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("manufacturerId");

                entity.Property(e => e.RegionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("regionId");

                entity.HasOne(d => d.Manufacturer)
                    .WithOne(p => p.ManufacturerRegion)
                    .HasForeignKey<ManufacturerRegion>(d => d.ManufacturerId)
                    .HasConstraintName("manufacturer_region_ibfk_1");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.ManufacturerRegions)
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("manufacturer_region_ibfk_2");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notification");

                entity.HasIndex(e => e.UserId, "accountId");

                entity.Property(e => e.NotificationId)
                    .HasMaxLength(36)
                    .HasColumnName("notificationId");

                entity.Property(e => e.Actions)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("actions");

                entity.Property(e => e.Caption)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("caption");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.DateRead).HasColumnName("date_read");

                entity.Property(e => e.Details)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("details");

                entity.Property(e => e.Icon)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("icon")
                    .HasDefaultValueSql("'''''''notify.png'''''''");

                entity.Property(e => e.MarkedAsRead).HasColumnName("marked_as_read");

                entity.Property(e => e.UserId)
                    .HasMaxLength(36)
                    .HasColumnName("userId")
                    .HasDefaultValueSql("'NULL'");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("notification_ibfk_1");
            });

            modelBuilder.Entity<Otp>(entity =>
            {
                entity.ToTable("otp");

                entity.HasIndex(e => new { e.Code, e.UserId }, "code")
                    .IsUnique();

                entity.HasIndex(e => e.Guid, "guid")
                    .IsUnique();

                entity.HasIndex(e => new { e.UserId, e.Guid }, "userId")
                    .IsUnique();

                entity.Property(e => e.OtpId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("otpId");

                entity.Property(e => e.Code)
                    .HasColumnType("int(6)")
                    .HasColumnName("code");

                entity.Property(e => e.ExpiryDate)
                    .HasColumnType("date")
                    .HasColumnName("expiry_date");

                entity.Property(e => e.Guid)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("guid");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Otps)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("reset_code");
            });

            modelBuilder.Entity<Password>(entity =>
            {
                entity.ToTable("password");

                entity.HasIndex(e => e.UserId, "userId");

                entity.Property(e => e.PasswordId)
                    .HasColumnType("int(11)")
                    .HasColumnName("passwordId");

                entity.Property(e => e.Hashed)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasColumnName("hashed");

                entity.Property(e => e.LastChanged).HasColumnName("last_changed");

                entity.Property(e => e.PastPasswords)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("past_passwords")
                    .HasComment("This field, takes a an object like \r\n\r\n[{\r\n \"hashed\" \"string\",\r\n \"salt\" : byte[]\r\n}\r\n]");

                entity.Property(e => e.Salt)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnName("salt");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Passwords)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("password_ibfk_1");
            });

            modelBuilder.Entity<Phone>(entity =>
            {
                entity.ToTable("phone");

                entity.HasIndex(e => e.CountryId, "countryId");

                entity.HasIndex(e => new { e.CountryId, e.Number }, "countryId_2")
                    .IsUnique();

                entity.Property(e => e.PhoneId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("phoneId");

                entity.Property(e => e.CanNotify).HasColumnName("can_notify");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("countryId");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.IsPrimary).HasColumnName("is_primary");

                entity.Property(e => e.IsVerified).HasColumnName("is_verified");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.Number)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("number");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnType("enum('Home','Mobile','Fax','Office','Work')")
                    .HasColumnName("type");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Phones)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("phone_ibfk_1");
            });

            modelBuilder.Entity<Privilege>(entity =>
            {
                entity.ToTable("privilege");

                entity.Property(e => e.PrivilegeId)
                    .HasColumnType("int(11)")
                    .HasColumnName("privilegeId");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("name");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnType("enum('Admin','Managerial','Assistive','Stakeholder','Employee','Directorial','System')")
                    .HasColumnName("type");
            });

            modelBuilder.Entity<Region>(entity =>
            {
                entity.ToTable("region");

                entity.HasIndex(e => e.Guid, "guid")
                    .IsUnique();

                entity.HasIndex(e => e.CountryId, "region_country");

                entity.Property(e => e.RegionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("regionId");

                entity.Property(e => e.CapitalCity)
                    .HasColumnType("int(11)")
                    .HasColumnName("capital_city");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("countryId");

                entity.Property(e => e.Guid)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("guid");

                entity.Property(e => e.Latitude)
                    .IsRequired()
                    .HasMaxLength(11)
                    .HasColumnName("latitude");

                entity.Property(e => e.Longitude)
                    .IsRequired()
                    .HasMaxLength(11)
                    .HasColumnName("longitude");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.ShortCode)
                    .IsRequired()
                    .HasMaxLength(5)
                    .HasColumnName("short_code");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Regions)
                    .HasForeignKey(d => d.CountryId)
                    .HasConstraintName("region_country");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("role");

                entity.Property(e => e.RoleId)
                    .HasColumnType("int(11)")
                    .HasColumnName("roleId");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("enum('User','Employee','Manager','Owner','Admin')")
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Salary>(entity =>
            {
                entity.ToTable("salary");

                entity.HasIndex(e => e.SalaryCode, "salary_code")
                    .IsUnique();

                entity.Property(e => e.SalaryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("salaryId");

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.SalaryCode)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("salary_code");

                entity.Property(e => e.SalaryLevel)
                    .IsRequired()
                    .HasColumnType("enum('Low','Medium','High','Boss')")
                    .HasColumnName("salary_level");
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.ToTable("sale");

                entity.HasIndex(e => e.UnitCode, "code")
                    .IsUnique();

                entity.HasIndex(e => e.InvoiceId, "invoiceId");

                entity.HasIndex(e => e.ItemId, "itemId");

                entity.HasIndex(e => e.UserId, "userId");

                entity.Property(e => e.SaleId)
                    .HasMaxLength(36)
                    .HasColumnName("saleId");

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.DateOfSale).HasColumnName("date_of_sale");

                entity.Property(e => e.InvoiceId)
                    .HasMaxLength(36)
                    .HasColumnName("invoiceId")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.ItemId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("itemId");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Quantity)
                    .HasColumnType("int(11)")
                    .HasColumnName("quantity");

                entity.Property(e => e.UnitCode)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("unit_code");

                entity.Property(e => e.UnitPrice).HasColumnName("unit_price");

                entity.Property(e => e.UserId)
                    .HasMaxLength(36)
                    .HasColumnName("userId")
                    .HasDefaultValueSql("'NULL'");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.InvoiceId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("sale_ibfk_3");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.ItemId)
                    .HasConstraintName("sale_ibfk_1");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("sale_ibfk_2");
            });

            modelBuilder.Entity<Sentmail>(entity =>
            {
                entity.HasKey(e => e.MailId)
                    .HasName("PRIMARY");

                entity.ToTable("sentmail");

                entity.Property(e => e.MailId)
                    .HasColumnType("int(11)")
                    .HasColumnName("mailId");

                entity.Property(e => e.DateSent).HasColumnName("date_sent");

                entity.Property(e => e.EmailBody)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("email_body");

                entity.Property(e => e.EmailType)
                    .IsRequired()
                    .HasColumnType("enum('VerifyEmail','Verified','Suspended','ResetPassword','Welcome')")
                    .HasColumnName("email_type");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("supplier");

                entity.HasIndex(e => e.SupplierCode, "code")
                    .IsUnique();

                entity.Property(e => e.SupplierId)
                    .HasMaxLength(36)
                    .HasColumnName("supplierId");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("address");

                entity.Property(e => e.Company)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("company");

                entity.Property(e => e.Contact)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("contact");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("email");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.LogoPath)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("logo_path");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.SupplierCode)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("supplier_code");

                entity.Property(e => e.Website)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("website");
            });

            modelBuilder.Entity<SupplierCity>(entity =>
            {
                entity.ToTable("supplier_city");

                entity.HasIndex(e => e.CityId, "cityId");

                entity.HasIndex(e => new { e.SupplierId, e.CityId }, "supplierId")
                    .IsUnique();

                entity.HasIndex(e => e.SupplierId, "supplierId_2")
                    .IsUnique();

                entity.Property(e => e.SupplierCityId)
                    .HasColumnType("int(11)")
                    .HasColumnName("supplier_cityId");

                entity.Property(e => e.CityId)
                    .HasColumnType("int(11)")
                    .HasColumnName("cityId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("supplierId");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.SupplierCities)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("supplier_city_ibfk_2");

                entity.HasOne(d => d.Supplier)
                    .WithOne(p => p.SupplierCity)
                    .HasForeignKey<SupplierCity>(d => d.SupplierId)
                    .HasConstraintName("supplier_city_ibfk_1");
            });

            modelBuilder.Entity<SupplierCountry>(entity =>
            {
                entity.ToTable("supplier_country");

                entity.HasIndex(e => new { e.SupplierId, e.CountryId }, "supplierId")
                    .IsUnique();

                entity.HasIndex(e => e.CountryId, "supplier_country_ibfk_2")
                    .IsUnique();

                entity.Property(e => e.SupplierCountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("supplier_countryId");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("countryId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("supplierId");

                entity.HasOne(d => d.Country)
                    .WithOne(p => p.SupplierCountry)
                    .HasForeignKey<SupplierCountry>(d => d.CountryId)
                    .HasConstraintName("supplier_country_ibfk_2");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.SupplierCountries)
                    .HasForeignKey(d => d.SupplierId)
                    .HasConstraintName("supplier_country_ibfk_1");
            });

            modelBuilder.Entity<SupplierEmail>(entity =>
            {
                entity.ToTable("supplier_email");

                entity.HasIndex(e => e.EmailId, "emailId");

                entity.HasIndex(e => e.SupplierId, "supplierId");

                entity.HasIndex(e => new { e.SupplierId, e.EmailId }, "supplierId_2")
                    .IsUnique();

                entity.Property(e => e.SupplierEmailId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("supplier_emailId");

                entity.Property(e => e.EmailId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("emailId");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("supplierId");

                entity.HasOne(d => d.Email)
                    .WithMany(p => p.SupplierEmails)
                    .HasForeignKey(d => d.EmailId)
                    .HasConstraintName("supplier_email_ibfk_2");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.SupplierEmails)
                    .HasForeignKey(d => d.SupplierId)
                    .HasConstraintName("supplier_email_ibfk_1");
            });

            modelBuilder.Entity<SupplierLocation>(entity =>
            {
                entity.ToTable("supplier_location");

                entity.HasIndex(e => e.LocationId, "locationId");

                entity.HasIndex(e => new { e.SupplierId, e.LocationId }, "supplierId")
                    .IsUnique();

                entity.Property(e => e.SupplierLocationId)
                    .HasColumnType("int(11)")
                    .HasColumnName("supplier_locationId");

                entity.Property(e => e.LocationId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("locationId");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("supplierId");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.SupplierLocations)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("supplier_location_ibfk_2");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.SupplierLocations)
                    .HasForeignKey(d => d.SupplierId)
                    .HasConstraintName("supplier_location_ibfk_1");
            });

            modelBuilder.Entity<SupplierPhone>(entity =>
            {
                entity.ToTable("supplier_phone");

                entity.HasIndex(e => e.PhoneId, "phoneId");

                entity.HasIndex(e => e.SupplierId, "supplierId");

                entity.HasIndex(e => new { e.SupplierId, e.PhoneId }, "supplierId_2")
                    .IsUnique();

                entity.Property(e => e.SupplierPhoneId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("supplier_phoneId");

                entity.Property(e => e.PhoneId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("phoneId");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("supplierId");

                entity.HasOne(d => d.Phone)
                    .WithMany(p => p.SupplierPhones)
                    .HasForeignKey(d => d.PhoneId)
                    .HasConstraintName("supplier_phone_ibfk_2");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.SupplierPhones)
                    .HasForeignKey(d => d.SupplierId)
                    .HasConstraintName("supplier_phone_ibfk_1");
            });

            modelBuilder.Entity<SupplierRegion>(entity =>
            {
                entity.ToTable("supplier_region");

                entity.HasIndex(e => e.RegionId, "regionId");

                entity.HasIndex(e => new { e.SupplierId, e.RegionId }, "supplierId")
                    .IsUnique();

                entity.HasIndex(e => e.SupplierId, "supplierId_2")
                    .IsUnique();

                entity.Property(e => e.SupplierRegionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("supplier_regionId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.RegionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("regionId");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("supplierId");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.SupplierRegions)
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("supplier_region_ibfk_2");

                entity.HasOne(d => d.Supplier)
                    .WithOne(p => p.SupplierRegion)
                    .HasForeignKey<SupplierRegion>(d => d.SupplierId)
                    .HasConstraintName("supplier_region_ibfk_1");
            });

            modelBuilder.Entity<Unit>(entity =>
            {
                entity.ToTable("unit");

                entity.Property(e => e.UnitId)
                    .HasColumnType("int(11)")
                    .HasColumnName("unitId");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnType("enum('Industrial','Home','Tertiary','Education')")
                    .HasColumnName("type");

                entity.Property(e => e.UnitCode)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("unit_code");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.EmployeeId, "employeeId");

                entity.HasIndex(e => e.RoleId, "roleId");

                entity.HasIndex(e => e.Username, "username")
                    .IsUnique();

                entity.HasIndex(e => e.Username, "username_2");

                entity.Property(e => e.UserId)
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.EmployeeId)
                    .HasMaxLength(36)
                    .HasColumnName("employeeId")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.ImagePath)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("image_path");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("password");

                entity.Property(e => e.RoleId)
                    .HasColumnType("int(11)")
                    .HasColumnName("roleId")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnType("enum('Active','Suspended','Banned','Deleted')")
                    .HasColumnName("status")
                    .HasDefaultValueSql("'''Active'''");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("username");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("user_ibfk_1");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("user_ibfk_2");
            });

            modelBuilder.Entity<UserCity>(entity =>
            {
                entity.ToTable("user_city");

                entity.HasIndex(e => e.CityId, "cityId");

                entity.HasIndex(e => new { e.UserId, e.CityId }, "userId")
                    .IsUnique();

                entity.HasIndex(e => e.UserId, "userId_2")
                    .IsUnique();

                entity.Property(e => e.UserCityId)
                    .HasColumnType("int(11)")
                    .HasColumnName("user_cityId");

                entity.Property(e => e.CityId)
                    .HasColumnType("int(11)")
                    .HasColumnName("cityId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.UserCities)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("user_city_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserCity)
                    .HasForeignKey<UserCity>(d => d.UserId)
                    .HasConstraintName("user_city_ibfk_1");
            });

            modelBuilder.Entity<UserCountry>(entity =>
            {
                entity.ToTable("user_country");

                entity.HasIndex(e => new { e.UserId, e.CountryId }, "userId")
                    .IsUnique();

                entity.HasIndex(e => e.CountryId, "user_country_ibfk_2")
                    .IsUnique();

                entity.Property(e => e.UserCountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("user_countryId");

                entity.Property(e => e.CountryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("countryId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.Country)
                    .WithOne(p => p.UserCountry)
                    .HasForeignKey<UserCountry>(d => d.CountryId)
                    .HasConstraintName("user_country_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserCountries)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("user_country_ibfk_1");
            });

            modelBuilder.Entity<UserEmail>(entity =>
            {
                entity.ToTable("user_email");

                entity.HasIndex(e => e.EmailId, "emailId");

                entity.HasIndex(e => e.UserId, "userId");

                entity.HasIndex(e => new { e.UserId, e.EmailId }, "userId_2")
                    .IsUnique();

                entity.Property(e => e.UserEmailId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("user_emailId");

                entity.Property(e => e.EmailId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("emailId");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.Email)
                    .WithMany(p => p.UserEmails)
                    .HasForeignKey(d => d.EmailId)
                    .HasConstraintName("user_email_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserEmails)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("user_email_ibfk_1");
            });

            modelBuilder.Entity<UserPhone>(entity =>
            {
                entity.ToTable("user_phone");

                entity.HasIndex(e => e.PhoneId, "phoneId");

                entity.HasIndex(e => e.UserId, "userId");

                entity.HasIndex(e => new { e.UserId, e.PhoneId }, "userId_2")
                    .IsUnique();

                entity.Property(e => e.UserPhoneId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("user_phoneId");

                entity.Property(e => e.PhoneId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("phoneId");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.Phone)
                    .WithMany(p => p.UserPhones)
                    .HasForeignKey(d => d.PhoneId)
                    .HasConstraintName("user_phone_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserPhones)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("user_phone_ibfk_1");
            });

            modelBuilder.Entity<UserPrivilege>(entity =>
            {
                entity.ToTable("user_privilege");

                entity.HasIndex(e => e.PrivilegeId, "privilegeId");

                entity.HasIndex(e => e.UserId, "userId");

                entity.Property(e => e.UserPrivilegeId)
                    .HasColumnType("int(11)")
                    .HasColumnName("user_privilegeId");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.DateGranted).HasColumnName("date_granted");

                entity.Property(e => e.DateRevoked).HasColumnName("date_revoked");

                entity.Property(e => e.IsRevoked).HasColumnName("is_revoked");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.PrivilegeId)
                    .HasColumnType("int(11)")
                    .HasColumnName("privilegeId");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.Privilege)
                    .WithMany(p => p.UserPrivileges)
                    .HasForeignKey(d => d.PrivilegeId)
                    .HasConstraintName("user_privilege_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserPrivileges)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("user_privilege_ibfk_1");
            });

            modelBuilder.Entity<UserPrivilegeAction>(entity =>
            {
                entity.ToTable("user_privilege_action");

                entity.HasIndex(e => e.ActionAuthorUserId, "action_author_userId");

                entity.HasIndex(e => e.UserPrivilegeId, "user_privilegeId");

                entity.Property(e => e.UserPrivilegeActionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("user_privilege_actionId");

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasColumnType("enum('Grant','Revoke')")
                    .HasColumnName("action");

                entity.Property(e => e.ActionAuthorUserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("action_author_userId");

                entity.Property(e => e.ActionDate).HasColumnName("action_date");

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasMaxLength(250)
                    .HasColumnName("reason");

                entity.Property(e => e.UserPrivilegeId)
                    .HasColumnType("int(11)")
                    .HasColumnName("user_privilegeId");

                entity.HasOne(d => d.ActionAuthorUser)
                    .WithMany(p => p.UserPrivilegeActions)
                    .HasForeignKey(d => d.ActionAuthorUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_privilege_action_ibfk_2");

                entity.HasOne(d => d.UserPrivilege)
                    .WithMany(p => p.UserPrivilegeActions)
                    .HasForeignKey(d => d.UserPrivilegeId)
                    .HasConstraintName("user_privilege_action_ibfk_1");
            });

            modelBuilder.Entity<UserRegion>(entity =>
            {
                entity.ToTable("user_region");

                entity.HasIndex(e => e.RegionId, "regionId");

                entity.HasIndex(e => new { e.UserId, e.RegionId }, "userId")
                    .IsUnique();

                entity.HasIndex(e => e.UserId, "userId_2")
                    .IsUnique();

                entity.Property(e => e.UserRegionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("user_regionId");

                entity.Property(e => e.DateSet).HasColumnName("date_set");

                entity.Property(e => e.RegionId)
                    .HasColumnType("int(11)")
                    .HasColumnName("regionId");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.UserRegions)
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("user_region_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserRegion)
                    .HasForeignKey<UserRegion>(d => d.UserId)
                    .HasConstraintName("user_region_ibfk_1");
            });

            modelBuilder.Entity<UserToken>(entity =>
            {
                entity.ToTable("user_token");

                entity.HasIndex(e => e.UserId, "userId")
                    .IsUnique();

                entity.Property(e => e.UserTokenId)
                    .HasColumnType("int(11)")
                    .HasColumnName("user_tokenId");

                entity.Property(e => e.DateCreated).HasColumnName("date_created");

                entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");

                entity.Property(e => e.LastModified).HasColumnName("last_modified");

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnName("token");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(36)
                    .HasColumnName("userId");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserToken)
                    .HasForeignKey<UserToken>(d => d.UserId)
                    .HasConstraintName("user_token_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
