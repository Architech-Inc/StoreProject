using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class ContactChangeRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contact_change_request",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    new_email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    new_phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    verification_token = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    verified_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    approved_by_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_change_request", x => x.id);
                    table.ForeignKey(
                        name: "fk_contact_change_request_users_approved_by_id",
                        column: x => x.approved_by_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_contact_change_request_users_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 8, 12, 14, 32, 43, 398, DateTimeKind.Utc).AddTicks(5887));

            migrationBuilder.CreateIndex(
                name: "ix_contact_change_request_approved_by_id",
                table: "contact_change_request",
                column: "approved_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_contact_change_request_user_id",
                table: "contact_change_request",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contact_change_request");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 8, 11, 15, 38, 56, 697, DateTimeKind.Utc).AddTicks(8795));
        }
    }
}
