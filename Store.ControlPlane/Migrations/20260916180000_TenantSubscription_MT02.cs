using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.ControlPlane.Migrations
{
    /// <summary>
    /// Wave 18 / MT-02 — tenant subscription lifecycle persistence.
    ///
    /// Adds scalar columns tracking plan id, status, billing window, grace
    /// period end, and last payment token. Also adds a JSON `payments`
    /// column for the invoice history surfaced on the Billing page.
    /// </summary>
    public partial class TenantSubscription_MT02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "subscription_plan_id",
                table: "tenants",
                type: "varchar(64)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "subscription_status",
                table: "tenants",
                type: "int",
                nullable: false,
                defaultValue: 0); // SubscriptionStatus.Active

            migrationBuilder.AddColumn<DateTime>(
                name: "subscription_start_utc",
                table: "tenants",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "subscription_end_utc",
                table: "tenants",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_billing_at_utc",
                table: "tenants",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "grace_period_until_utc",
                table: "tenants",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_payment_token",
                table: "tenants",
                type: "varchar(128)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payments",
                table: "tenants",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "payments", table: "tenants");
            migrationBuilder.DropColumn(name: "last_payment_token", table: "tenants");
            migrationBuilder.DropColumn(name: "grace_period_until_utc", table: "tenants");
            migrationBuilder.DropColumn(name: "next_billing_at_utc", table: "tenants");
            migrationBuilder.DropColumn(name: "subscription_end_utc", table: "tenants");
            migrationBuilder.DropColumn(name: "subscription_start_utc", table: "tenants");
            migrationBuilder.DropColumn(name: "subscription_status", table: "tenants");
            migrationBuilder.DropColumn(name: "subscription_plan_id", table: "tenants");
        }
    }
}