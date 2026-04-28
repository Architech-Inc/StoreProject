using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class SnakeCaseNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Batches_Items_ItemId",
                table: "Batches");

            migrationBuilder.DropForeignKey(
                name: "FK_ChangeLogs_Users_UserId",
                table: "ChangeLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Regions_RegionId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerEmails_Customers_CustomerId",
                table: "CustomerEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerEmails_Emails_EmailId",
                table: "CustomerEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerLocations_Customers_CustomerId",
                table: "CustomerLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerLocations_Locations_LocationId",
                table: "CustomerLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPhones_Customers_CustomerId",
                table: "CustomerPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPhones_Phones_PhoneId",
                table: "CustomerPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPrivilegeActions_CustomerPrivileges_CustomerPrivileg~",
                table: "CustomerPrivilegeActions");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPrivilegeActions_Users_PerformedByUserId",
                table: "CustomerPrivilegeActions");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPrivileges_Customers_CustomerId",
                table: "CustomerPrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPrivileges_Privileges_PrivilegeId",
                table: "CustomerPrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_Discounts_Items_ItemId",
                table: "Discounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Discounts_Users_ManagedByUserId",
                table: "Discounts");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEmails_Emails_EmailId",
                table: "EmployeeEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeEmails_Employees_EmployeeId",
                table: "EmployeeEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeLocations_Employees_EmployeeId",
                table: "EmployeeLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeLocations_Locations_LocationId",
                table: "EmployeeLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePhones_Employees_EmployeeId",
                table: "EmployeePhones");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePhones_Phones_PhoneId",
                table: "EmployeePhones");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePrivilegeActions_EmployeePrivileges_EmployeePrivileg~",
                table: "EmployeePrivilegeActions");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePrivilegeActions_Users_PerformedByUserId",
                table: "EmployeePrivilegeActions");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePrivileges_Employees_EmployeeId",
                table: "EmployeePrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeePrivileges_Privileges_PrivilegeId",
                table: "EmployeePrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Salaries_SalaryId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Users_UserId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceTenders_Invoices_InvoiceId",
                table: "InvoiceTenders");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategories_Categories_CategoryId",
                table: "ItemCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategories_Items_ItemId",
                table: "ItemCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCodes_Items_ItemId",
                table: "ItemCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemExpiries_Items_ItemId",
                table: "ItemExpiries");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Categories_CategoryId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Manufacturers_ManufacturerId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Units_UnitId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemsOrders_Suppliers_SupplierId",
                table: "ItemsOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemsOrders_Users_CreatedByUserId",
                table: "ItemsOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Cities_CityId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_ManufacturerEmails_Emails_EmailId",
                table: "ManufacturerEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_ManufacturerEmails_Manufacturers_ManufacturerId",
                table: "ManufacturerEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_ManufacturerLocations_Locations_LocationId",
                table: "ManufacturerLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ManufacturerLocations_Manufacturers_ManufacturerId",
                table: "ManufacturerLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ManufacturerPhones_Manufacturers_ManufacturerId",
                table: "ManufacturerPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_ManufacturerPhones_Phones_PhoneId",
                table: "ManufacturerPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ItemsOrders_ItemsOrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Items_ItemId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Otps_Users_UserId",
                table: "Otps");

            migrationBuilder.DropForeignKey(
                name: "FK_Regions_Countries_CountryId",
                table: "Regions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Invoices_InvoiceId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Items_ItemId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Users_UserId",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierEmails_Emails_EmailId",
                table: "SupplierEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierEmails_Suppliers_SupplierId",
                table: "SupplierEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierLocations_Locations_LocationId",
                table: "SupplierLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierLocations_Suppliers_SupplierId",
                table: "SupplierLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierPhones_Phones_PhoneId",
                table: "SupplierPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierPhones_Suppliers_SupplierId",
                table: "SupplierPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEmails_Emails_EmailId",
                table: "UserEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_UserEmails_Users_UserId",
                table: "UserEmails");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPasswords_Users_UserId",
                table: "UserPasswords");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPhones_Phones_PhoneId",
                table: "UserPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPhones_Users_UserId",
                table: "UserPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrivilegeActions_UserPrivileges_UserPrivilegeId",
                table: "UserPrivilegeActions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrivilegeActions_Users_PerformedByUserId",
                table: "UserPrivilegeActions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrivileges_Privileges_PrivilegeId",
                table: "UserPrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPrivileges_Users_UserId",
                table: "UserPrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Employees_EmployeeId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPrivileges",
                table: "UserPrivileges");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPrivilegeActions",
                table: "UserPrivilegeActions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPhones",
                table: "UserPhones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPasswords",
                table: "UserPasswords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserEmails",
                table: "UserEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Units",
                table: "Units");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Suppliers",
                table: "Suppliers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupplierPhones",
                table: "SupplierPhones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupplierLocations",
                table: "SupplierLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupplierEmails",
                table: "SupplierEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sales",
                table: "Sales");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Salaries",
                table: "Salaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Regions",
                table: "Regions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Privileges",
                table: "Privileges");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Phones",
                table: "Phones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Otps",
                table: "Otps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Manufacturers",
                table: "Manufacturers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManufacturerPhones",
                table: "ManufacturerPhones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManufacturerLocations",
                table: "ManufacturerLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ManufacturerEmails",
                table: "ManufacturerEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Locations",
                table: "Locations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Languages",
                table: "Languages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemsOrders",
                table: "ItemsOrders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Items",
                table: "Items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemExpiries",
                table: "ItemExpiries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemCodes",
                table: "ItemCodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemCategories",
                table: "ItemCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InvoiceTenders",
                table: "InvoiceTenders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Invoices",
                table: "Invoices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeePrivileges",
                table: "EmployeePrivileges");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeePrivilegeActions",
                table: "EmployeePrivilegeActions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeePhones",
                table: "EmployeePhones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeLocations",
                table: "EmployeeLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeEmails",
                table: "EmployeeEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Emails",
                table: "Emails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documents",
                table: "Documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Discounts",
                table: "Discounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Departments",
                table: "Departments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Customers",
                table: "Customers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerPrivileges",
                table: "CustomerPrivileges");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerPrivilegeActions",
                table: "CustomerPrivilegeActions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerPhones",
                table: "CustomerPhones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerLocations",
                table: "CustomerLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomerEmails",
                table: "CustomerEmails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countries",
                table: "Countries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cities",
                table: "Cities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChangeLogs",
                table: "ChangeLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Batches",
                table: "Batches");

            migrationBuilder.RenameTable(
                name: "UserTokens",
                newName: "user_token");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "user");

            migrationBuilder.RenameTable(
                name: "UserPrivileges",
                newName: "user_privilege");

            migrationBuilder.RenameTable(
                name: "UserPrivilegeActions",
                newName: "user_privilege_action");

            migrationBuilder.RenameTable(
                name: "UserPhones",
                newName: "user_phone");

            migrationBuilder.RenameTable(
                name: "UserPasswords",
                newName: "user_password");

            migrationBuilder.RenameTable(
                name: "UserEmails",
                newName: "user_email");

            migrationBuilder.RenameTable(
                name: "Units",
                newName: "unit");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                newName: "supplier");

            migrationBuilder.RenameTable(
                name: "SupplierPhones",
                newName: "supplier_phone");

            migrationBuilder.RenameTable(
                name: "SupplierLocations",
                newName: "supplier_location");

            migrationBuilder.RenameTable(
                name: "SupplierEmails",
                newName: "supplier_email");

            migrationBuilder.RenameTable(
                name: "Sales",
                newName: "sale");

            migrationBuilder.RenameTable(
                name: "Salaries",
                newName: "salary");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "role");

            migrationBuilder.RenameTable(
                name: "Regions",
                newName: "region");

            migrationBuilder.RenameTable(
                name: "Privileges",
                newName: "privilege");

            migrationBuilder.RenameTable(
                name: "Phones",
                newName: "phone");

            migrationBuilder.RenameTable(
                name: "Otps",
                newName: "otp");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "order_item");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "notification");

            migrationBuilder.RenameTable(
                name: "Manufacturers",
                newName: "manufacturer");

            migrationBuilder.RenameTable(
                name: "ManufacturerPhones",
                newName: "manufacturer_phone");

            migrationBuilder.RenameTable(
                name: "ManufacturerLocations",
                newName: "manufacturer_location");

            migrationBuilder.RenameTable(
                name: "ManufacturerEmails",
                newName: "manufacturer_email");

            migrationBuilder.RenameTable(
                name: "Locations",
                newName: "location");

            migrationBuilder.RenameTable(
                name: "Languages",
                newName: "language");

            migrationBuilder.RenameTable(
                name: "ItemsOrders",
                newName: "items_order");

            migrationBuilder.RenameTable(
                name: "Items",
                newName: "item");

            migrationBuilder.RenameTable(
                name: "ItemExpiries",
                newName: "item_expiry");

            migrationBuilder.RenameTable(
                name: "ItemCodes",
                newName: "item_code");

            migrationBuilder.RenameTable(
                name: "ItemCategories",
                newName: "item_category");

            migrationBuilder.RenameTable(
                name: "InvoiceTenders",
                newName: "invoice_tender");

            migrationBuilder.RenameTable(
                name: "Invoices",
                newName: "invoice");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "employee");

            migrationBuilder.RenameTable(
                name: "EmployeePrivileges",
                newName: "employee_privilege");

            migrationBuilder.RenameTable(
                name: "EmployeePrivilegeActions",
                newName: "employee_privilege_action");

            migrationBuilder.RenameTable(
                name: "EmployeePhones",
                newName: "employee_phone");

            migrationBuilder.RenameTable(
                name: "EmployeeLocations",
                newName: "employee_location");

            migrationBuilder.RenameTable(
                name: "EmployeeEmails",
                newName: "employee_email");

            migrationBuilder.RenameTable(
                name: "Emails",
                newName: "email");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "document");

            migrationBuilder.RenameTable(
                name: "Discounts",
                newName: "discount");

            migrationBuilder.RenameTable(
                name: "Departments",
                newName: "department");

            migrationBuilder.RenameTable(
                name: "Customers",
                newName: "customer");

            migrationBuilder.RenameTable(
                name: "CustomerPrivileges",
                newName: "customer_privilege");

            migrationBuilder.RenameTable(
                name: "CustomerPrivilegeActions",
                newName: "customer_privilege_action");

            migrationBuilder.RenameTable(
                name: "CustomerPhones",
                newName: "customer_phone");

            migrationBuilder.RenameTable(
                name: "CustomerLocations",
                newName: "customer_location");

            migrationBuilder.RenameTable(
                name: "CustomerEmails",
                newName: "customer_email");

            migrationBuilder.RenameTable(
                name: "Currencies",
                newName: "currency");

            migrationBuilder.RenameTable(
                name: "Countries",
                newName: "country");

            migrationBuilder.RenameTable(
                name: "Cities",
                newName: "city");

            migrationBuilder.RenameTable(
                name: "ChangeLogs",
                newName: "change_log");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "category");

            migrationBuilder.RenameTable(
                name: "Batches",
                newName: "batch");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "user_token",
                newName: "token");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_token",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenHash",
                table: "user_token",
                newName: "refresh_token_hash");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenExpiryDate",
                table: "user_token",
                newName: "refresh_token_expiry_date");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "user_token",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsRevoked",
                table: "user_token",
                newName: "is_revoked");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "user_token",
                newName: "expiry_date");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "user_token",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "UserTokenId",
                table: "user_token",
                newName: "user_token_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserTokens_UserId",
                table: "user_token",
                newName: "ix_user_token_user_id");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "user",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "user",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "user",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "user",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "user",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "user",
                newName: "employee_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "user",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Username",
                table: "user",
                newName: "ix_user_username");

            migrationBuilder.RenameIndex(
                name: "IX_Users_RoleId",
                table: "user",
                newName: "ix_user_role_id");

            migrationBuilder.RenameIndex(
                name: "IX_Users_EmployeeId",
                table: "user",
                newName: "ix_user_employee_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "user_privilege",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_privilege",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "PrivilegeId",
                table: "user_privilege",
                newName: "privilege_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "user_privilege",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "user_privilege",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "user_privilege",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "UserPrivilegeId",
                table: "user_privilege",
                newName: "user_privilege_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserPrivileges_UserId",
                table: "user_privilege",
                newName: "ix_user_privilege_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserPrivileges_PrivilegeId",
                table: "user_privilege",
                newName: "ix_user_privilege_privilege_id");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "user_privilege_action",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "user_privilege_action",
                newName: "action");

            migrationBuilder.RenameColumn(
                name: "UserPrivilegeId",
                table: "user_privilege_action",
                newName: "user_privilege_id");

            migrationBuilder.RenameColumn(
                name: "PerformedByUserId",
                table: "user_privilege_action",
                newName: "performed_by_user_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "user_privilege_action",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "user_privilege_action",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "UserPrivilegeActionId",
                table: "user_privilege_action",
                newName: "user_privilege_action_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserPrivilegeActions_UserPrivilegeId",
                table: "user_privilege_action",
                newName: "ix_user_privilege_action_user_privilege_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserPrivilegeActions_PerformedByUserId",
                table: "user_privilege_action",
                newName: "ix_user_privilege_action_performed_by_user_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_phone",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "PhoneId",
                table: "user_phone",
                newName: "phone_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "user_phone",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "user_phone",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "user_phone",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "UserPhoneId",
                table: "user_phone",
                newName: "user_phone_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserPhones_UserId",
                table: "user_phone",
                newName: "ix_user_phone_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserPhones_PhoneId",
                table: "user_phone",
                newName: "ix_user_phone_phone_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_password",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "user_password",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "user_password",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "user_password",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "UserPasswordId",
                table: "user_password",
                newName: "user_password_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserPasswords_UserId",
                table: "user_password",
                newName: "ix_user_password_user_id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "user_email",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "user_email",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "user_email",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "user_email",
                newName: "email_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "user_email",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "UserEmailId",
                table: "user_email",
                newName: "user_email_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserEmails_UserId",
                table: "user_email",
                newName: "ix_user_email_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_UserEmails_EmailId",
                table: "user_email",
                newName: "ix_user_email_email_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "unit",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "unit",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Abbreviation",
                table: "unit",
                newName: "abbreviation");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "unit",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "unit",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "UnitId",
                table: "unit",
                newName: "unit_id");

            migrationBuilder.RenameIndex(
                name: "IX_Units_Abbreviation",
                table: "unit",
                newName: "ix_unit_abbreviation");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "supplier",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "supplier",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "RegistrationNumber",
                table: "supplier",
                newName: "registration_number");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "supplier",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "supplier",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "supplier",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "supplier",
                newName: "supplier_id");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "supplier_phone",
                newName: "supplier_id");

            migrationBuilder.RenameColumn(
                name: "PhoneId",
                table: "supplier_phone",
                newName: "phone_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "supplier_phone",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "supplier_phone",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "supplier_phone",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "SupplierPhoneId",
                table: "supplier_phone",
                newName: "supplier_phone_id");

            migrationBuilder.RenameIndex(
                name: "IX_SupplierPhones_SupplierId",
                table: "supplier_phone",
                newName: "ix_supplier_phone_supplier_id");

            migrationBuilder.RenameIndex(
                name: "IX_SupplierPhones_PhoneId",
                table: "supplier_phone",
                newName: "ix_supplier_phone_phone_id");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "supplier_location",
                newName: "supplier_id");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "supplier_location",
                newName: "location_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "supplier_location",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "supplier_location",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "supplier_location",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "SupplierLocationId",
                table: "supplier_location",
                newName: "supplier_location_id");

            migrationBuilder.RenameIndex(
                name: "IX_SupplierLocations_SupplierId",
                table: "supplier_location",
                newName: "ix_supplier_location_supplier_id");

            migrationBuilder.RenameIndex(
                name: "IX_SupplierLocations_LocationId",
                table: "supplier_location",
                newName: "ix_supplier_location_location_id");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "supplier_email",
                newName: "supplier_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "supplier_email",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "supplier_email",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "supplier_email",
                newName: "email_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "supplier_email",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "SupplierEmailId",
                table: "supplier_email",
                newName: "supplier_email_id");

            migrationBuilder.RenameIndex(
                name: "IX_SupplierEmails_SupplierId",
                table: "supplier_email",
                newName: "ix_supplier_email_supplier_id");

            migrationBuilder.RenameIndex(
                name: "IX_SupplierEmails_EmailId",
                table: "supplier_email",
                newName: "ix_supplier_email_email_id");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "sale",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "sale",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "sale",
                newName: "unit_price");

            migrationBuilder.RenameColumn(
                name: "UnitAbbreviation",
                table: "sale",
                newName: "unit_abbreviation");

            migrationBuilder.RenameColumn(
                name: "LineTotal",
                table: "sale",
                newName: "line_total");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "sale",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ItemName",
                table: "sale",
                newName: "item_name");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "sale",
                newName: "item_id");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "sale",
                newName: "invoice_id");

            migrationBuilder.RenameColumn(
                name: "DiscountAmount",
                table: "sale",
                newName: "discount_amount");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "sale",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "SaleId",
                table: "sale",
                newName: "sale_id");

            migrationBuilder.RenameIndex(
                name: "IX_Sales_UserId",
                table: "sale",
                newName: "ix_sale_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Sales_ItemId",
                table: "sale",
                newName: "ix_sale_item_id");

            migrationBuilder.RenameIndex(
                name: "IX_Sales_InvoiceId",
                table: "sale",
                newName: "ix_sale_invoice_id");

            migrationBuilder.RenameColumn(
                name: "Grade",
                table: "salary",
                newName: "grade");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "salary",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "salary",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "salary",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "BasicAmount",
                table: "salary",
                newName: "basic_amount");

            migrationBuilder.RenameColumn(
                name: "AllowanceAmount",
                table: "salary",
                newName: "allowance_amount");

            migrationBuilder.RenameColumn(
                name: "SalaryId",
                table: "salary",
                newName: "salary_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "role",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "role",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "role",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "role",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "role",
                newName: "role_id");

            migrationBuilder.RenameIndex(
                name: "IX_Roles_Name",
                table: "role",
                newName: "ix_role_name");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "region",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "region",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "region",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CountryId",
                table: "region",
                newName: "country_id");

            migrationBuilder.RenameColumn(
                name: "RegionId",
                table: "region",
                newName: "region_id");

            migrationBuilder.RenameIndex(
                name: "IX_Regions_CountryId",
                table: "region",
                newName: "ix_region_country_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "privilege",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Module",
                table: "privilege",
                newName: "module");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "privilege",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "privilege",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "privilege",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "PrivilegeId",
                table: "privilege",
                newName: "privilege_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "phone",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "phone",
                newName: "number");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "phone",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsVerified",
                table: "phone",
                newName: "is_verified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "phone",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CountryId",
                table: "phone",
                newName: "country_id");

            migrationBuilder.RenameColumn(
                name: "PhoneId",
                table: "phone",
                newName: "phone_id");

            migrationBuilder.RenameIndex(
                name: "IX_Phones_CountryId_Number",
                table: "phone",
                newName: "ix_phone_country_id_number");

            migrationBuilder.RenameColumn(
                name: "Purpose",
                table: "otp",
                newName: "purpose");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "otp",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "otp",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "otp",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsUsed",
                table: "otp",
                newName: "is_used");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "otp",
                newName: "expires_at");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "otp",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "OtpId",
                table: "otp",
                newName: "otp_id");

            migrationBuilder.RenameIndex(
                name: "IX_Otps_UserId",
                table: "otp",
                newName: "ix_otp_user_id");

            migrationBuilder.RenameColumn(
                name: "UnitCost",
                table: "order_item",
                newName: "unit_cost");

            migrationBuilder.RenameColumn(
                name: "QuantityReceived",
                table: "order_item",
                newName: "quantity_received");

            migrationBuilder.RenameColumn(
                name: "QuantityOrdered",
                table: "order_item",
                newName: "quantity_ordered");

            migrationBuilder.RenameColumn(
                name: "LineTotal",
                table: "order_item",
                newName: "line_total");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "order_item",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ItemsOrderId",
                table: "order_item",
                newName: "items_order_id");

            migrationBuilder.RenameColumn(
                name: "ItemName",
                table: "order_item",
                newName: "item_name");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "order_item",
                newName: "item_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "order_item",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "OrderItemId",
                table: "order_item",
                newName: "order_item_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_ItemsOrderId",
                table: "order_item",
                newName: "ix_order_item_items_order_id");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_ItemId",
                table: "order_item",
                newName: "ix_order_item_item_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "notification",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "notification",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "notification",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "notification",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "notification",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "notification",
                newName: "is_read");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "notification",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "NotificationId",
                table: "notification",
                newName: "notification_id");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "notification",
                newName: "ix_notification_user_id");

            migrationBuilder.RenameColumn(
                name: "Website",
                table: "manufacturer",
                newName: "website");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "manufacturer",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "manufacturer",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "RegistrationNumber",
                table: "manufacturer",
                newName: "registration_number");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "manufacturer",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "manufacturer",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "manufacturer",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "manufacturer",
                newName: "manufacturer_id");

            migrationBuilder.RenameColumn(
                name: "PhoneId",
                table: "manufacturer_phone",
                newName: "phone_id");

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "manufacturer_phone",
                newName: "manufacturer_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "manufacturer_phone",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "manufacturer_phone",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "manufacturer_phone",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "ManufacturerPhoneId",
                table: "manufacturer_phone",
                newName: "manufacturer_phone_id");

            migrationBuilder.RenameIndex(
                name: "IX_ManufacturerPhones_PhoneId",
                table: "manufacturer_phone",
                newName: "ix_manufacturer_phone_phone_id");

            migrationBuilder.RenameIndex(
                name: "IX_ManufacturerPhones_ManufacturerId",
                table: "manufacturer_phone",
                newName: "ix_manufacturer_phone_manufacturer_id");

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "manufacturer_location",
                newName: "manufacturer_id");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "manufacturer_location",
                newName: "location_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "manufacturer_location",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "manufacturer_location",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "manufacturer_location",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "ManufacturerLocationId",
                table: "manufacturer_location",
                newName: "manufacturer_location_id");

            migrationBuilder.RenameIndex(
                name: "IX_ManufacturerLocations_ManufacturerId",
                table: "manufacturer_location",
                newName: "ix_manufacturer_location_manufacturer_id");

            migrationBuilder.RenameIndex(
                name: "IX_ManufacturerLocations_LocationId",
                table: "manufacturer_location",
                newName: "ix_manufacturer_location_location_id");

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "manufacturer_email",
                newName: "manufacturer_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "manufacturer_email",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "manufacturer_email",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "manufacturer_email",
                newName: "email_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "manufacturer_email",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "ManufacturerEmailId",
                table: "manufacturer_email",
                newName: "manufacturer_email_id");

            migrationBuilder.RenameIndex(
                name: "IX_ManufacturerEmails_ManufacturerId",
                table: "manufacturer_email",
                newName: "ix_manufacturer_email_manufacturer_id");

            migrationBuilder.RenameIndex(
                name: "IX_ManufacturerEmails_EmailId",
                table: "manufacturer_email",
                newName: "ix_manufacturer_email_email_id");

            migrationBuilder.RenameColumn(
                name: "Longitude",
                table: "location",
                newName: "longitude");

            migrationBuilder.RenameColumn(
                name: "Latitude",
                table: "location",
                newName: "latitude");

            migrationBuilder.RenameColumn(
                name: "StreetAddress",
                table: "location",
                newName: "street_address");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "location",
                newName: "postal_code");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "location",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "location",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CityId",
                table: "location",
                newName: "city_id");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "location",
                newName: "location_id");

            migrationBuilder.RenameIndex(
                name: "IX_Locations_CityId",
                table: "location",
                newName: "ix_location_city_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "language",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "language",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "language",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "language",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "LanguageId",
                table: "language",
                newName: "language_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "items_order",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "items_order",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "items_order",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "items_order",
                newName: "supplier_id");

            migrationBuilder.RenameColumn(
                name: "OrderNumber",
                table: "items_order",
                newName: "order_number");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "items_order",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ExpectedDeliveryDate",
                table: "items_order",
                newName: "expected_delivery_date");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "items_order",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "items_order",
                newName: "created_by_user_id");

            migrationBuilder.RenameColumn(
                name: "ItemsOrderId",
                table: "items_order",
                newName: "items_order_id");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsOrders_SupplierId",
                table: "items_order",
                newName: "ix_items_order_supplier_id");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsOrders_OrderNumber",
                table: "items_order",
                newName: "ix_items_order_order_number");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsOrders_CreatedByUserId",
                table: "items_order",
                newName: "ix_items_order_created_by_user_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "item",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "item",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "item",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Barcode",
                table: "item",
                newName: "barcode");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "item",
                newName: "unit_price");

            migrationBuilder.RenameColumn(
                name: "UnitId",
                table: "item",
                newName: "unit_id");

            migrationBuilder.RenameColumn(
                name: "ReorderLevel",
                table: "item",
                newName: "reorder_level");

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "item",
                newName: "manufacturer_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "item",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "item",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "InStock",
                table: "item",
                newName: "in_stock");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "item",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "item",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CostPrice",
                table: "item",
                newName: "cost_price");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "item",
                newName: "category_id");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "item",
                newName: "item_id");

            migrationBuilder.RenameIndex(
                name: "IX_Items_UnitId",
                table: "item",
                newName: "ix_item_unit_id");

            migrationBuilder.RenameIndex(
                name: "IX_Items_ManufacturerId",
                table: "item",
                newName: "ix_item_manufacturer_id");

            migrationBuilder.RenameIndex(
                name: "IX_Items_CategoryId",
                table: "item",
                newName: "ix_item_category_id");

            migrationBuilder.RenameIndex(
                name: "IX_Items_Barcode",
                table: "item",
                newName: "ix_item_barcode");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "item_expiry",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "item_expiry",
                newName: "item_id");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "item_expiry",
                newName: "expiry_date");

            migrationBuilder.RenameColumn(
                name: "DaysWarningBefore",
                table: "item_expiry",
                newName: "days_warning_before");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "item_expiry",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "ItemExpiryId",
                table: "item_expiry",
                newName: "item_expiry_id");

            migrationBuilder.RenameIndex(
                name: "IX_ItemExpiries_ItemId",
                table: "item_expiry",
                newName: "ix_item_expiry_item_id");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "item_code",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "item_code",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "item_code",
                newName: "item_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "item_code",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CodeType",
                table: "item_code",
                newName: "code_type");

            migrationBuilder.RenameColumn(
                name: "ItemCodeId",
                table: "item_code",
                newName: "item_code_id");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCodes_ItemId",
                table: "item_code",
                newName: "ix_item_code_item_id");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCodes_Code",
                table: "item_code",
                newName: "ix_item_code_code");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "item_category",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "item_category",
                newName: "item_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "item_category",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "item_category",
                newName: "category_id");

            migrationBuilder.RenameColumn(
                name: "ItemCategoryId",
                table: "item_category",
                newName: "item_category_id");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCategories_ItemId",
                table: "item_category",
                newName: "ix_item_category_item_id");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCategories_CategoryId",
                table: "item_category",
                newName: "ix_item_category_category_id");

            migrationBuilder.RenameColumn(
                name: "Reference",
                table: "invoice_tender",
                newName: "reference");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "invoice_tender",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "PaymentType",
                table: "invoice_tender",
                newName: "payment_type");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "invoice_tender",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "invoice_tender",
                newName: "invoice_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "invoice_tender",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "InvoiceTenderId",
                table: "invoice_tender",
                newName: "invoice_tender_id");

            migrationBuilder.RenameIndex(
                name: "IX_InvoiceTenders_InvoiceId",
                table: "invoice_tender",
                newName: "ix_invoice_tender_invoice_id");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "invoice",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "invoice",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "invoice",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "PaymentType",
                table: "invoice",
                newName: "payment_type");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "invoice",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPaid",
                table: "invoice",
                newName: "is_paid");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "invoice",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "invoice",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "ChangeGiven",
                table: "invoice",
                newName: "change_given");

            migrationBuilder.RenameColumn(
                name: "AmountTendered",
                table: "invoice",
                newName: "amount_tendered");

            migrationBuilder.RenameColumn(
                name: "InvoiceId",
                table: "invoice",
                newName: "invoice_id");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_UserId",
                table: "invoice",
                newName: "ix_invoice_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_CustomerId",
                table: "invoice",
                newName: "ix_invoice_customer_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "employee",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "employee",
                newName: "gender");

            migrationBuilder.RenameColumn(
                name: "SalaryId",
                table: "employee",
                newName: "salary_id");

            migrationBuilder.RenameColumn(
                name: "PlaceOfBirth",
                table: "employee",
                newName: "place_of_birth");

            migrationBuilder.RenameColumn(
                name: "NidNumber",
                table: "employee",
                newName: "nid_number");

            migrationBuilder.RenameColumn(
                name: "MiddleName",
                table: "employee",
                newName: "middle_name");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "employee",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "employee",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "employee",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "employee",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "employee",
                newName: "department_id");

            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "employee",
                newName: "date_of_birth");

            migrationBuilder.RenameColumn(
                name: "DateEmployed",
                table: "employee",
                newName: "date_employed");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "employee",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "employee",
                newName: "employee_id");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_SalaryId",
                table: "employee",
                newName: "ix_employee_salary_id");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_DepartmentId",
                table: "employee",
                newName: "ix_employee_department_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "employee_privilege",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "PrivilegeId",
                table: "employee_privilege",
                newName: "privilege_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "employee_privilege",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "employee_privilege",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "employee_privilege",
                newName: "employee_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "employee_privilege",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "EmployeePrivilegeId",
                table: "employee_privilege",
                newName: "employee_privilege_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeePrivileges_PrivilegeId",
                table: "employee_privilege",
                newName: "ix_employee_privilege_privilege_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeePrivileges_EmployeeId",
                table: "employee_privilege",
                newName: "ix_employee_privilege_employee_id");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "employee_privilege_action",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "employee_privilege_action",
                newName: "action");

            migrationBuilder.RenameColumn(
                name: "PerformedByUserId",
                table: "employee_privilege_action",
                newName: "performed_by_user_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "employee_privilege_action",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "EmployeePrivilegeId",
                table: "employee_privilege_action",
                newName: "employee_privilege_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "employee_privilege_action",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "EmployeePrivilegeActionId",
                table: "employee_privilege_action",
                newName: "employee_privilege_action_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeePrivilegeActions_PerformedByUserId",
                table: "employee_privilege_action",
                newName: "ix_employee_privilege_action_performed_by_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeePrivilegeActions_EmployeePrivilegeId",
                table: "employee_privilege_action",
                newName: "ix_employee_privilege_action_employee_privilege_id");

            migrationBuilder.RenameColumn(
                name: "PhoneId",
                table: "employee_phone",
                newName: "phone_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "employee_phone",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "employee_phone",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "employee_phone",
                newName: "employee_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "employee_phone",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "EmployeePhoneId",
                table: "employee_phone",
                newName: "employee_phone_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeePhones_PhoneId",
                table: "employee_phone",
                newName: "ix_employee_phone_phone_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeePhones_EmployeeId",
                table: "employee_phone",
                newName: "ix_employee_phone_employee_id");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "employee_location",
                newName: "location_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "employee_location",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "employee_location",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "employee_location",
                newName: "employee_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "employee_location",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "EmployeeLocationId",
                table: "employee_location",
                newName: "employee_location_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeLocations_LocationId",
                table: "employee_location",
                newName: "ix_employee_location_location_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeLocations_EmployeeId",
                table: "employee_location",
                newName: "ix_employee_location_employee_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "employee_email",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "employee_email",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "employee_email",
                newName: "employee_id");

            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "employee_email",
                newName: "email_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "employee_email",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "EmployeeEmailId",
                table: "employee_email",
                newName: "employee_email_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeEmails_EmployeeId",
                table: "employee_email",
                newName: "ix_employee_email_employee_id");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeEmails_EmailId",
                table: "employee_email",
                newName: "ix_employee_email_email_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "email",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "email",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "email",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsVerified",
                table: "email",
                newName: "is_verified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "email",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "email",
                newName: "email_id");

            migrationBuilder.RenameIndex(
                name: "IX_Emails_Address",
                table: "email",
                newName: "ix_email_address");

            migrationBuilder.RenameColumn(
                name: "UploadedByUserId",
                table: "document",
                newName: "uploaded_by_user_id");

            migrationBuilder.RenameColumn(
                name: "MimeType",
                table: "document",
                newName: "mime_type");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "document",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "FileSizeBytes",
                table: "document",
                newName: "file_size_bytes");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "document",
                newName: "file_path");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "document",
                newName: "file_name");

            migrationBuilder.RenameColumn(
                name: "EntityName",
                table: "document",
                newName: "entity_name");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "document",
                newName: "entity_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "document",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "document",
                newName: "document_id");

            migrationBuilder.RenameColumn(
                name: "Percentage",
                table: "discount",
                newName: "percentage");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "discount",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "ValidTo",
                table: "discount",
                newName: "valid_to");

            migrationBuilder.RenameColumn(
                name: "ValidFrom",
                table: "discount",
                newName: "valid_from");

            migrationBuilder.RenameColumn(
                name: "ManagedByUserId",
                table: "discount",
                newName: "managed_by_user_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "discount",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "discount",
                newName: "item_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "discount",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "discount",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "DiscountId",
                table: "discount",
                newName: "discount_id");

            migrationBuilder.RenameIndex(
                name: "IX_Discounts_ManagedByUserId",
                table: "discount",
                newName: "ix_discount_managed_by_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Discounts_ItemId",
                table: "discount",
                newName: "ix_discount_item_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "department",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "department",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "department",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "department",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "department",
                newName: "department_id");

            migrationBuilder.RenameIndex(
                name: "IX_Departments_Name",
                table: "department",
                newName: "ix_department_name");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "customer",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "customer",
                newName: "gender");

            migrationBuilder.RenameColumn(
                name: "MiddleName",
                table: "customer",
                newName: "middle_name");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "customer",
                newName: "last_name");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "customer",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "customer",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "customer",
                newName: "first_name");

            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "customer",
                newName: "date_of_birth");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "customer",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "customer",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "customer_privilege",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "PrivilegeId",
                table: "customer_privilege",
                newName: "privilege_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "customer_privilege",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "customer_privilege",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "customer_privilege",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "customer_privilege",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "CustomerPrivilegeId",
                table: "customer_privilege",
                newName: "customer_privilege_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerPrivileges_PrivilegeId",
                table: "customer_privilege",
                newName: "ix_customer_privilege_privilege_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerPrivileges_CustomerId",
                table: "customer_privilege",
                newName: "ix_customer_privilege_customer_id");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "customer_privilege_action",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "customer_privilege_action",
                newName: "action");

            migrationBuilder.RenameColumn(
                name: "PerformedByUserId",
                table: "customer_privilege_action",
                newName: "performed_by_user_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "customer_privilege_action",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "customer_privilege_action",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CustomerPrivilegeId",
                table: "customer_privilege_action",
                newName: "customer_privilege_id");

            migrationBuilder.RenameColumn(
                name: "CustomerPrivilegeActionId",
                table: "customer_privilege_action",
                newName: "customer_privilege_action_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerPrivilegeActions_PerformedByUserId",
                table: "customer_privilege_action",
                newName: "ix_customer_privilege_action_performed_by_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerPrivilegeActions_CustomerPrivilegeId",
                table: "customer_privilege_action",
                newName: "ix_customer_privilege_action_customer_privilege_id");

            migrationBuilder.RenameColumn(
                name: "PhoneId",
                table: "customer_phone",
                newName: "phone_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "customer_phone",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "customer_phone",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "customer_phone",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "customer_phone",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "CustomerPhoneId",
                table: "customer_phone",
                newName: "customer_phone_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerPhones_PhoneId",
                table: "customer_phone",
                newName: "ix_customer_phone_phone_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerPhones_CustomerId",
                table: "customer_phone",
                newName: "ix_customer_phone_customer_id");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "customer_location",
                newName: "location_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "customer_location",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "customer_location",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "customer_location",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "customer_location",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "CustomerLocationId",
                table: "customer_location",
                newName: "customer_location_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerLocations_LocationId",
                table: "customer_location",
                newName: "ix_customer_location_location_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerLocations_CustomerId",
                table: "customer_location",
                newName: "ix_customer_location_customer_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "customer_email",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsPrimary",
                table: "customer_email",
                newName: "is_primary");

            migrationBuilder.RenameColumn(
                name: "EmailId",
                table: "customer_email",
                newName: "email_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "customer_email",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "customer_email",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "CustomerEmailId",
                table: "customer_email",
                newName: "customer_email_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerEmails_EmailId",
                table: "customer_email",
                newName: "ix_customer_email_email_id");

            migrationBuilder.RenameIndex(
                name: "IX_CustomerEmails_CustomerId",
                table: "customer_email",
                newName: "ix_customer_email_customer_id");

            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "currency",
                newName: "symbol");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "currency",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "currency",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "currency",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "currency",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CurrencyId",
                table: "currency",
                newName: "currency_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "country",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "PhoneCode",
                table: "country",
                newName: "phone_code");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "country",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsoCode",
                table: "country",
                newName: "iso_code");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "country",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CountryId",
                table: "country",
                newName: "country_id");

            migrationBuilder.RenameIndex(
                name: "IX_Countries_IsoCode",
                table: "country",
                newName: "ix_country_iso_code");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "city",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "RegionId",
                table: "city",
                newName: "region_id");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "city",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "city",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CityId",
                table: "city",
                newName: "city_id");

            migrationBuilder.RenameIndex(
                name: "IX_Cities_RegionId",
                table: "city",
                newName: "ix_city_region_id");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "change_log",
                newName: "action");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "change_log",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "OldValues",
                table: "change_log",
                newName: "old_values");

            migrationBuilder.RenameColumn(
                name: "NewValues",
                table: "change_log",
                newName: "new_values");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "change_log",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "change_log",
                newName: "ip_address");

            migrationBuilder.RenameColumn(
                name: "EntityName",
                table: "change_log",
                newName: "entity_name");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "change_log",
                newName: "entity_id");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "change_log",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "ChangeLogId",
                table: "change_log",
                newName: "change_log_id");

            migrationBuilder.RenameIndex(
                name: "IX_ChangeLogs_UserId",
                table: "change_log",
                newName: "ix_change_log_user_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "category",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "category",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "category",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "category",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "category",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "category",
                newName: "category_id");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_Name",
                table: "category",
                newName: "ix_category_name");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "batch",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "batch",
                newName: "notes");

            migrationBuilder.RenameColumn(
                name: "ReceivedDate",
                table: "batch",
                newName: "received_date");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "batch",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "batch",
                newName: "item_id");

            migrationBuilder.RenameColumn(
                name: "ExpiryDate",
                table: "batch",
                newName: "expiry_date");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "batch",
                newName: "date_created");

            migrationBuilder.RenameColumn(
                name: "CostPrice",
                table: "batch",
                newName: "cost_price");

            migrationBuilder.RenameColumn(
                name: "BatchNumber",
                table: "batch",
                newName: "batch_number");

            migrationBuilder.RenameColumn(
                name: "BatchId",
                table: "batch",
                newName: "batch_id");

            migrationBuilder.RenameIndex(
                name: "IX_Batches_ItemId",
                table: "batch",
                newName: "ix_batch_item_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_token",
                table: "user_token",
                column: "user_token_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user",
                table: "user",
                column: "user_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_privilege",
                table: "user_privilege",
                column: "user_privilege_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_privilege_action",
                table: "user_privilege_action",
                column: "user_privilege_action_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_phone",
                table: "user_phone",
                column: "user_phone_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_password",
                table: "user_password",
                column: "user_password_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_email",
                table: "user_email",
                column: "user_email_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_unit",
                table: "unit",
                column: "unit_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_supplier",
                table: "supplier",
                column: "supplier_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_supplier_phone",
                table: "supplier_phone",
                column: "supplier_phone_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_supplier_location",
                table: "supplier_location",
                column: "supplier_location_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_supplier_email",
                table: "supplier_email",
                column: "supplier_email_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_sale",
                table: "sale",
                column: "sale_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_salary",
                table: "salary",
                column: "salary_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_role",
                table: "role",
                column: "role_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_region",
                table: "region",
                column: "region_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_privilege",
                table: "privilege",
                column: "privilege_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_phone",
                table: "phone",
                column: "phone_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_otp",
                table: "otp",
                column: "otp_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_order_item",
                table: "order_item",
                column: "order_item_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_notification",
                table: "notification",
                column: "notification_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_manufacturer",
                table: "manufacturer",
                column: "manufacturer_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_manufacturer_phone",
                table: "manufacturer_phone",
                column: "manufacturer_phone_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_manufacturer_location",
                table: "manufacturer_location",
                column: "manufacturer_location_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_manufacturer_email",
                table: "manufacturer_email",
                column: "manufacturer_email_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_location",
                table: "location",
                column: "location_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_language",
                table: "language",
                column: "language_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_items_order",
                table: "items_order",
                column: "items_order_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_item",
                table: "item",
                column: "item_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_item_expiry",
                table: "item_expiry",
                column: "item_expiry_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_item_code",
                table: "item_code",
                column: "item_code_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_item_category",
                table: "item_category",
                column: "item_category_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_invoice_tender",
                table: "invoice_tender",
                column: "invoice_tender_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_invoice",
                table: "invoice",
                column: "invoice_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_employee",
                table: "employee",
                column: "employee_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_employee_privilege",
                table: "employee_privilege",
                column: "employee_privilege_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_employee_privilege_action",
                table: "employee_privilege_action",
                column: "employee_privilege_action_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_employee_phone",
                table: "employee_phone",
                column: "employee_phone_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_employee_location",
                table: "employee_location",
                column: "employee_location_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_employee_email",
                table: "employee_email",
                column: "employee_email_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_email",
                table: "email",
                column: "email_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_document",
                table: "document",
                column: "document_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_discount",
                table: "discount",
                column: "discount_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_department",
                table: "department",
                column: "department_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer",
                table: "customer",
                column: "customer_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer_privilege",
                table: "customer_privilege",
                column: "customer_privilege_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer_privilege_action",
                table: "customer_privilege_action",
                column: "customer_privilege_action_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer_phone",
                table: "customer_phone",
                column: "customer_phone_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer_location",
                table: "customer_location",
                column: "customer_location_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_customer_email",
                table: "customer_email",
                column: "customer_email_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_currency",
                table: "currency",
                column: "currency_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_country",
                table: "country",
                column: "country_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_city",
                table: "city",
                column: "city_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_change_log",
                table: "change_log",
                column: "change_log_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_category",
                table: "category",
                column: "category_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_batch",
                table: "batch",
                column: "batch_id");

            migrationBuilder.AddForeignKey(
                name: "fk_batch_items_item_id",
                table: "batch",
                column: "item_id",
                principalTable: "item",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_change_log_users_user_id",
                table: "change_log",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_city_regions_region_id",
                table: "city",
                column: "region_id",
                principalTable: "region",
                principalColumn: "region_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_email_customers_customer_id",
                table: "customer_email",
                column: "customer_id",
                principalTable: "customer",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_email_emails_email_id",
                table: "customer_email",
                column: "email_id",
                principalTable: "email",
                principalColumn: "email_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_location_customer_customer_id",
                table: "customer_location",
                column: "customer_id",
                principalTable: "customer",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_location_locations_location_id",
                table: "customer_location",
                column: "location_id",
                principalTable: "location",
                principalColumn: "location_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_phone_customers_customer_id",
                table: "customer_phone",
                column: "customer_id",
                principalTable: "customer",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_phone_phones_phone_id",
                table: "customer_phone",
                column: "phone_id",
                principalTable: "phone",
                principalColumn: "phone_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_privilege_customer_customer_id",
                table: "customer_privilege",
                column: "customer_id",
                principalTable: "customer",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_privilege_privileges_privilege_id",
                table: "customer_privilege",
                column: "privilege_id",
                principalTable: "privilege",
                principalColumn: "privilege_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_privilege_action_customer_privilege_customer_privil~",
                table: "customer_privilege_action",
                column: "customer_privilege_id",
                principalTable: "customer_privilege",
                principalColumn: "customer_privilege_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_customer_privilege_action_users_performed_by_user_id",
                table: "customer_privilege_action",
                column: "performed_by_user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_discount_items_item_id",
                table: "discount",
                column: "item_id",
                principalTable: "item",
                principalColumn: "item_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_discount_users_managed_by_user_id",
                table: "discount",
                column: "managed_by_user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_department_department_id",
                table: "employee",
                column: "department_id",
                principalTable: "department",
                principalColumn: "department_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_salaries_salary_id",
                table: "employee",
                column: "salary_id",
                principalTable: "salary",
                principalColumn: "salary_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_email_email_email_id",
                table: "employee_email",
                column: "email_id",
                principalTable: "email",
                principalColumn: "email_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_email_employees_employee_id",
                table: "employee_email",
                column: "employee_id",
                principalTable: "employee",
                principalColumn: "employee_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_location_employee_employee_id",
                table: "employee_location",
                column: "employee_id",
                principalTable: "employee",
                principalColumn: "employee_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_location_locations_location_id",
                table: "employee_location",
                column: "location_id",
                principalTable: "location",
                principalColumn: "location_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_phone_employees_employee_id",
                table: "employee_phone",
                column: "employee_id",
                principalTable: "employee",
                principalColumn: "employee_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_phone_phones_phone_id",
                table: "employee_phone",
                column: "phone_id",
                principalTable: "phone",
                principalColumn: "phone_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_privilege_employee_employee_id",
                table: "employee_privilege",
                column: "employee_id",
                principalTable: "employee",
                principalColumn: "employee_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_privilege_privileges_privilege_id",
                table: "employee_privilege",
                column: "privilege_id",
                principalTable: "privilege",
                principalColumn: "privilege_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_privilege_action_employee_privilege_employee_privil~",
                table: "employee_privilege_action",
                column: "employee_privilege_id",
                principalTable: "employee_privilege",
                principalColumn: "employee_privilege_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_employee_privilege_action_users_performed_by_user_id",
                table: "employee_privilege_action",
                column: "performed_by_user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_invoice_customer_customer_id",
                table: "invoice",
                column: "customer_id",
                principalTable: "customer",
                principalColumn: "customer_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_invoice_users_user_id",
                table: "invoice",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_invoice_tender_invoice_invoice_id",
                table: "invoice_tender",
                column: "invoice_id",
                principalTable: "invoice",
                principalColumn: "invoice_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_item_category_category_id",
                table: "item",
                column: "category_id",
                principalTable: "category",
                principalColumn: "category_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_item_manufacturers_manufacturer_id",
                table: "item",
                column: "manufacturer_id",
                principalTable: "manufacturer",
                principalColumn: "manufacturer_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_item_units_unit_id",
                table: "item",
                column: "unit_id",
                principalTable: "unit",
                principalColumn: "unit_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_item_category_category_category_id",
                table: "item_category",
                column: "category_id",
                principalTable: "category",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_item_category_item_item_id",
                table: "item_category",
                column: "item_id",
                principalTable: "item",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_item_code_item_item_id",
                table: "item_code",
                column: "item_id",
                principalTable: "item",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_item_expiry_item_item_id",
                table: "item_expiry",
                column: "item_id",
                principalTable: "item",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_items_order_suppliers_supplier_id",
                table: "items_order",
                column: "supplier_id",
                principalTable: "supplier",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_items_order_users_created_by_user_id",
                table: "items_order",
                column: "created_by_user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_location_city_city_id",
                table: "location",
                column: "city_id",
                principalTable: "city",
                principalColumn: "city_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_manufacturer_email_email_email_id",
                table: "manufacturer_email",
                column: "email_id",
                principalTable: "email",
                principalColumn: "email_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_manufacturer_email_manufacturers_manufacturer_id",
                table: "manufacturer_email",
                column: "manufacturer_id",
                principalTable: "manufacturer",
                principalColumn: "manufacturer_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_manufacturer_location_location_location_id",
                table: "manufacturer_location",
                column: "location_id",
                principalTable: "location",
                principalColumn: "location_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_manufacturer_location_manufacturer_manufacturer_id",
                table: "manufacturer_location",
                column: "manufacturer_id",
                principalTable: "manufacturer",
                principalColumn: "manufacturer_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_manufacturer_phone_manufacturers_manufacturer_id",
                table: "manufacturer_phone",
                column: "manufacturer_id",
                principalTable: "manufacturer",
                principalColumn: "manufacturer_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_manufacturer_phone_phones_phone_id",
                table: "manufacturer_phone",
                column: "phone_id",
                principalTable: "phone",
                principalColumn: "phone_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_notification_users_user_id",
                table: "notification",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_item_item_item_id",
                table: "order_item",
                column: "item_id",
                principalTable: "item",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_order_item_items_order_items_order_id",
                table: "order_item",
                column: "items_order_id",
                principalTable: "items_order",
                principalColumn: "items_order_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_otp_users_user_id",
                table: "otp",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_region_country_country_id",
                table: "region",
                column: "country_id",
                principalTable: "country",
                principalColumn: "country_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_sale_invoice_invoice_id",
                table: "sale",
                column: "invoice_id",
                principalTable: "invoice",
                principalColumn: "invoice_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sale_item_item_id",
                table: "sale",
                column: "item_id",
                principalTable: "item",
                principalColumn: "item_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_sale_users_user_id",
                table: "sale",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_supplier_email_email_email_id",
                table: "supplier_email",
                column: "email_id",
                principalTable: "email",
                principalColumn: "email_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_supplier_email_suppliers_supplier_id",
                table: "supplier_email",
                column: "supplier_id",
                principalTable: "supplier",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_supplier_location_location_location_id",
                table: "supplier_location",
                column: "location_id",
                principalTable: "location",
                principalColumn: "location_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_supplier_location_supplier_supplier_id",
                table: "supplier_location",
                column: "supplier_id",
                principalTable: "supplier",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_supplier_phone_phone_phone_id",
                table: "supplier_phone",
                column: "phone_id",
                principalTable: "phone",
                principalColumn: "phone_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_supplier_phone_suppliers_supplier_id",
                table: "supplier_phone",
                column: "supplier_id",
                principalTable: "supplier",
                principalColumn: "supplier_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_employee_employee_id",
                table: "user",
                column: "employee_id",
                principalTable: "employee",
                principalColumn: "employee_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_user_role_role_id",
                table: "user",
                column: "role_id",
                principalTable: "role",
                principalColumn: "role_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_user_email_email_email_id",
                table: "user_email",
                column: "email_id",
                principalTable: "email",
                principalColumn: "email_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_email_users_user_id",
                table: "user_email",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_password_user_user_id",
                table: "user_password",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_phone_phone_phone_id",
                table: "user_phone",
                column: "phone_id",
                principalTable: "phone",
                principalColumn: "phone_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_phone_users_user_id",
                table: "user_phone",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_privilege_privilege_privilege_id",
                table: "user_privilege",
                column: "privilege_id",
                principalTable: "privilege",
                principalColumn: "privilege_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_privilege_user_user_id",
                table: "user_privilege",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_privilege_action_user_performed_by_user_id",
                table: "user_privilege_action",
                column: "performed_by_user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_privilege_action_user_privilege_user_privilege_id",
                table: "user_privilege_action",
                column: "user_privilege_id",
                principalTable: "user_privilege",
                principalColumn: "user_privilege_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_user_token_user_user_id",
                table: "user_token",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_batch_items_item_id",
                table: "batch");

            migrationBuilder.DropForeignKey(
                name: "fk_change_log_users_user_id",
                table: "change_log");

            migrationBuilder.DropForeignKey(
                name: "fk_city_regions_region_id",
                table: "city");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_email_customers_customer_id",
                table: "customer_email");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_email_emails_email_id",
                table: "customer_email");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_location_customer_customer_id",
                table: "customer_location");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_location_locations_location_id",
                table: "customer_location");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_phone_customers_customer_id",
                table: "customer_phone");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_phone_phones_phone_id",
                table: "customer_phone");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_privilege_customer_customer_id",
                table: "customer_privilege");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_privilege_privileges_privilege_id",
                table: "customer_privilege");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_privilege_action_customer_privilege_customer_privil~",
                table: "customer_privilege_action");

            migrationBuilder.DropForeignKey(
                name: "fk_customer_privilege_action_users_performed_by_user_id",
                table: "customer_privilege_action");

            migrationBuilder.DropForeignKey(
                name: "fk_discount_items_item_id",
                table: "discount");

            migrationBuilder.DropForeignKey(
                name: "fk_discount_users_managed_by_user_id",
                table: "discount");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_department_department_id",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_salaries_salary_id",
                table: "employee");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_email_email_email_id",
                table: "employee_email");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_email_employees_employee_id",
                table: "employee_email");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_location_employee_employee_id",
                table: "employee_location");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_location_locations_location_id",
                table: "employee_location");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_phone_employees_employee_id",
                table: "employee_phone");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_phone_phones_phone_id",
                table: "employee_phone");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_privilege_employee_employee_id",
                table: "employee_privilege");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_privilege_privileges_privilege_id",
                table: "employee_privilege");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_privilege_action_employee_privilege_employee_privil~",
                table: "employee_privilege_action");

            migrationBuilder.DropForeignKey(
                name: "fk_employee_privilege_action_users_performed_by_user_id",
                table: "employee_privilege_action");

            migrationBuilder.DropForeignKey(
                name: "fk_invoice_customer_customer_id",
                table: "invoice");

            migrationBuilder.DropForeignKey(
                name: "fk_invoice_users_user_id",
                table: "invoice");

            migrationBuilder.DropForeignKey(
                name: "fk_invoice_tender_invoice_invoice_id",
                table: "invoice_tender");

            migrationBuilder.DropForeignKey(
                name: "fk_item_category_category_id",
                table: "item");

            migrationBuilder.DropForeignKey(
                name: "fk_item_manufacturers_manufacturer_id",
                table: "item");

            migrationBuilder.DropForeignKey(
                name: "fk_item_units_unit_id",
                table: "item");

            migrationBuilder.DropForeignKey(
                name: "fk_item_category_category_category_id",
                table: "item_category");

            migrationBuilder.DropForeignKey(
                name: "fk_item_category_item_item_id",
                table: "item_category");

            migrationBuilder.DropForeignKey(
                name: "fk_item_code_item_item_id",
                table: "item_code");

            migrationBuilder.DropForeignKey(
                name: "fk_item_expiry_item_item_id",
                table: "item_expiry");

            migrationBuilder.DropForeignKey(
                name: "fk_items_order_suppliers_supplier_id",
                table: "items_order");

            migrationBuilder.DropForeignKey(
                name: "fk_items_order_users_created_by_user_id",
                table: "items_order");

            migrationBuilder.DropForeignKey(
                name: "fk_location_city_city_id",
                table: "location");

            migrationBuilder.DropForeignKey(
                name: "fk_manufacturer_email_email_email_id",
                table: "manufacturer_email");

            migrationBuilder.DropForeignKey(
                name: "fk_manufacturer_email_manufacturers_manufacturer_id",
                table: "manufacturer_email");

            migrationBuilder.DropForeignKey(
                name: "fk_manufacturer_location_location_location_id",
                table: "manufacturer_location");

            migrationBuilder.DropForeignKey(
                name: "fk_manufacturer_location_manufacturer_manufacturer_id",
                table: "manufacturer_location");

            migrationBuilder.DropForeignKey(
                name: "fk_manufacturer_phone_manufacturers_manufacturer_id",
                table: "manufacturer_phone");

            migrationBuilder.DropForeignKey(
                name: "fk_manufacturer_phone_phones_phone_id",
                table: "manufacturer_phone");

            migrationBuilder.DropForeignKey(
                name: "fk_notification_users_user_id",
                table: "notification");

            migrationBuilder.DropForeignKey(
                name: "fk_order_item_item_item_id",
                table: "order_item");

            migrationBuilder.DropForeignKey(
                name: "fk_order_item_items_order_items_order_id",
                table: "order_item");

            migrationBuilder.DropForeignKey(
                name: "fk_otp_users_user_id",
                table: "otp");

            migrationBuilder.DropForeignKey(
                name: "fk_region_country_country_id",
                table: "region");

            migrationBuilder.DropForeignKey(
                name: "fk_sale_invoice_invoice_id",
                table: "sale");

            migrationBuilder.DropForeignKey(
                name: "fk_sale_item_item_id",
                table: "sale");

            migrationBuilder.DropForeignKey(
                name: "fk_sale_users_user_id",
                table: "sale");

            migrationBuilder.DropForeignKey(
                name: "fk_supplier_email_email_email_id",
                table: "supplier_email");

            migrationBuilder.DropForeignKey(
                name: "fk_supplier_email_suppliers_supplier_id",
                table: "supplier_email");

            migrationBuilder.DropForeignKey(
                name: "fk_supplier_location_location_location_id",
                table: "supplier_location");

            migrationBuilder.DropForeignKey(
                name: "fk_supplier_location_supplier_supplier_id",
                table: "supplier_location");

            migrationBuilder.DropForeignKey(
                name: "fk_supplier_phone_phone_phone_id",
                table: "supplier_phone");

            migrationBuilder.DropForeignKey(
                name: "fk_supplier_phone_suppliers_supplier_id",
                table: "supplier_phone");

            migrationBuilder.DropForeignKey(
                name: "fk_user_employee_employee_id",
                table: "user");

            migrationBuilder.DropForeignKey(
                name: "fk_user_role_role_id",
                table: "user");

            migrationBuilder.DropForeignKey(
                name: "fk_user_email_email_email_id",
                table: "user_email");

            migrationBuilder.DropForeignKey(
                name: "fk_user_email_users_user_id",
                table: "user_email");

            migrationBuilder.DropForeignKey(
                name: "fk_user_password_user_user_id",
                table: "user_password");

            migrationBuilder.DropForeignKey(
                name: "fk_user_phone_phone_phone_id",
                table: "user_phone");

            migrationBuilder.DropForeignKey(
                name: "fk_user_phone_users_user_id",
                table: "user_phone");

            migrationBuilder.DropForeignKey(
                name: "fk_user_privilege_privilege_privilege_id",
                table: "user_privilege");

            migrationBuilder.DropForeignKey(
                name: "fk_user_privilege_user_user_id",
                table: "user_privilege");

            migrationBuilder.DropForeignKey(
                name: "fk_user_privilege_action_user_performed_by_user_id",
                table: "user_privilege_action");

            migrationBuilder.DropForeignKey(
                name: "fk_user_privilege_action_user_privilege_user_privilege_id",
                table: "user_privilege_action");

            migrationBuilder.DropForeignKey(
                name: "fk_user_token_user_user_id",
                table: "user_token");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_token",
                table: "user_token");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_privilege_action",
                table: "user_privilege_action");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_privilege",
                table: "user_privilege");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_phone",
                table: "user_phone");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_password",
                table: "user_password");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_email",
                table: "user_email");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user",
                table: "user");

            migrationBuilder.DropPrimaryKey(
                name: "pk_unit",
                table: "unit");

            migrationBuilder.DropPrimaryKey(
                name: "pk_supplier_phone",
                table: "supplier_phone");

            migrationBuilder.DropPrimaryKey(
                name: "pk_supplier_location",
                table: "supplier_location");

            migrationBuilder.DropPrimaryKey(
                name: "pk_supplier_email",
                table: "supplier_email");

            migrationBuilder.DropPrimaryKey(
                name: "pk_supplier",
                table: "supplier");

            migrationBuilder.DropPrimaryKey(
                name: "pk_sale",
                table: "sale");

            migrationBuilder.DropPrimaryKey(
                name: "pk_salary",
                table: "salary");

            migrationBuilder.DropPrimaryKey(
                name: "pk_role",
                table: "role");

            migrationBuilder.DropPrimaryKey(
                name: "pk_region",
                table: "region");

            migrationBuilder.DropPrimaryKey(
                name: "pk_privilege",
                table: "privilege");

            migrationBuilder.DropPrimaryKey(
                name: "pk_phone",
                table: "phone");

            migrationBuilder.DropPrimaryKey(
                name: "pk_otp",
                table: "otp");

            migrationBuilder.DropPrimaryKey(
                name: "pk_order_item",
                table: "order_item");

            migrationBuilder.DropPrimaryKey(
                name: "pk_notification",
                table: "notification");

            migrationBuilder.DropPrimaryKey(
                name: "pk_manufacturer_phone",
                table: "manufacturer_phone");

            migrationBuilder.DropPrimaryKey(
                name: "pk_manufacturer_location",
                table: "manufacturer_location");

            migrationBuilder.DropPrimaryKey(
                name: "pk_manufacturer_email",
                table: "manufacturer_email");

            migrationBuilder.DropPrimaryKey(
                name: "pk_manufacturer",
                table: "manufacturer");

            migrationBuilder.DropPrimaryKey(
                name: "pk_location",
                table: "location");

            migrationBuilder.DropPrimaryKey(
                name: "pk_language",
                table: "language");

            migrationBuilder.DropPrimaryKey(
                name: "pk_items_order",
                table: "items_order");

            migrationBuilder.DropPrimaryKey(
                name: "pk_item_expiry",
                table: "item_expiry");

            migrationBuilder.DropPrimaryKey(
                name: "pk_item_code",
                table: "item_code");

            migrationBuilder.DropPrimaryKey(
                name: "pk_item_category",
                table: "item_category");

            migrationBuilder.DropPrimaryKey(
                name: "pk_item",
                table: "item");

            migrationBuilder.DropPrimaryKey(
                name: "pk_invoice_tender",
                table: "invoice_tender");

            migrationBuilder.DropPrimaryKey(
                name: "pk_invoice",
                table: "invoice");

            migrationBuilder.DropPrimaryKey(
                name: "pk_employee_privilege_action",
                table: "employee_privilege_action");

            migrationBuilder.DropPrimaryKey(
                name: "pk_employee_privilege",
                table: "employee_privilege");

            migrationBuilder.DropPrimaryKey(
                name: "pk_employee_phone",
                table: "employee_phone");

            migrationBuilder.DropPrimaryKey(
                name: "pk_employee_location",
                table: "employee_location");

            migrationBuilder.DropPrimaryKey(
                name: "pk_employee_email",
                table: "employee_email");

            migrationBuilder.DropPrimaryKey(
                name: "pk_employee",
                table: "employee");

            migrationBuilder.DropPrimaryKey(
                name: "pk_email",
                table: "email");

            migrationBuilder.DropPrimaryKey(
                name: "pk_document",
                table: "document");

            migrationBuilder.DropPrimaryKey(
                name: "pk_discount",
                table: "discount");

            migrationBuilder.DropPrimaryKey(
                name: "pk_department",
                table: "department");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customer_privilege_action",
                table: "customer_privilege_action");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customer_privilege",
                table: "customer_privilege");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customer_phone",
                table: "customer_phone");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customer_location",
                table: "customer_location");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customer_email",
                table: "customer_email");

            migrationBuilder.DropPrimaryKey(
                name: "pk_customer",
                table: "customer");

            migrationBuilder.DropPrimaryKey(
                name: "pk_currency",
                table: "currency");

            migrationBuilder.DropPrimaryKey(
                name: "pk_country",
                table: "country");

            migrationBuilder.DropPrimaryKey(
                name: "pk_city",
                table: "city");

            migrationBuilder.DropPrimaryKey(
                name: "pk_change_log",
                table: "change_log");

            migrationBuilder.DropPrimaryKey(
                name: "pk_category",
                table: "category");

            migrationBuilder.DropPrimaryKey(
                name: "pk_batch",
                table: "batch");

            migrationBuilder.RenameTable(
                name: "user_token",
                newName: "UserTokens");

            migrationBuilder.RenameTable(
                name: "user_privilege_action",
                newName: "UserPrivilegeActions");

            migrationBuilder.RenameTable(
                name: "user_privilege",
                newName: "UserPrivileges");

            migrationBuilder.RenameTable(
                name: "user_phone",
                newName: "UserPhones");

            migrationBuilder.RenameTable(
                name: "user_password",
                newName: "UserPasswords");

            migrationBuilder.RenameTable(
                name: "user_email",
                newName: "UserEmails");

            migrationBuilder.RenameTable(
                name: "user",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "unit",
                newName: "Units");

            migrationBuilder.RenameTable(
                name: "supplier_phone",
                newName: "SupplierPhones");

            migrationBuilder.RenameTable(
                name: "supplier_location",
                newName: "SupplierLocations");

            migrationBuilder.RenameTable(
                name: "supplier_email",
                newName: "SupplierEmails");

            migrationBuilder.RenameTable(
                name: "supplier",
                newName: "Suppliers");

            migrationBuilder.RenameTable(
                name: "sale",
                newName: "Sales");

            migrationBuilder.RenameTable(
                name: "salary",
                newName: "Salaries");

            migrationBuilder.RenameTable(
                name: "role",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "region",
                newName: "Regions");

            migrationBuilder.RenameTable(
                name: "privilege",
                newName: "Privileges");

            migrationBuilder.RenameTable(
                name: "phone",
                newName: "Phones");

            migrationBuilder.RenameTable(
                name: "otp",
                newName: "Otps");

            migrationBuilder.RenameTable(
                name: "order_item",
                newName: "OrderItems");

            migrationBuilder.RenameTable(
                name: "notification",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "manufacturer_phone",
                newName: "ManufacturerPhones");

            migrationBuilder.RenameTable(
                name: "manufacturer_location",
                newName: "ManufacturerLocations");

            migrationBuilder.RenameTable(
                name: "manufacturer_email",
                newName: "ManufacturerEmails");

            migrationBuilder.RenameTable(
                name: "manufacturer",
                newName: "Manufacturers");

            migrationBuilder.RenameTable(
                name: "location",
                newName: "Locations");

            migrationBuilder.RenameTable(
                name: "language",
                newName: "Languages");

            migrationBuilder.RenameTable(
                name: "items_order",
                newName: "ItemsOrders");

            migrationBuilder.RenameTable(
                name: "item_expiry",
                newName: "ItemExpiries");

            migrationBuilder.RenameTable(
                name: "item_code",
                newName: "ItemCodes");

            migrationBuilder.RenameTable(
                name: "item_category",
                newName: "ItemCategories");

            migrationBuilder.RenameTable(
                name: "item",
                newName: "Items");

            migrationBuilder.RenameTable(
                name: "invoice_tender",
                newName: "InvoiceTenders");

            migrationBuilder.RenameTable(
                name: "invoice",
                newName: "Invoices");

            migrationBuilder.RenameTable(
                name: "employee_privilege_action",
                newName: "EmployeePrivilegeActions");

            migrationBuilder.RenameTable(
                name: "employee_privilege",
                newName: "EmployeePrivileges");

            migrationBuilder.RenameTable(
                name: "employee_phone",
                newName: "EmployeePhones");

            migrationBuilder.RenameTable(
                name: "employee_location",
                newName: "EmployeeLocations");

            migrationBuilder.RenameTable(
                name: "employee_email",
                newName: "EmployeeEmails");

            migrationBuilder.RenameTable(
                name: "employee",
                newName: "Employees");

            migrationBuilder.RenameTable(
                name: "email",
                newName: "Emails");

            migrationBuilder.RenameTable(
                name: "document",
                newName: "Documents");

            migrationBuilder.RenameTable(
                name: "discount",
                newName: "Discounts");

            migrationBuilder.RenameTable(
                name: "department",
                newName: "Departments");

            migrationBuilder.RenameTable(
                name: "customer_privilege_action",
                newName: "CustomerPrivilegeActions");

            migrationBuilder.RenameTable(
                name: "customer_privilege",
                newName: "CustomerPrivileges");

            migrationBuilder.RenameTable(
                name: "customer_phone",
                newName: "CustomerPhones");

            migrationBuilder.RenameTable(
                name: "customer_location",
                newName: "CustomerLocations");

            migrationBuilder.RenameTable(
                name: "customer_email",
                newName: "CustomerEmails");

            migrationBuilder.RenameTable(
                name: "customer",
                newName: "Customers");

            migrationBuilder.RenameTable(
                name: "currency",
                newName: "Currencies");

            migrationBuilder.RenameTable(
                name: "country",
                newName: "Countries");

            migrationBuilder.RenameTable(
                name: "city",
                newName: "Cities");

            migrationBuilder.RenameTable(
                name: "change_log",
                newName: "ChangeLogs");

            migrationBuilder.RenameTable(
                name: "category",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "batch",
                newName: "Batches");

            migrationBuilder.RenameColumn(
                name: "token",
                table: "UserTokens",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserTokens",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "refresh_token_hash",
                table: "UserTokens",
                newName: "RefreshTokenHash");

            migrationBuilder.RenameColumn(
                name: "refresh_token_expiry_date",
                table: "UserTokens",
                newName: "RefreshTokenExpiryDate");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "UserTokens",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_revoked",
                table: "UserTokens",
                newName: "IsRevoked");

            migrationBuilder.RenameColumn(
                name: "expiry_date",
                table: "UserTokens",
                newName: "ExpiryDate");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "UserTokens",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "user_token_id",
                table: "UserTokens",
                newName: "UserTokenId");

            migrationBuilder.RenameIndex(
                name: "ix_user_token_user_id",
                table: "UserTokens",
                newName: "IX_UserTokens_UserId");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "UserPrivilegeActions",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "action",
                table: "UserPrivilegeActions",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "user_privilege_id",
                table: "UserPrivilegeActions",
                newName: "UserPrivilegeId");

            migrationBuilder.RenameColumn(
                name: "performed_by_user_id",
                table: "UserPrivilegeActions",
                newName: "PerformedByUserId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "UserPrivilegeActions",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "UserPrivilegeActions",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "user_privilege_action_id",
                table: "UserPrivilegeActions",
                newName: "UserPrivilegeActionId");

            migrationBuilder.RenameIndex(
                name: "ix_user_privilege_action_user_privilege_id",
                table: "UserPrivilegeActions",
                newName: "IX_UserPrivilegeActions_UserPrivilegeId");

            migrationBuilder.RenameIndex(
                name: "ix_user_privilege_action_performed_by_user_id",
                table: "UserPrivilegeActions",
                newName: "IX_UserPrivilegeActions_PerformedByUserId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "UserPrivileges",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserPrivileges",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "privilege_id",
                table: "UserPrivileges",
                newName: "PrivilegeId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "UserPrivileges",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "UserPrivileges",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "UserPrivileges",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "user_privilege_id",
                table: "UserPrivileges",
                newName: "UserPrivilegeId");

            migrationBuilder.RenameIndex(
                name: "ix_user_privilege_user_id",
                table: "UserPrivileges",
                newName: "IX_UserPrivileges_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_user_privilege_privilege_id",
                table: "UserPrivileges",
                newName: "IX_UserPrivileges_PrivilegeId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserPhones",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "phone_id",
                table: "UserPhones",
                newName: "PhoneId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "UserPhones",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "UserPhones",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "UserPhones",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "user_phone_id",
                table: "UserPhones",
                newName: "UserPhoneId");

            migrationBuilder.RenameIndex(
                name: "ix_user_phone_user_id",
                table: "UserPhones",
                newName: "IX_UserPhones_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_user_phone_phone_id",
                table: "UserPhones",
                newName: "IX_UserPhones_PhoneId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserPasswords",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "UserPasswords",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "UserPasswords",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "UserPasswords",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "user_password_id",
                table: "UserPasswords",
                newName: "UserPasswordId");

            migrationBuilder.RenameIndex(
                name: "ix_user_password_user_id",
                table: "UserPasswords",
                newName: "IX_UserPasswords_UserId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "UserEmails",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "UserEmails",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "UserEmails",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "email_id",
                table: "UserEmails",
                newName: "EmailId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "UserEmails",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "user_email_id",
                table: "UserEmails",
                newName: "UserEmailId");

            migrationBuilder.RenameIndex(
                name: "ix_user_email_user_id",
                table: "UserEmails",
                newName: "IX_UserEmails_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_user_email_email_id",
                table: "UserEmails",
                newName: "IX_UserEmails_EmailId");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Users",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "Users",
                newName: "RoleId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Users",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "Users",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "Users",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Users",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "ix_user_username",
                table: "Users",
                newName: "IX_Users_Username");

            migrationBuilder.RenameIndex(
                name: "ix_user_role_id",
                table: "Users",
                newName: "IX_Users_RoleId");

            migrationBuilder.RenameIndex(
                name: "ix_user_employee_id",
                table: "Users",
                newName: "IX_Users_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Units",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Units",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "abbreviation",
                table: "Units",
                newName: "Abbreviation");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Units",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Units",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "unit_id",
                table: "Units",
                newName: "UnitId");

            migrationBuilder.RenameIndex(
                name: "ix_unit_abbreviation",
                table: "Units",
                newName: "IX_Units_Abbreviation");

            migrationBuilder.RenameColumn(
                name: "supplier_id",
                table: "SupplierPhones",
                newName: "SupplierId");

            migrationBuilder.RenameColumn(
                name: "phone_id",
                table: "SupplierPhones",
                newName: "PhoneId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "SupplierPhones",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "SupplierPhones",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "SupplierPhones",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "supplier_phone_id",
                table: "SupplierPhones",
                newName: "SupplierPhoneId");

            migrationBuilder.RenameIndex(
                name: "ix_supplier_phone_supplier_id",
                table: "SupplierPhones",
                newName: "IX_SupplierPhones_SupplierId");

            migrationBuilder.RenameIndex(
                name: "ix_supplier_phone_phone_id",
                table: "SupplierPhones",
                newName: "IX_SupplierPhones_PhoneId");

            migrationBuilder.RenameColumn(
                name: "supplier_id",
                table: "SupplierLocations",
                newName: "SupplierId");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "SupplierLocations",
                newName: "LocationId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "SupplierLocations",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "SupplierLocations",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "SupplierLocations",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "supplier_location_id",
                table: "SupplierLocations",
                newName: "SupplierLocationId");

            migrationBuilder.RenameIndex(
                name: "ix_supplier_location_supplier_id",
                table: "SupplierLocations",
                newName: "IX_SupplierLocations_SupplierId");

            migrationBuilder.RenameIndex(
                name: "ix_supplier_location_location_id",
                table: "SupplierLocations",
                newName: "IX_SupplierLocations_LocationId");

            migrationBuilder.RenameColumn(
                name: "supplier_id",
                table: "SupplierEmails",
                newName: "SupplierId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "SupplierEmails",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "SupplierEmails",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "email_id",
                table: "SupplierEmails",
                newName: "EmailId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "SupplierEmails",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "supplier_email_id",
                table: "SupplierEmails",
                newName: "SupplierEmailId");

            migrationBuilder.RenameIndex(
                name: "ix_supplier_email_supplier_id",
                table: "SupplierEmails",
                newName: "IX_SupplierEmails_SupplierId");

            migrationBuilder.RenameIndex(
                name: "ix_supplier_email_email_id",
                table: "SupplierEmails",
                newName: "IX_SupplierEmails_EmailId");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "Suppliers",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Suppliers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "registration_number",
                table: "Suppliers",
                newName: "RegistrationNumber");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Suppliers",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "Suppliers",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Suppliers",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "supplier_id",
                table: "Suppliers",
                newName: "SupplierId");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "Sales",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Sales",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "unit_price",
                table: "Sales",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "unit_abbreviation",
                table: "Sales",
                newName: "UnitAbbreviation");

            migrationBuilder.RenameColumn(
                name: "line_total",
                table: "Sales",
                newName: "LineTotal");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Sales",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "item_name",
                table: "Sales",
                newName: "ItemName");

            migrationBuilder.RenameColumn(
                name: "item_id",
                table: "Sales",
                newName: "ItemId");

            migrationBuilder.RenameColumn(
                name: "invoice_id",
                table: "Sales",
                newName: "InvoiceId");

            migrationBuilder.RenameColumn(
                name: "discount_amount",
                table: "Sales",
                newName: "DiscountAmount");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Sales",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "sale_id",
                table: "Sales",
                newName: "SaleId");

            migrationBuilder.RenameIndex(
                name: "ix_sale_user_id",
                table: "Sales",
                newName: "IX_Sales_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_sale_item_id",
                table: "Sales",
                newName: "IX_Sales_ItemId");

            migrationBuilder.RenameIndex(
                name: "ix_sale_invoice_id",
                table: "Sales",
                newName: "IX_Sales_InvoiceId");

            migrationBuilder.RenameColumn(
                name: "grade",
                table: "Salaries",
                newName: "Grade");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Salaries",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Salaries",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Salaries",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "basic_amount",
                table: "Salaries",
                newName: "BasicAmount");

            migrationBuilder.RenameColumn(
                name: "allowance_amount",
                table: "Salaries",
                newName: "AllowanceAmount");

            migrationBuilder.RenameColumn(
                name: "salary_id",
                table: "Salaries",
                newName: "SalaryId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Roles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Roles",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Roles",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Roles",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "Roles",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "ix_role_name",
                table: "Roles",
                newName: "IX_Roles_Name");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Regions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Regions",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Regions",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "country_id",
                table: "Regions",
                newName: "CountryId");

            migrationBuilder.RenameColumn(
                name: "region_id",
                table: "Regions",
                newName: "RegionId");

            migrationBuilder.RenameIndex(
                name: "ix_region_country_id",
                table: "Regions",
                newName: "IX_Regions_CountryId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Privileges",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "module",
                table: "Privileges",
                newName: "Module");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Privileges",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Privileges",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Privileges",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "privilege_id",
                table: "Privileges",
                newName: "PrivilegeId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "Phones",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "number",
                table: "Phones",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Phones",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_verified",
                table: "Phones",
                newName: "IsVerified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Phones",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "country_id",
                table: "Phones",
                newName: "CountryId");

            migrationBuilder.RenameColumn(
                name: "phone_id",
                table: "Phones",
                newName: "PhoneId");

            migrationBuilder.RenameIndex(
                name: "ix_phone_country_id_number",
                table: "Phones",
                newName: "IX_Phones_CountryId_Number");

            migrationBuilder.RenameColumn(
                name: "purpose",
                table: "Otps",
                newName: "Purpose");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "Otps",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Otps",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Otps",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_used",
                table: "Otps",
                newName: "IsUsed");

            migrationBuilder.RenameColumn(
                name: "expires_at",
                table: "Otps",
                newName: "ExpiresAt");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Otps",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "otp_id",
                table: "Otps",
                newName: "OtpId");

            migrationBuilder.RenameIndex(
                name: "ix_otp_user_id",
                table: "Otps",
                newName: "IX_Otps_UserId");

            migrationBuilder.RenameColumn(
                name: "unit_cost",
                table: "OrderItems",
                newName: "UnitCost");

            migrationBuilder.RenameColumn(
                name: "quantity_received",
                table: "OrderItems",
                newName: "QuantityReceived");

            migrationBuilder.RenameColumn(
                name: "quantity_ordered",
                table: "OrderItems",
                newName: "QuantityOrdered");

            migrationBuilder.RenameColumn(
                name: "line_total",
                table: "OrderItems",
                newName: "LineTotal");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "OrderItems",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "items_order_id",
                table: "OrderItems",
                newName: "ItemsOrderId");

            migrationBuilder.RenameColumn(
                name: "item_name",
                table: "OrderItems",
                newName: "ItemName");

            migrationBuilder.RenameColumn(
                name: "item_id",
                table: "OrderItems",
                newName: "ItemId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "OrderItems",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "order_item_id",
                table: "OrderItems",
                newName: "OrderItemId");

            migrationBuilder.RenameIndex(
                name: "ix_order_item_items_order_id",
                table: "OrderItems",
                newName: "IX_OrderItems_ItemsOrderId");

            migrationBuilder.RenameIndex(
                name: "ix_order_item_item_id",
                table: "OrderItems",
                newName: "IX_OrderItems_ItemId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "Notifications",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Notifications",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "Notifications",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Notifications",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Notifications",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_read",
                table: "Notifications",
                newName: "IsRead");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Notifications",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "notification_id",
                table: "Notifications",
                newName: "NotificationId");

            migrationBuilder.RenameIndex(
                name: "ix_notification_user_id",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.RenameColumn(
                name: "phone_id",
                table: "ManufacturerPhones",
                newName: "PhoneId");

            migrationBuilder.RenameColumn(
                name: "manufacturer_id",
                table: "ManufacturerPhones",
                newName: "ManufacturerId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "ManufacturerPhones",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "ManufacturerPhones",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "ManufacturerPhones",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "manufacturer_phone_id",
                table: "ManufacturerPhones",
                newName: "ManufacturerPhoneId");

            migrationBuilder.RenameIndex(
                name: "ix_manufacturer_phone_phone_id",
                table: "ManufacturerPhones",
                newName: "IX_ManufacturerPhones_PhoneId");

            migrationBuilder.RenameIndex(
                name: "ix_manufacturer_phone_manufacturer_id",
                table: "ManufacturerPhones",
                newName: "IX_ManufacturerPhones_ManufacturerId");

            migrationBuilder.RenameColumn(
                name: "manufacturer_id",
                table: "ManufacturerLocations",
                newName: "ManufacturerId");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "ManufacturerLocations",
                newName: "LocationId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "ManufacturerLocations",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "ManufacturerLocations",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "ManufacturerLocations",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "manufacturer_location_id",
                table: "ManufacturerLocations",
                newName: "ManufacturerLocationId");

            migrationBuilder.RenameIndex(
                name: "ix_manufacturer_location_manufacturer_id",
                table: "ManufacturerLocations",
                newName: "IX_ManufacturerLocations_ManufacturerId");

            migrationBuilder.RenameIndex(
                name: "ix_manufacturer_location_location_id",
                table: "ManufacturerLocations",
                newName: "IX_ManufacturerLocations_LocationId");

            migrationBuilder.RenameColumn(
                name: "manufacturer_id",
                table: "ManufacturerEmails",
                newName: "ManufacturerId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "ManufacturerEmails",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "ManufacturerEmails",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "email_id",
                table: "ManufacturerEmails",
                newName: "EmailId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "ManufacturerEmails",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "manufacturer_email_id",
                table: "ManufacturerEmails",
                newName: "ManufacturerEmailId");

            migrationBuilder.RenameIndex(
                name: "ix_manufacturer_email_manufacturer_id",
                table: "ManufacturerEmails",
                newName: "IX_ManufacturerEmails_ManufacturerId");

            migrationBuilder.RenameIndex(
                name: "ix_manufacturer_email_email_id",
                table: "ManufacturerEmails",
                newName: "IX_ManufacturerEmails_EmailId");

            migrationBuilder.RenameColumn(
                name: "website",
                table: "Manufacturers",
                newName: "Website");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "Manufacturers",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Manufacturers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "registration_number",
                table: "Manufacturers",
                newName: "RegistrationNumber");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Manufacturers",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "Manufacturers",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Manufacturers",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "manufacturer_id",
                table: "Manufacturers",
                newName: "ManufacturerId");

            migrationBuilder.RenameColumn(
                name: "longitude",
                table: "Locations",
                newName: "Longitude");

            migrationBuilder.RenameColumn(
                name: "latitude",
                table: "Locations",
                newName: "Latitude");

            migrationBuilder.RenameColumn(
                name: "street_address",
                table: "Locations",
                newName: "StreetAddress");

            migrationBuilder.RenameColumn(
                name: "postal_code",
                table: "Locations",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Locations",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Locations",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "city_id",
                table: "Locations",
                newName: "CityId");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "Locations",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "ix_location_city_id",
                table: "Locations",
                newName: "IX_Locations_CityId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Languages",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "Languages",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Languages",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Languages",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "language_id",
                table: "Languages",
                newName: "LanguageId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "ItemsOrders",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "ItemsOrders",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "ItemsOrders",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "supplier_id",
                table: "ItemsOrders",
                newName: "SupplierId");

            migrationBuilder.RenameColumn(
                name: "order_number",
                table: "ItemsOrders",
                newName: "OrderNumber");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "ItemsOrders",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "expected_delivery_date",
                table: "ItemsOrders",
                newName: "ExpectedDeliveryDate");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "ItemsOrders",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "created_by_user_id",
                table: "ItemsOrders",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "items_order_id",
                table: "ItemsOrders",
                newName: "ItemsOrderId");

            migrationBuilder.RenameIndex(
                name: "ix_items_order_supplier_id",
                table: "ItemsOrders",
                newName: "IX_ItemsOrders_SupplierId");

            migrationBuilder.RenameIndex(
                name: "ix_items_order_order_number",
                table: "ItemsOrders",
                newName: "IX_ItemsOrders_OrderNumber");

            migrationBuilder.RenameIndex(
                name: "ix_items_order_created_by_user_id",
                table: "ItemsOrders",
                newName: "IX_ItemsOrders_CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "ItemExpiries",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "item_id",
                table: "ItemExpiries",
                newName: "ItemId");

            migrationBuilder.RenameColumn(
                name: "expiry_date",
                table: "ItemExpiries",
                newName: "ExpiryDate");

            migrationBuilder.RenameColumn(
                name: "days_warning_before",
                table: "ItemExpiries",
                newName: "DaysWarningBefore");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "ItemExpiries",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "item_expiry_id",
                table: "ItemExpiries",
                newName: "ItemExpiryId");

            migrationBuilder.RenameIndex(
                name: "ix_item_expiry_item_id",
                table: "ItemExpiries",
                newName: "IX_ItemExpiries_ItemId");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "ItemCodes",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "ItemCodes",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "item_id",
                table: "ItemCodes",
                newName: "ItemId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "ItemCodes",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "code_type",
                table: "ItemCodes",
                newName: "CodeType");

            migrationBuilder.RenameColumn(
                name: "item_code_id",
                table: "ItemCodes",
                newName: "ItemCodeId");

            migrationBuilder.RenameIndex(
                name: "ix_item_code_item_id",
                table: "ItemCodes",
                newName: "IX_ItemCodes_ItemId");

            migrationBuilder.RenameIndex(
                name: "ix_item_code_code",
                table: "ItemCodes",
                newName: "IX_ItemCodes_Code");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "ItemCategories",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "item_id",
                table: "ItemCategories",
                newName: "ItemId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "ItemCategories",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "ItemCategories",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "item_category_id",
                table: "ItemCategories",
                newName: "ItemCategoryId");

            migrationBuilder.RenameIndex(
                name: "ix_item_category_item_id",
                table: "ItemCategories",
                newName: "IX_ItemCategories_ItemId");

            migrationBuilder.RenameIndex(
                name: "ix_item_category_category_id",
                table: "ItemCategories",
                newName: "IX_ItemCategories_CategoryId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "Items",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Items",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Items",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "barcode",
                table: "Items",
                newName: "Barcode");

            migrationBuilder.RenameColumn(
                name: "unit_price",
                table: "Items",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "unit_id",
                table: "Items",
                newName: "UnitId");

            migrationBuilder.RenameColumn(
                name: "reorder_level",
                table: "Items",
                newName: "ReorderLevel");

            migrationBuilder.RenameColumn(
                name: "manufacturer_id",
                table: "Items",
                newName: "ManufacturerId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Items",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Items",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "in_stock",
                table: "Items",
                newName: "InStock");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "Items",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Items",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "cost_price",
                table: "Items",
                newName: "CostPrice");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "Items",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "item_id",
                table: "Items",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "ix_item_unit_id",
                table: "Items",
                newName: "IX_Items_UnitId");

            migrationBuilder.RenameIndex(
                name: "ix_item_manufacturer_id",
                table: "Items",
                newName: "IX_Items_ManufacturerId");

            migrationBuilder.RenameIndex(
                name: "ix_item_category_id",
                table: "Items",
                newName: "IX_Items_CategoryId");

            migrationBuilder.RenameIndex(
                name: "ix_item_barcode",
                table: "Items",
                newName: "IX_Items_Barcode");

            migrationBuilder.RenameColumn(
                name: "reference",
                table: "InvoiceTenders",
                newName: "Reference");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "InvoiceTenders",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "payment_type",
                table: "InvoiceTenders",
                newName: "PaymentType");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "InvoiceTenders",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "invoice_id",
                table: "InvoiceTenders",
                newName: "InvoiceId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "InvoiceTenders",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "invoice_tender_id",
                table: "InvoiceTenders",
                newName: "InvoiceTenderId");

            migrationBuilder.RenameIndex(
                name: "ix_invoice_tender_invoice_id",
                table: "InvoiceTenders",
                newName: "IX_InvoiceTenders_InvoiceId");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "Invoices",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "Invoices",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "Invoices",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "payment_type",
                table: "Invoices",
                newName: "PaymentType");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Invoices",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_paid",
                table: "Invoices",
                newName: "IsPaid");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Invoices",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "Invoices",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "change_given",
                table: "Invoices",
                newName: "ChangeGiven");

            migrationBuilder.RenameColumn(
                name: "amount_tendered",
                table: "Invoices",
                newName: "AmountTendered");

            migrationBuilder.RenameColumn(
                name: "invoice_id",
                table: "Invoices",
                newName: "InvoiceId");

            migrationBuilder.RenameIndex(
                name: "ix_invoice_user_id",
                table: "Invoices",
                newName: "IX_Invoices_UserId");

            migrationBuilder.RenameIndex(
                name: "ix_invoice_customer_id",
                table: "Invoices",
                newName: "IX_Invoices_CustomerId");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "EmployeePrivilegeActions",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "action",
                table: "EmployeePrivilegeActions",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "performed_by_user_id",
                table: "EmployeePrivilegeActions",
                newName: "PerformedByUserId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "EmployeePrivilegeActions",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "employee_privilege_id",
                table: "EmployeePrivilegeActions",
                newName: "EmployeePrivilegeId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "EmployeePrivilegeActions",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "employee_privilege_action_id",
                table: "EmployeePrivilegeActions",
                newName: "EmployeePrivilegeActionId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_privilege_action_performed_by_user_id",
                table: "EmployeePrivilegeActions",
                newName: "IX_EmployeePrivilegeActions_PerformedByUserId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_privilege_action_employee_privilege_id",
                table: "EmployeePrivilegeActions",
                newName: "IX_EmployeePrivilegeActions_EmployeePrivilegeId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "EmployeePrivileges",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "privilege_id",
                table: "EmployeePrivileges",
                newName: "PrivilegeId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "EmployeePrivileges",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "EmployeePrivileges",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "EmployeePrivileges",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "EmployeePrivileges",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "employee_privilege_id",
                table: "EmployeePrivileges",
                newName: "EmployeePrivilegeId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_privilege_privilege_id",
                table: "EmployeePrivileges",
                newName: "IX_EmployeePrivileges_PrivilegeId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_privilege_employee_id",
                table: "EmployeePrivileges",
                newName: "IX_EmployeePrivileges_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "phone_id",
                table: "EmployeePhones",
                newName: "PhoneId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "EmployeePhones",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "EmployeePhones",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "EmployeePhones",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "EmployeePhones",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "employee_phone_id",
                table: "EmployeePhones",
                newName: "EmployeePhoneId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_phone_phone_id",
                table: "EmployeePhones",
                newName: "IX_EmployeePhones_PhoneId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_phone_employee_id",
                table: "EmployeePhones",
                newName: "IX_EmployeePhones_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "EmployeeLocations",
                newName: "LocationId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "EmployeeLocations",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "EmployeeLocations",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "EmployeeLocations",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "EmployeeLocations",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "employee_location_id",
                table: "EmployeeLocations",
                newName: "EmployeeLocationId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_location_location_id",
                table: "EmployeeLocations",
                newName: "IX_EmployeeLocations_LocationId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_location_employee_id",
                table: "EmployeeLocations",
                newName: "IX_EmployeeLocations_EmployeeId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "EmployeeEmails",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "EmployeeEmails",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "EmployeeEmails",
                newName: "EmployeeId");

            migrationBuilder.RenameColumn(
                name: "email_id",
                table: "EmployeeEmails",
                newName: "EmailId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "EmployeeEmails",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "employee_email_id",
                table: "EmployeeEmails",
                newName: "EmployeeEmailId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_email_employee_id",
                table: "EmployeeEmails",
                newName: "IX_EmployeeEmails_EmployeeId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_email_email_id",
                table: "EmployeeEmails",
                newName: "IX_EmployeeEmails_EmailId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Employees",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "gender",
                table: "Employees",
                newName: "Gender");

            migrationBuilder.RenameColumn(
                name: "salary_id",
                table: "Employees",
                newName: "SalaryId");

            migrationBuilder.RenameColumn(
                name: "place_of_birth",
                table: "Employees",
                newName: "PlaceOfBirth");

            migrationBuilder.RenameColumn(
                name: "nid_number",
                table: "Employees",
                newName: "NidNumber");

            migrationBuilder.RenameColumn(
                name: "middle_name",
                table: "Employees",
                newName: "MiddleName");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Employees",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Employees",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "Employees",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Employees",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "department_id",
                table: "Employees",
                newName: "DepartmentId");

            migrationBuilder.RenameColumn(
                name: "date_of_birth",
                table: "Employees",
                newName: "DateOfBirth");

            migrationBuilder.RenameColumn(
                name: "date_employed",
                table: "Employees",
                newName: "DateEmployed");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Employees",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "Employees",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_salary_id",
                table: "Employees",
                newName: "IX_Employees_SalaryId");

            migrationBuilder.RenameIndex(
                name: "ix_employee_department_id",
                table: "Employees",
                newName: "IX_Employees_DepartmentId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "Emails",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Emails",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Emails",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_verified",
                table: "Emails",
                newName: "IsVerified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Emails",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "email_id",
                table: "Emails",
                newName: "EmailId");

            migrationBuilder.RenameIndex(
                name: "ix_email_address",
                table: "Emails",
                newName: "IX_Emails_Address");

            migrationBuilder.RenameColumn(
                name: "uploaded_by_user_id",
                table: "Documents",
                newName: "UploadedByUserId");

            migrationBuilder.RenameColumn(
                name: "mime_type",
                table: "Documents",
                newName: "MimeType");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Documents",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "file_size_bytes",
                table: "Documents",
                newName: "FileSizeBytes");

            migrationBuilder.RenameColumn(
                name: "file_path",
                table: "Documents",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "file_name",
                table: "Documents",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "entity_name",
                table: "Documents",
                newName: "EntityName");

            migrationBuilder.RenameColumn(
                name: "entity_id",
                table: "Documents",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Documents",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "document_id",
                table: "Documents",
                newName: "DocumentId");

            migrationBuilder.RenameColumn(
                name: "percentage",
                table: "Discounts",
                newName: "Percentage");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Discounts",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "valid_to",
                table: "Discounts",
                newName: "ValidTo");

            migrationBuilder.RenameColumn(
                name: "valid_from",
                table: "Discounts",
                newName: "ValidFrom");

            migrationBuilder.RenameColumn(
                name: "managed_by_user_id",
                table: "Discounts",
                newName: "ManagedByUserId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Discounts",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "item_id",
                table: "Discounts",
                newName: "ItemId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Discounts",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Discounts",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "discount_id",
                table: "Discounts",
                newName: "DiscountId");

            migrationBuilder.RenameIndex(
                name: "ix_discount_managed_by_user_id",
                table: "Discounts",
                newName: "IX_Discounts_ManagedByUserId");

            migrationBuilder.RenameIndex(
                name: "ix_discount_item_id",
                table: "Discounts",
                newName: "IX_Discounts_ItemId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Departments",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Departments",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Departments",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Departments",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "department_id",
                table: "Departments",
                newName: "DepartmentId");

            migrationBuilder.RenameIndex(
                name: "ix_department_name",
                table: "Departments",
                newName: "IX_Departments_Name");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "CustomerPrivilegeActions",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "action",
                table: "CustomerPrivilegeActions",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "performed_by_user_id",
                table: "CustomerPrivilegeActions",
                newName: "PerformedByUserId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "CustomerPrivilegeActions",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "CustomerPrivilegeActions",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "customer_privilege_id",
                table: "CustomerPrivilegeActions",
                newName: "CustomerPrivilegeId");

            migrationBuilder.RenameColumn(
                name: "customer_privilege_action_id",
                table: "CustomerPrivilegeActions",
                newName: "CustomerPrivilegeActionId");

            migrationBuilder.RenameIndex(
                name: "ix_customer_privilege_action_performed_by_user_id",
                table: "CustomerPrivilegeActions",
                newName: "IX_CustomerPrivilegeActions_PerformedByUserId");

            migrationBuilder.RenameIndex(
                name: "ix_customer_privilege_action_customer_privilege_id",
                table: "CustomerPrivilegeActions",
                newName: "IX_CustomerPrivilegeActions_CustomerPrivilegeId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "CustomerPrivileges",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "privilege_id",
                table: "CustomerPrivileges",
                newName: "PrivilegeId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "CustomerPrivileges",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "CustomerPrivileges",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "CustomerPrivileges",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "CustomerPrivileges",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "customer_privilege_id",
                table: "CustomerPrivileges",
                newName: "CustomerPrivilegeId");

            migrationBuilder.RenameIndex(
                name: "ix_customer_privilege_privilege_id",
                table: "CustomerPrivileges",
                newName: "IX_CustomerPrivileges_PrivilegeId");

            migrationBuilder.RenameIndex(
                name: "ix_customer_privilege_customer_id",
                table: "CustomerPrivileges",
                newName: "IX_CustomerPrivileges_CustomerId");

            migrationBuilder.RenameColumn(
                name: "phone_id",
                table: "CustomerPhones",
                newName: "PhoneId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "CustomerPhones",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "CustomerPhones",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "CustomerPhones",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "CustomerPhones",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "customer_phone_id",
                table: "CustomerPhones",
                newName: "CustomerPhoneId");

            migrationBuilder.RenameIndex(
                name: "ix_customer_phone_phone_id",
                table: "CustomerPhones",
                newName: "IX_CustomerPhones_PhoneId");

            migrationBuilder.RenameIndex(
                name: "ix_customer_phone_customer_id",
                table: "CustomerPhones",
                newName: "IX_CustomerPhones_CustomerId");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "CustomerLocations",
                newName: "LocationId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "CustomerLocations",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "CustomerLocations",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "CustomerLocations",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "CustomerLocations",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "customer_location_id",
                table: "CustomerLocations",
                newName: "CustomerLocationId");

            migrationBuilder.RenameIndex(
                name: "ix_customer_location_location_id",
                table: "CustomerLocations",
                newName: "IX_CustomerLocations_LocationId");

            migrationBuilder.RenameIndex(
                name: "ix_customer_location_customer_id",
                table: "CustomerLocations",
                newName: "IX_CustomerLocations_CustomerId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "CustomerEmails",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_primary",
                table: "CustomerEmails",
                newName: "IsPrimary");

            migrationBuilder.RenameColumn(
                name: "email_id",
                table: "CustomerEmails",
                newName: "EmailId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "CustomerEmails",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "CustomerEmails",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "customer_email_id",
                table: "CustomerEmails",
                newName: "CustomerEmailId");

            migrationBuilder.RenameIndex(
                name: "ix_customer_email_email_id",
                table: "CustomerEmails",
                newName: "IX_CustomerEmails_EmailId");

            migrationBuilder.RenameIndex(
                name: "ix_customer_email_customer_id",
                table: "CustomerEmails",
                newName: "IX_CustomerEmails_CustomerId");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "Customers",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "gender",
                table: "Customers",
                newName: "Gender");

            migrationBuilder.RenameColumn(
                name: "middle_name",
                table: "Customers",
                newName: "MiddleName");

            migrationBuilder.RenameColumn(
                name: "last_name",
                table: "Customers",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Customers",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "Customers",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "first_name",
                table: "Customers",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "date_of_birth",
                table: "Customers",
                newName: "DateOfBirth");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Customers",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "Customers",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "symbol",
                table: "Currencies",
                newName: "Symbol");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Currencies",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "Currencies",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Currencies",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Currencies",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "currency_id",
                table: "Currencies",
                newName: "CurrencyId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Countries",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "phone_code",
                table: "Countries",
                newName: "PhoneCode");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Countries",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "iso_code",
                table: "Countries",
                newName: "IsoCode");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Countries",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "country_id",
                table: "Countries",
                newName: "CountryId");

            migrationBuilder.RenameIndex(
                name: "ix_country_iso_code",
                table: "Countries",
                newName: "IX_Countries_IsoCode");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Cities",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "region_id",
                table: "Cities",
                newName: "RegionId");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Cities",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Cities",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "city_id",
                table: "Cities",
                newName: "CityId");

            migrationBuilder.RenameIndex(
                name: "ix_city_region_id",
                table: "Cities",
                newName: "IX_Cities_RegionId");

            migrationBuilder.RenameColumn(
                name: "action",
                table: "ChangeLogs",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "ChangeLogs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "old_values",
                table: "ChangeLogs",
                newName: "OldValues");

            migrationBuilder.RenameColumn(
                name: "new_values",
                table: "ChangeLogs",
                newName: "NewValues");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "ChangeLogs",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "ip_address",
                table: "ChangeLogs",
                newName: "IpAddress");

            migrationBuilder.RenameColumn(
                name: "entity_name",
                table: "ChangeLogs",
                newName: "EntityName");

            migrationBuilder.RenameColumn(
                name: "entity_id",
                table: "ChangeLogs",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "ChangeLogs",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "change_log_id",
                table: "ChangeLogs",
                newName: "ChangeLogId");

            migrationBuilder.RenameIndex(
                name: "ix_change_log_user_id",
                table: "ChangeLogs",
                newName: "IX_ChangeLogs_UserId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Categories",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Categories",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Categories",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "Categories",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Categories",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "category_id",
                table: "Categories",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "ix_category_name",
                table: "Categories",
                newName: "IX_Categories_Name");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "Batches",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "Batches",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "received_date",
                table: "Batches",
                newName: "ReceivedDate");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Batches",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "item_id",
                table: "Batches",
                newName: "ItemId");

            migrationBuilder.RenameColumn(
                name: "expiry_date",
                table: "Batches",
                newName: "ExpiryDate");

            migrationBuilder.RenameColumn(
                name: "date_created",
                table: "Batches",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "cost_price",
                table: "Batches",
                newName: "CostPrice");

            migrationBuilder.RenameColumn(
                name: "batch_number",
                table: "Batches",
                newName: "BatchNumber");

            migrationBuilder.RenameColumn(
                name: "batch_id",
                table: "Batches",
                newName: "BatchId");

            migrationBuilder.RenameIndex(
                name: "ix_batch_item_id",
                table: "Batches",
                newName: "IX_Batches_ItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTokens",
                table: "UserTokens",
                column: "UserTokenId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPrivilegeActions",
                table: "UserPrivilegeActions",
                column: "UserPrivilegeActionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPrivileges",
                table: "UserPrivileges",
                column: "UserPrivilegeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPhones",
                table: "UserPhones",
                column: "UserPhoneId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPasswords",
                table: "UserPasswords",
                column: "UserPasswordId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserEmails",
                table: "UserEmails",
                column: "UserEmailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Units",
                table: "Units",
                column: "UnitId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupplierPhones",
                table: "SupplierPhones",
                column: "SupplierPhoneId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupplierLocations",
                table: "SupplierLocations",
                column: "SupplierLocationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupplierEmails",
                table: "SupplierEmails",
                column: "SupplierEmailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Suppliers",
                table: "Suppliers",
                column: "SupplierId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sales",
                table: "Sales",
                column: "SaleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Salaries",
                table: "Salaries",
                column: "SalaryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "RoleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Regions",
                table: "Regions",
                column: "RegionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Privileges",
                table: "Privileges",
                column: "PrivilegeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Phones",
                table: "Phones",
                column: "PhoneId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Otps",
                table: "Otps",
                column: "OtpId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "OrderItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "NotificationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManufacturerPhones",
                table: "ManufacturerPhones",
                column: "ManufacturerPhoneId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManufacturerLocations",
                table: "ManufacturerLocations",
                column: "ManufacturerLocationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ManufacturerEmails",
                table: "ManufacturerEmails",
                column: "ManufacturerEmailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Manufacturers",
                table: "Manufacturers",
                column: "ManufacturerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locations",
                table: "Locations",
                column: "LocationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Languages",
                table: "Languages",
                column: "LanguageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemsOrders",
                table: "ItemsOrders",
                column: "ItemsOrderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemExpiries",
                table: "ItemExpiries",
                column: "ItemExpiryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemCodes",
                table: "ItemCodes",
                column: "ItemCodeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemCategories",
                table: "ItemCategories",
                column: "ItemCategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Items",
                table: "Items",
                column: "ItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InvoiceTenders",
                table: "InvoiceTenders",
                column: "InvoiceTenderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invoices",
                table: "Invoices",
                column: "InvoiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeePrivilegeActions",
                table: "EmployeePrivilegeActions",
                column: "EmployeePrivilegeActionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeePrivileges",
                table: "EmployeePrivileges",
                column: "EmployeePrivilegeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeePhones",
                table: "EmployeePhones",
                column: "EmployeePhoneId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeLocations",
                table: "EmployeeLocations",
                column: "EmployeeLocationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeEmails",
                table: "EmployeeEmails",
                column: "EmployeeEmailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emails",
                table: "Emails",
                column: "EmailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documents",
                table: "Documents",
                column: "DocumentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Discounts",
                table: "Discounts",
                column: "DiscountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Departments",
                table: "Departments",
                column: "DepartmentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerPrivilegeActions",
                table: "CustomerPrivilegeActions",
                column: "CustomerPrivilegeActionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerPrivileges",
                table: "CustomerPrivileges",
                column: "CustomerPrivilegeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerPhones",
                table: "CustomerPhones",
                column: "CustomerPhoneId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerLocations",
                table: "CustomerLocations",
                column: "CustomerLocationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomerEmails",
                table: "CustomerEmails",
                column: "CustomerEmailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Customers",
                table: "Customers",
                column: "CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies",
                column: "CurrencyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countries",
                table: "Countries",
                column: "CountryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cities",
                table: "Cities",
                column: "CityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChangeLogs",
                table: "ChangeLogs",
                column: "ChangeLogId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Batches",
                table: "Batches",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Batches_Items_ItemId",
                table: "Batches",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChangeLogs_Users_UserId",
                table: "ChangeLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Regions_RegionId",
                table: "Cities",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "RegionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerEmails_Customers_CustomerId",
                table: "CustomerEmails",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerEmails_Emails_EmailId",
                table: "CustomerEmails",
                column: "EmailId",
                principalTable: "Emails",
                principalColumn: "EmailId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerLocations_Customers_CustomerId",
                table: "CustomerLocations",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerLocations_Locations_LocationId",
                table: "CustomerLocations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPhones_Customers_CustomerId",
                table: "CustomerPhones",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPhones_Phones_PhoneId",
                table: "CustomerPhones",
                column: "PhoneId",
                principalTable: "Phones",
                principalColumn: "PhoneId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPrivilegeActions_CustomerPrivileges_CustomerPrivileg~",
                table: "CustomerPrivilegeActions",
                column: "CustomerPrivilegeId",
                principalTable: "CustomerPrivileges",
                principalColumn: "CustomerPrivilegeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPrivilegeActions_Users_PerformedByUserId",
                table: "CustomerPrivilegeActions",
                column: "PerformedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPrivileges_Customers_CustomerId",
                table: "CustomerPrivileges",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPrivileges_Privileges_PrivilegeId",
                table: "CustomerPrivileges",
                column: "PrivilegeId",
                principalTable: "Privileges",
                principalColumn: "PrivilegeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Discounts_Items_ItemId",
                table: "Discounts",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Discounts_Users_ManagedByUserId",
                table: "Discounts",
                column: "ManagedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEmails_Emails_EmailId",
                table: "EmployeeEmails",
                column: "EmailId",
                principalTable: "Emails",
                principalColumn: "EmailId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeEmails_Employees_EmployeeId",
                table: "EmployeeEmails",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeLocations_Employees_EmployeeId",
                table: "EmployeeLocations",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeLocations_Locations_LocationId",
                table: "EmployeeLocations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePhones_Employees_EmployeeId",
                table: "EmployeePhones",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePhones_Phones_PhoneId",
                table: "EmployeePhones",
                column: "PhoneId",
                principalTable: "Phones",
                principalColumn: "PhoneId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePrivilegeActions_EmployeePrivileges_EmployeePrivileg~",
                table: "EmployeePrivilegeActions",
                column: "EmployeePrivilegeId",
                principalTable: "EmployeePrivileges",
                principalColumn: "EmployeePrivilegeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePrivilegeActions_Users_PerformedByUserId",
                table: "EmployeePrivilegeActions",
                column: "PerformedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePrivileges_Employees_EmployeeId",
                table: "EmployeePrivileges",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeePrivileges_Privileges_PrivilegeId",
                table: "EmployeePrivileges",
                column: "PrivilegeId",
                principalTable: "Privileges",
                principalColumn: "PrivilegeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Salaries_SalaryId",
                table: "Employees",
                column: "SalaryId",
                principalTable: "Salaries",
                principalColumn: "SalaryId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Customers_CustomerId",
                table: "Invoices",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Users_UserId",
                table: "Invoices",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceTenders_Invoices_InvoiceId",
                table: "InvoiceTenders",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "InvoiceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategories_Categories_CategoryId",
                table: "ItemCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategories_Items_ItemId",
                table: "ItemCategories",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCodes_Items_ItemId",
                table: "ItemCodes",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemExpiries_Items_ItemId",
                table: "ItemExpiries",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Categories_CategoryId",
                table: "Items",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Manufacturers_ManufacturerId",
                table: "Items",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "ManufacturerId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Units_UnitId",
                table: "Items",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "UnitId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsOrders_Suppliers_SupplierId",
                table: "ItemsOrders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsOrders_Users_CreatedByUserId",
                table: "ItemsOrders",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Cities_CityId",
                table: "Locations",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "CityId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ManufacturerEmails_Emails_EmailId",
                table: "ManufacturerEmails",
                column: "EmailId",
                principalTable: "Emails",
                principalColumn: "EmailId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManufacturerEmails_Manufacturers_ManufacturerId",
                table: "ManufacturerEmails",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "ManufacturerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManufacturerLocations_Locations_LocationId",
                table: "ManufacturerLocations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManufacturerLocations_Manufacturers_ManufacturerId",
                table: "ManufacturerLocations",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "ManufacturerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManufacturerPhones_Manufacturers_ManufacturerId",
                table: "ManufacturerPhones",
                column: "ManufacturerId",
                principalTable: "Manufacturers",
                principalColumn: "ManufacturerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ManufacturerPhones_Phones_PhoneId",
                table: "ManufacturerPhones",
                column: "PhoneId",
                principalTable: "Phones",
                principalColumn: "PhoneId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ItemsOrders_ItemsOrderId",
                table: "OrderItems",
                column: "ItemsOrderId",
                principalTable: "ItemsOrders",
                principalColumn: "ItemsOrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Items_ItemId",
                table: "OrderItems",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Otps_Users_UserId",
                table: "Otps",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Regions_Countries_CountryId",
                table: "Regions",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "CountryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Invoices_InvoiceId",
                table: "Sales",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "InvoiceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Items_ItemId",
                table: "Sales",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Users_UserId",
                table: "Sales",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierEmails_Emails_EmailId",
                table: "SupplierEmails",
                column: "EmailId",
                principalTable: "Emails",
                principalColumn: "EmailId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierEmails_Suppliers_SupplierId",
                table: "SupplierEmails",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierLocations_Locations_LocationId",
                table: "SupplierLocations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierLocations_Suppliers_SupplierId",
                table: "SupplierLocations",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierPhones_Phones_PhoneId",
                table: "SupplierPhones",
                column: "PhoneId",
                principalTable: "Phones",
                principalColumn: "PhoneId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierPhones_Suppliers_SupplierId",
                table: "SupplierPhones",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEmails_Emails_EmailId",
                table: "UserEmails",
                column: "EmailId",
                principalTable: "Emails",
                principalColumn: "EmailId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserEmails_Users_UserId",
                table: "UserEmails",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPasswords_Users_UserId",
                table: "UserPasswords",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPhones_Phones_PhoneId",
                table: "UserPhones",
                column: "PhoneId",
                principalTable: "Phones",
                principalColumn: "PhoneId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPhones_Users_UserId",
                table: "UserPhones",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrivilegeActions_UserPrivileges_UserPrivilegeId",
                table: "UserPrivilegeActions",
                column: "UserPrivilegeId",
                principalTable: "UserPrivileges",
                principalColumn: "UserPrivilegeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrivilegeActions_Users_PerformedByUserId",
                table: "UserPrivilegeActions",
                column: "PerformedByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrivileges_Privileges_PrivilegeId",
                table: "UserPrivileges",
                column: "PrivilegeId",
                principalTable: "Privileges",
                principalColumn: "PrivilegeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPrivileges_Users_UserId",
                table: "UserPrivileges",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Employees_EmployeeId",
                table: "Users",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserId",
                table: "UserTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
