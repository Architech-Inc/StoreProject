using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <summary>
    /// GAP-19 — lookup indexes for scanner and search endpoints that previously
    /// caused full-table scans on growing tenants.
    ///   - Supplier.RegistrationNumber (UNIQUE, NULL-filtered)
    ///   - Batch.BatchNumber (UNIQUE)
    /// </summary>
    public partial class LookupIndexes_GAP19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Supplier.RegistrationNumber — UNIQUE on populated rows so duplicate
            // business-tax-IDs become a hard schema constraint.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX `ix_supplier_registration_number` " +
                "ON supplier (registration_number) " +
                "WHERE registration_number IS NOT NULL AND registration_number <> '';");

            // Batch.BatchNumber — UNIQUE so duplicate batches cannot be inserted.
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX `ix_batch_batch_number` " +
                "ON batch (batch_number);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX `ix_supplier_registration_number` ON supplier;");
            migrationBuilder.Sql("DROP INDEX `ix_batch_batch_number` ON batch;");
        }
    }
}
