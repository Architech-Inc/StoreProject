using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteColumns2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 9, 13, 21, 56, 27, 75, DateTimeKind.Utc).AddTicks(4351));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 9, 13, 21, 54, 37, 962, DateTimeKind.Utc).AddTicks(6244));
        }
    }
}
