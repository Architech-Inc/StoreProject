using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceBranchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "branch_id",
                table: "invoice",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoice_branch_id",
                table: "invoice",
                column: "branch_id");

            migrationBuilder.AddForeignKey(
                name: "fk_invoice_branch_branch_id",
                table: "invoice",
                column: "branch_id",
                principalTable: "branch",
                principalColumn: "branch_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_invoice_branch_branch_id",
                table: "invoice");

            migrationBuilder.DropIndex(
                name: "ix_invoice_branch_id",
                table: "invoice");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "invoice");
        }
    }
}
