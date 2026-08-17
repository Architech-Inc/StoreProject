using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class SupportMultipleUserSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_token_user_user_id",
                table: "user_token");

            migrationBuilder.DropIndex(
                name: "ix_user_token_user_id",
                table: "user_token");

            migrationBuilder.AddColumn<string>(
                name: "device_name",
                table: "user_token",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ip_address",
                table: "user_token",
                type: "varchar(45)",
                maxLength: 45,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_active",
                table: "user_token",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "user_agent",
                table: "user_token",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 8, 17, 13, 9, 45, 172, DateTimeKind.Utc).AddTicks(9277));

            migrationBuilder.CreateIndex(
                name: "ix_user_token_user_id",
                table: "user_token",
                column: "user_id");

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
                name: "fk_user_token_user_user_id",
                table: "user_token");

            migrationBuilder.DropIndex(
                name: "ix_user_token_user_id",
                table: "user_token");

            migrationBuilder.DropColumn(
                name: "device_name",
                table: "user_token");

            migrationBuilder.DropColumn(
                name: "ip_address",
                table: "user_token");

            migrationBuilder.DropColumn(
                name: "last_active",
                table: "user_token");

            migrationBuilder.DropColumn(
                name: "user_agent",
                table: "user_token");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 8, 12, 23, 5, 0, 982, DateTimeKind.Utc).AddTicks(828));

            migrationBuilder.CreateIndex(
                name: "ix_user_token_user_id",
                table: "user_token",
                column: "user_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_user_token_user_user_id",
                table: "user_token",
                column: "user_id",
                principalTable: "user",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
