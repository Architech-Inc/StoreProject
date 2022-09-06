using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace StoreProjectModels.DatabaseModels
{
    public partial class store_dbContext : DbContext
    {
        public store_dbContext()
        {
        }

        public store_dbContext(DbContextOptions<store_dbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Costumer> Costumers { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<ItemsOrder> ItemsOrders { get; set; }
        public virtual DbSet<Salary> Salaries { get; set; }
        public virtual DbSet<Sale> Sales { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<Unit> Units { get; set; }
        public virtual DbSet<User> Users { get; set; }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            if (!optionsBuilder.IsConfigured)
//            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//                optionsBuilder.UseMySQL("server=localhost;port=3306;database=store_db;user=root;password=");
//            }
//        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("category");

                entity.Property(e => e.CategoryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("categoryID");

                entity.Property(e => e.ImgBase64)
                    .IsRequired()
                    .HasColumnType("longtext");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.Property(e => e.NoP).HasColumnType("int(11)");
            });

            modelBuilder.Entity<Costumer>(entity =>
            {
                entity.ToTable("costumer");

                entity.HasIndex(e => e.NidNumber, "nid_number")
                    .IsUnique();

                entity.Property(e => e.CostumerId)
                    .HasMaxLength(100)
                    .HasColumnName("costumerID");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("address");

                entity.Property(e => e.Contact)
                    .IsRequired()
                    .HasMaxLength(9)
                    .HasColumnName("contact");

                entity.Property(e => e.DateRegistered)
                    .HasColumnName("date_registered")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(25)
                    .HasColumnName("name");

                entity.Property(e => e.NidNumber)
                    .HasColumnType("int(11)")
                    .HasColumnName("nid_number");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DeptId)
                    .HasName("PRIMARY");

                entity.ToTable("department");

                entity.Property(e => e.DeptId)
                    .HasColumnType("int(11)")
                    .HasColumnName("deptID");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("employee");

                entity.HasIndex(e => e.DeptId, "department");

                entity.HasIndex(e => e.SalaryId, "is_paid");

                entity.Property(e => e.EmployeeId)
                    .HasMaxLength(100)
                    .HasColumnName("employeeID");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("address");

                entity.Property(e => e.Contact)
                    .IsRequired()
                    .HasMaxLength(9)
                    .HasColumnName("contact");

                entity.Property(e => e.DateEmployed).HasColumnName("date_employed");

                entity.Property(e => e.DeptId)
                    .HasColumnType("int(11)")
                    .HasColumnName("deptID")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Dob)
                    .HasColumnType("date")
                    .HasColumnName("dob");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("first_name");

                entity.Property(e => e.ImgBase64)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("imgBase64");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("last_name");

                entity.Property(e => e.NidNumber)
                    .HasColumnType("int(20)")
                    .HasColumnName("nid_number");

                entity.Property(e => e.Pob)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("pob");

                entity.Property(e => e.SalaryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("salaryID")
                    .HasDefaultValueSql("'NULL'");

                entity.HasOne(d => d.Dept)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.DeptId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("department");

                entity.HasOne(d => d.Salary)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(d => d.SalaryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("is_paid");
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("invoice");

                entity.HasIndex(e => e.CostumerId, "costumer");

                entity.HasIndex(e => e.UserId, "user");

                entity.Property(e => e.InvoiceId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("invoiceID");

                entity.Property(e => e.AmountTendered).HasColumnName("amount_tendered");

                entity.Property(e => e.BankAccountName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("bank_account_name");

                entity.Property(e => e.BankAccountNumber)
                    .HasColumnType("int(30)")
                    .HasColumnName("bank_account_number");

                entity.Property(e => e.CostumerId)
                    .HasMaxLength(100)
                    .HasColumnName("costumerID")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.DateRecorded)
                    .HasColumnName("date_recorded")
                    .HasDefaultValueSql("'current_timestamp()'");

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
                    .HasMaxLength(100)
                    .HasColumnName("userID");

                entity.HasOne(d => d.Costumer)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.CostumerId)
                    .HasConstraintName("costumer");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user");
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.ToTable("items");

                entity.HasIndex(e => e.CategoryId, "category");

                entity.HasIndex(e => e.ItemCode, "item_code")
                    .IsUnique();

                entity.HasIndex(e => e.UnitId, "unit_under");

                entity.Property(e => e.ItemId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("itemID");

                entity.Property(e => e.CategoryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("categoryID")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.DateCreated)
                    .HasColumnName("date_created")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.DiscountPercentage)
                    .HasColumnType("int(11)")
                    .HasColumnName("discount_percentage")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.ImgBase64)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("imgBase64");

                entity.Property(e => e.InStock)
                    .HasColumnType("int(11)")
                    .HasColumnName("in_stock");

                entity.Property(e => e.ItemCode)
                    .HasColumnType("int(11)")
                    .HasColumnName("item_code")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.ItemName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("item_name");

                entity.Property(e => e.ItemType)
                    .IsRequired()
                    .HasMaxLength(15)
                    .HasColumnName("item_type");

                entity.Property(e => e.ReoderLevel)
                    .HasColumnType("int(11)")
                    .HasColumnName("reoder_level")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.UnitId)
                    .HasColumnType("int(11)")
                    .HasColumnName("unitID")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.UnitPrice).HasColumnName("unit_price");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("category");

                entity.HasOne(d => d.Unit)
                    .WithMany(p => p.Items)
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("unit_under");
            });

            modelBuilder.Entity<ItemsOrder>(entity =>
            {
                entity.HasKey(e => e.OrderId)
                    .HasName("PRIMARY");

                entity.ToTable("items_order");

                entity.HasIndex(e => e.ItemId, "ordered_item");

                entity.HasIndex(e => e.UserId, "who_received");

                entity.HasIndex(e => e.SupplierId, "who_supplied");

                entity.Property(e => e.OrderId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("orderID");

                entity.Property(e => e.Cost).HasColumnName("cost");

                entity.Property(e => e.ItemId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("itemID");

                entity.Property(e => e.OrderDate).HasColumnName("order_date");

                entity.Property(e => e.Quantity)
                    .HasColumnType("int(11)")
                    .HasColumnName("quantity");

                entity.Property(e => e.ReceivedDate).HasColumnName("received_date");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasColumnType("enum('received','not_received')")
                    .HasColumnName("status");

                entity.Property(e => e.SupplierId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("supplierID");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("userID");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.ItemsOrders)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ordered_item");

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.ItemsOrders)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("who_supplied");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ItemsOrders)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("who_received");
            });

            modelBuilder.Entity<Salary>(entity =>
            {
                entity.ToTable("salary");

                entity.Property(e => e.SalaryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("salaryID");

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("code");
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.ToTable("sale");

                entity.HasIndex(e => e.InvoiceId, "in_invoice");

                entity.HasIndex(e => e.ItemId, "which_item");

                entity.HasIndex(e => e.UserId, "who_sold");

                entity.Property(e => e.SaleId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("saleID");

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.InvoiceId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("invoiceID")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.ItemId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("itemID");

                entity.Property(e => e.ItemName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("item_name");

                entity.Property(e => e.Quantity)
                    .HasColumnType("int(11)")
                    .HasColumnName("quantity");

                entity.Property(e => e.SaleDate)
                    .HasColumnName("sale_date")
                    .HasDefaultValueSql("'current_timestamp()'");

                entity.Property(e => e.UnitPrice).HasColumnName("unit_price");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("userID");

                entity.HasOne(d => d.Invoice)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.InvoiceId)
                    .HasConstraintName("in_invoice");

                entity.HasOne(d => d.Item)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.ItemId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("which_item");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("who_sold");
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.ToTable("supplier");

                entity.HasIndex(e => e.Code, "code")
                    .IsUnique();

                entity.Property(e => e.SupplierId)
                    .HasMaxLength(100)
                    .HasColumnName("supplierID");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("address");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("code");

                entity.Property(e => e.Company)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("company");

                entity.Property(e => e.Contact)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("contact");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("email");

                entity.Property(e => e.LogoBase64)
                    .IsRequired()
                    .HasColumnType("longtext")
                    .HasColumnName("logoBase64");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Website)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("website");
            });

            modelBuilder.Entity<Unit>(entity =>
            {
                entity.ToTable("unit");

                entity.Property(e => e.UnitId)
                    .HasColumnType("int(11)")
                    .HasColumnName("unitID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(25)
                    .HasColumnName("name");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnType("enum('industrial','home','tertiary','education')")
                    .HasColumnName("type");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.EmployeeId, "employee");

                entity.HasIndex(e => e.UnitId, "unit_assigned");

                entity.Property(e => e.UserId)
                    .HasMaxLength(100)
                    .HasColumnName("userID");

                entity.Property(e => e.AccountType)
                    .IsRequired()
                    .HasColumnType("enum('admin','user','manager','supervisor')")
                    .HasColumnName("account_type")
                    .HasDefaultValueSql("'''user'''");

                entity.Property(e => e.EmployeeId)
                    .HasMaxLength(20)
                    .HasColumnName("employeeID")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("password");

                entity.Property(e => e.UnitId)
                    .HasColumnType("int(11)")
                    .HasColumnName("unitID")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("username");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("employee");

                entity.HasOne(d => d.Unit)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.UnitId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("unit_assigned");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
