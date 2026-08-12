using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContactChangeStatusToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "contact_change_request",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 8, 12, 23, 5, 0, 982, DateTimeKind.Utc).AddTicks(828));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "contact_change_request",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 8, 12, 14, 32, 43, 398, DateTimeKind.Utc).AddTicks(5887));
        }
    }
}
