using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "auto_send_purchase_orders",
                table: "supplier",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "due_date",
                table: "purchase_order",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_paid",
                table: "purchase_order",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "commissions",
                table: "payslip",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_commissions",
                table: "payroll_run",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "due_date",
                table: "invoice",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "commission_basis",
                table: "employee_contract",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "commission_rate",
                table: "employee_contract",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 9, 6, 7, 25, 19, 426, DateTimeKind.Utc).AddTicks(2409));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "auto_send_purchase_orders",
                table: "supplier");

            migrationBuilder.DropColumn(
                name: "due_date",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "is_paid",
                table: "purchase_order");

            migrationBuilder.DropColumn(
                name: "commissions",
                table: "payslip");

            migrationBuilder.DropColumn(
                name: "total_commissions",
                table: "payroll_run");

            migrationBuilder.DropColumn(
                name: "due_date",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "commission_basis",
                table: "employee_contract");

            migrationBuilder.DropColumn(
                name: "commission_rate",
                table: "employee_contract");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 9, 4, 4, 50, 51, 336, DateTimeKind.Utc).AddTicks(8637));
        }
    }
}
