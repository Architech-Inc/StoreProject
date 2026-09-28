using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <summary>
    /// MT-05 — Tenant-aware audit routing.
    ///
    /// Adds a nullable <c>tenant_id</c> column to <c>audit_log</c> and a
    /// composite index <c>ix_audit_log_tenant_date</c> on
    /// <c>(tenant_id, date_created)</c>. The column is NULL for system-level /
    /// pre-provisioning audits (signup, login before tenant resolution) and
    /// populated for every audit entry generated inside a tenant context.
    ///
    /// Every audit READ must now filter by this column so cross-tenant
    /// audit leakage is impossible at the data layer (not just in the UI).
    /// </summary>
    public partial class AuditLogTenantId_MT05 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "audit_log",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_tenant_date",
                table: "audit_log",
                columns: new[] { "tenant_id", "date_created" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_audit_log_tenant_date",
                table: "audit_log");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "audit_log");
        }
    }
}