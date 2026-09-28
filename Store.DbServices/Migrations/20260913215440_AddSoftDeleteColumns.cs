using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "wastage_entry",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "wastage_entry",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "wastage_entry",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "user_token",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "user_token",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "user_token",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "user_phone",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "user_phone",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "user_phone",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "user_password",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "user_password",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "user_password",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "user_email",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "user_email",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "user_email",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "user_branch_role",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "user_branch_role",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "user_branch_role",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "user",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "user",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "user",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "unit",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "unit",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "unit",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "tax_profile",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "tax_profile",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "tax_profile",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "tax_bracket",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "tax_bracket",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "tax_bracket",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "supplier_phone",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "supplier_phone",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "supplier_phone",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "supplier_location",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "supplier_location",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "supplier_location",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "supplier_email",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "supplier_email",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "supplier_email",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "supplier",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "supplier",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "supplier",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "stock_transfer_item",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "stock_transfer_item",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "stock_transfer_item",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "stock_transfer",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "stock_transfer",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "stock_transfer",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "stock_movement",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "stock_movement",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "stock_movement",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "sale",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "sale",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "sale",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "salary",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "salary",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "salary",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "role_permission",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "role_permission",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "role_permission",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "role",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "role",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "role",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "restock_recommendation",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "restock_recommendation",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "restock_recommendation",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "region",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "region",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "region",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "purchase_order_item",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "purchase_order_item",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "purchase_order_item",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "purchase_order",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "purchase_order",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "purchase_order",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "phone",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "phone",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "phone",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "personnel_transfer_history",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "personnel_transfer_history",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "personnel_transfer_history",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "payslip",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "payslip",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "payslip",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "payroll_run",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "payroll_run",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "payroll_run",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "password_reset_token",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "password_reset_token",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "password_reset_token",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "otp",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "otp",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "otp",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "order_item",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "order_item",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "order_item",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "notification",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "notification",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "notification",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "mobile_money_transaction",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "mobile_money_transaction",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "mobile_money_transaction",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "manufacturer_phone",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "manufacturer_phone",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "manufacturer_phone",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "manufacturer_location",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "manufacturer_location",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "manufacturer_location",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "manufacturer_email",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "manufacturer_email",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "manufacturer_email",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "manufacturer",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "manufacturer",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "manufacturer",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "loyalty_transaction",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "loyalty_transaction",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "loyalty_transaction",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "loyalty_campaign_branch",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "loyalty_campaign_branch",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "loyalty_campaign_branch",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "loyalty_campaign",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "loyalty_campaign",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "loyalty_campaign",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "location",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "location",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "location",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "language",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "language",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "language",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "journal_entry_line",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "journal_entry_line",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "journal_entry_line",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "journal_entry",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "journal_entry",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "journal_entry",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "items_order",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "items_order",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "items_order",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "item_expiry",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "item_expiry",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "item_expiry",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "item_code",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "item_code",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "item_code",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "item_category",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "item_category",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "item_category",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "item",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "item",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "item",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "invoice_tender",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "invoice_tender",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "invoice_tender",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "invoice",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "invoice",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "invoice",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "fido_credential",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "fido_credential",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "fido_credential",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "employee_phone",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "employee_phone",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "employee_phone",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "employee_email",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "employee_email",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "employee_email",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "employee_contract",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "employee_contract",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "employee_contract",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "employee",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "employee",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "employee",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "email",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "email",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "email",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "document",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "document",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "document",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "discount_override_request",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "discount_override_request",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "discount_override_request",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "discount_branch",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "discount_branch",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "discount_branch",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "discount",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "discount",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "discount",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "department",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "department",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "department",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "customer_segment_price",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "customer_segment_price",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "customer_segment_price",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "customer_phone",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "customer_phone",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "customer_phone",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "customer_loyalty_account",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "customer_loyalty_account",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "customer_loyalty_account",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "customer_email",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "customer_email",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "customer_email",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "customer",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "customer",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "customer",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "currency",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "currency",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "currency",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "country",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "country",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "country",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "contact_change_request",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "contact_change_request",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "contact_change_request",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "city",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "city",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "city",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "change_log",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "change_log",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "change_log",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "category",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "category",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "category",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "cashier_shift",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "cashier_shift",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "cashier_shift",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "cash_variance_record",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "cash_variance_record",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "cash_variance_record",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "bundle_rule",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "bundle_rule",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "bundle_rule",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "branch_item_stock",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "branch_item_stock",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "branch_item_stock",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "branch",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "branch",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "branch",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "batch",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "batch",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "batch",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AlterColumn<string>(
            name: "action",
            table: "audit_log",
            type: "varchar(255)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "longtext")
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "audit_log",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "audit_log",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "audit_log",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
            name: "deleted_at",
            table: "account",
            type: "datetime(6)",
            nullable: true);

            migrationBuilder.AddColumn<Guid>(
            name: "deleted_by_id",
            table: "account",
            type: "char(36)",
            nullable: true,
            collation: "ascii_general_ci");

            migrationBuilder.AddColumn<bool>(
            name: "is_deleted",
            table: "account",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

            migrationBuilder.UpdateData(
            table: "role",
            keyColumn: "role_id",
            keyValue: 1,
            columns: new[] { "deleted_at", "deleted_by_id", "is_deleted" },
            values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
            table: "role",
            keyColumn: "role_id",
            keyValue: 2,
            columns: new[] { "deleted_at", "deleted_by_id", "is_deleted" },
            values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
            table: "role",
            keyColumn: "role_id",
            keyValue: 3,
            columns: new[] { "deleted_at", "deleted_by_id", "is_deleted" },
            values: new object[] { null, null, false });

            migrationBuilder.UpdateData(
            table: "system_setting",
            keyColumn: "setting_key",
            keyValue: "Auth:PasswordRecoveryMethod",
            column: "last_modified",
            value: new DateTime(2026, 9, 13, 21, 54, 37, 962, DateTimeKind.Utc).AddTicks(6244));

            migrationBuilder.CreateIndex(
            name: "ix_supplier_registration_number",
            table: "supplier",
            column: "registration_number",
            filter: "registration_number IS NOT NULL");

            migrationBuilder.CreateIndex(
            name: "ix_cashier_shift_cashier_shift_id",
            table: "cashier_shift",
            column: "cashier_shift_id",
            unique: true);

            migrationBuilder.CreateIndex(
            name: "ix_cashier_shift_opened_by_user_id_opened_at_utc",
            table: "cashier_shift",
            columns: new[] { "opened_by_user_id", "opened_at_utc" });

            migrationBuilder.CreateIndex(
            name: "ix_cashier_shift_status",
            table: "cashier_shift",
            column: "status");

            migrationBuilder.CreateIndex(
            name: "ix_audit_log_action_date_created",
            table: "audit_log",
            columns: new[] { "action", "date_created" });

            migrationBuilder.CreateIndex(
            name: "ix_audit_log_user_id_date_created",
            table: "audit_log",
            columns: new[] { "user_id", "date_created" });

            migrationBuilder.DropIndex(
            name: "ix_cashier_shift_opened_by_user_id",
            table: "cashier_shift");

            migrationBuilder.DropIndex(
            name: "ix_audit_log_user_id",
            table: "audit_log");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_supplier_registration_number",
                table: "supplier");

            migrationBuilder.DropIndex(
                name: "ix_cashier_shift_cashier_shift_id",
                table: "cashier_shift");

            migrationBuilder.DropIndex(
                name: "ix_cashier_shift_opened_by_user_id_opened_at_utc",
                table: "cashier_shift");

            migrationBuilder.DropIndex(
                name: "ix_cashier_shift_status",
                table: "cashier_shift");

            migrationBuilder.DropIndex(
                name: "ix_audit_log_action_date_created",
                table: "audit_log");

            migrationBuilder.DropIndex(
                name: "ix_audit_log_user_id_date_created",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "wastage_entry");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "wastage_entry");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "wastage_entry");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "user_token");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "user_token");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "user_token");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "user_phone");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "user_phone");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "user_phone");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "user_password");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "user_password");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "user_password");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "user_email");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "user_email");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "user_email");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "user_branch_role");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "user_branch_role");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "user_branch_role");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "user");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "user");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "user");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "unit");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "unit");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "unit");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "tax_profile");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "tax_profile");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "tax_profile");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "tax_bracket");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "tax_bracket");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "tax_bracket");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "supplier_phone");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "supplier_phone");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "supplier_phone");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "supplier_location");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "supplier_location");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "supplier_location");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "supplier_email");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "supplier_email");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "supplier_email");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "supplier");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "supplier");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "supplier");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "stock_transfer_item");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "stock_transfer_item");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "stock_transfer_item");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "stock_transfer");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "stock_transfer");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "stock_transfer");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "stock_movement");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "stock_movement");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "stock_movement");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "sale");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "sale");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "sale");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "salary");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "salary");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "salary");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "role_permission");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "role_permission");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "role_permission");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "role");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "role");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "role");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "restock_recommendation");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "restock_recommendation");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "restock_recommendation");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "region");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "region");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "region");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "purchase_order_item");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "purchase_order_item");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "purchase_order_item");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "phone");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "phone");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "phone");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "personnel_transfer_history");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "personnel_transfer_history");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "personnel_transfer_history");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "payslip");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "payslip");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "payslip");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "payroll_run");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "payroll_run");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "payroll_run");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "password_reset_token");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "password_reset_token");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "password_reset_token");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "otp");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "otp");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "otp");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "order_item");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "notification");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "notification");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "notification");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "mobile_money_transaction");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "mobile_money_transaction");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "mobile_money_transaction");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "manufacturer_phone");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "manufacturer_phone");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "manufacturer_phone");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "manufacturer_location");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "manufacturer_location");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "manufacturer_location");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "manufacturer_email");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "manufacturer_email");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "manufacturer_email");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "manufacturer");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "manufacturer");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "manufacturer");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "loyalty_transaction");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "loyalty_transaction");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "loyalty_transaction");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "loyalty_campaign_branch");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "loyalty_campaign_branch");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "loyalty_campaign_branch");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "loyalty_campaign");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "loyalty_campaign");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "loyalty_campaign");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "location");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "location");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "location");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "language");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "language");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "language");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "journal_entry_line");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "journal_entry_line");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "journal_entry_line");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "journal_entry");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "journal_entry");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "journal_entry");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "items_order");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "items_order");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "items_order");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "item_expiry");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "item_expiry");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "item_expiry");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "item_code");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "item_code");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "item_code");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "item_category");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "item_category");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "item_category");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "item");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "item");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "item");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "invoice_tender");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "invoice_tender");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "invoice_tender");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "fido_credential");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "fido_credential");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "fido_credential");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "employee_phone");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "employee_phone");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "employee_phone");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "employee_email");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "employee_email");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "employee_email");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "employee_contract");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "employee_contract");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "employee_contract");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "email");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "email");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "email");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "document");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "document");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "document");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "discount_override_request");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "discount_override_request");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "discount_override_request");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "discount_branch");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "discount_branch");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "discount_branch");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "department");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "department");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "department");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "customer_segment_price");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "customer_segment_price");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "customer_segment_price");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "customer_phone");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "customer_phone");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "customer_phone");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "customer_loyalty_account");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "customer_loyalty_account");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "customer_loyalty_account");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "customer_email");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "customer_email");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "customer_email");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "currency");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "currency");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "currency");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "country");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "country");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "country");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "contact_change_request");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "contact_change_request");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "contact_change_request");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "city");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "city");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "city");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "change_log");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "change_log");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "change_log");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "category");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "category");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "category");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "cashier_shift");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "cashier_shift");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "cashier_shift");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "cash_variance_record");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "cash_variance_record");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "cash_variance_record");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "bundle_rule");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "bundle_rule");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "bundle_rule");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "branch_item_stock");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "branch_item_stock");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "branch_item_stock");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "batch");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "batch");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "batch");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "account");

            migrationBuilder.DropColumn(
                name: "deleted_by_id",
                table: "account");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "account");

            migrationBuilder.AlterColumn<string>(
                name: "action",
                table: "audit_log",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 9, 6, 18, 35, 38, 100, DateTimeKind.Utc).AddTicks(414));

            migrationBuilder.CreateIndex(
                name: "ix_cashier_shift_opened_by_user_id",
                table: "cashier_shift",
                column: "opened_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_user_id",
                table: "audit_log",
                column: "user_id");
        }
    }
}
