using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.DbServices.Context;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <summary>
    /// SEC-23 — WebAuthn enrolled-device binding.
    ///
    /// New <c>trusted_device</c> table holds one row per (user, device).
    /// The fingerprint hash is server-only; the device id is the public
    /// opaque token surfaced to the client. See
    /// <c>Store.Models/Entities/TrustedDevice.cs</c> for the entity shape
    /// and <c>Store.Models/Security/DeviceFingerprint.cs</c> for the
    /// hash derivation.
    /// </summary>
    [DbContext(typeof(StoreDbContext))]
    [Migration("20260925120000_AddTrustedDevices_SEC23")]
    public partial class AddTrustedDevices_SEC23 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trusted_device",
                columns: table => new
                {
                    trusted_device_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "utf8mb4_general_ci"),
                    device_id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fingerprint_hash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    device_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip_address_cidr = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    first_seen_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_seen_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_web_authn_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_revoked = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    is_trusted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    trusted_until_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trusted_device", x => x.trusted_device_id);
                    table.ForeignKey(
                        name: "fk_trusted_device_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // (user_id, fingerprint_hash) — canonical device identity, unique per user.
            migrationBuilder.CreateIndex(
                name: "ix_trusted_device_user_id_fingerprint_hash",
                table: "trusted_device",
                columns: new[] { "user_id", "fingerprint_hash" },
                unique: true);

            // (user_id, device_id) — public device id, unique per user.
            migrationBuilder.CreateIndex(
                name: "ix_trusted_device_user_id_device_id",
                table: "trusted_device",
                columns: new[] { "user_id", "device_id" },
                unique: true);

            // user_id alone — supports the "list user's devices" query.
            migrationBuilder.CreateIndex(
                name: "ix_trusted_device_user_id",
                table: "trusted_device",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "trusted_device");
        }
    }
}
