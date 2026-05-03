using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class MobileMoneyTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mobile_money_transaction",
                columns: table => new
                {
                    mobile_money_transaction_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    invoice_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    provider = table.Column<int>(type: "int", nullable: false),
                    phone_number = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    provider_transaction_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    callback_payload = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    completed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mobile_money_transaction", x => x.mobile_money_transaction_id);
                    table.ForeignKey(
                        name: "fk_mobile_money_transaction_invoice_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoice",
                        principalColumn: "invoice_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_mobile_money_transaction_invoice_id",
                table: "mobile_money_transaction",
                column: "invoice_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mobile_money_transaction");
        }
    }
}
