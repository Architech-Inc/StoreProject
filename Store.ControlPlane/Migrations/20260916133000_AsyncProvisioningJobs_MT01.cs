using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.ControlPlane.Migrations
{
    /// <summary>
    /// MT-01 — Tenant provisioning becomes asynchronous.
    /// A new <c>tenant_provisioning_jobs</c> table tracks each request through
    /// Pending → InProgress → Completed/Failed. The <c>TenantProvisioningHostedService</c>
    /// dequeues and runs the existing <c>ProvisionTenantAsync</c> pipeline, then
    /// links the resulting tenant to the originating portal account.
    /// </summary>
    public partial class AsyncProvisioningJobs_MT01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant_provisioning_jobs",
                columns: table => new
                {
                    job_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    account_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    store_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    admin_email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    admin_username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    admin_password_cipher = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    currency = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false, defaultValue: "XAF"),
                    plan_tier = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    custom_domain = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    status_detail = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    failure_reason = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "UTC_TIMESTAMP(6)"),
                    started_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_provisioning_jobs", x => x.job_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_provisioning_jobs_account_id",
                table: "tenant_provisioning_jobs",
                column: "account_id");

            // The hosted service polls (Status=Pending) ORDER BY DateCreated ASC —
            // this composite index is the hot path.
            migrationBuilder.Sql(
                "CREATE INDEX `ix_provisioning_jobs_status_date` " +
                "ON tenant_provisioning_jobs (status, date_created);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX `ix_provisioning_jobs_status_date` ON tenant_provisioning_jobs;");
            migrationBuilder.DropTable(name: "tenant_provisioning_jobs");
        }
    }
}
