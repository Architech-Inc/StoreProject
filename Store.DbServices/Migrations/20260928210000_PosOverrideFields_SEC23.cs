using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Store.DbServices.Context;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <summary>
    /// Wave 23.A — POS per-line discount override binding.
    ///
    /// Adds three columns to <c>discount_override_request</c>:
    ///   - <c>pos_session_id</c> — opaque client-side session id tying the
    ///     override to a specific POS checkout attempt (anti-replay).
    ///   - <c>cart_fingerprint</c> — SHA-256 hex of (itemId, qty) pairs
    ///     captured at approval time; verified at checkout (anti-drift).
    ///   - <c>applied_at</c> — when the override transitioned to <c>Applied</c>
    ///     via a successful checkout. Null until that happens.
    ///
    /// Plus an index on <c>(pos_session_id, status)</c> so the checkout
    /// handler can efficiently look up "approved overrides for this session".
    /// </summary>
    [DbContext(typeof(StoreDbContext))]
    [Migration("20260928210000_PosOverrideFields_SEC23")]
    public partial class PosOverrideFields_SEC23 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "pos_session_id",
                table: "discount_override_request",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true,
                collation: "utf8mb4_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "cart_fingerprint",
                table: "discount_override_request",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true,
                collation: "utf8mb4_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "applied_at",
                table: "discount_override_request",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_discount_override_request_pos_session_id_status",
                table: "discount_override_request",
                columns: new[] { "pos_session_id", "status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_discount_override_request_pos_session_id_status",
                table: "discount_override_request");

            migrationBuilder.DropColumn(
                name: "applied_at",
                table: "discount_override_request");

            migrationBuilder.DropColumn(
                name: "cart_fingerprint",
                table: "discount_override_request");

            migrationBuilder.DropColumn(
                name: "pos_session_id",
                table: "discount_override_request");
        }
    }
}
