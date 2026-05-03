using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class LoyaltyModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_loyalty_account",
                columns: table => new
                {
                    loyalty_account_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    customer_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    points = table.Column<int>(type: "int", nullable: false),
                    tier = table.Column<int>(type: "int", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_loyalty_account", x => x.loyalty_account_id);
                    table.ForeignKey(
                        name: "fk_customer_loyalty_account_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "loyalty_transaction",
                columns: table => new
                {
                    loyalty_transaction_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    loyalty_account_id = table.Column<int>(type: "int", nullable: false),
                    invoice_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    points = table.Column<int>(type: "int", nullable: false),
                    transaction_type = table.Column<int>(type: "int", nullable: false),
                    note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loyalty_transaction", x => x.loyalty_transaction_id);
                    table.ForeignKey(
                        name: "fk_loyalty_transaction_customer_loyalty_account_loyalty_account~",
                        column: x => x.loyalty_account_id,
                        principalTable: "customer_loyalty_account",
                        principalColumn: "loyalty_account_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_loyalty_transaction_invoice_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoice",
                        principalColumn: "invoice_id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_customer_loyalty_account_customer_id",
                table: "customer_loyalty_account",
                column: "customer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_transaction_invoice_id",
                table: "loyalty_transaction",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_transaction_loyalty_account_id",
                table: "loyalty_transaction",
                column: "loyalty_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "loyalty_transaction");

            migrationBuilder.DropTable(
                name: "customer_loyalty_account");
        }
    }
}
