using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.ControlPlane.Migrations
{
    /// <summary>
    /// MT-07 — Tenant status / maintenance page.
    ///
    /// Adds a <c>maintenance_windows</c> JSON column to the <c>tenants</c>
    /// table. The column stores a list of <c>MaintenanceWindow</c> entries
    /// serialized as JSON — operators publish them and the public status
    /// endpoint surfaces them to anonymous visitors.
    /// </summary>
    public partial class TenantMaintenanceWindows_MT07 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "maintenance_windows",
                table: "tenants",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "maintenance_windows",
                table: "tenants");
        }
    }
}