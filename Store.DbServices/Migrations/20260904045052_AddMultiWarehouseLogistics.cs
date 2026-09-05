using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiWarehouseLogistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_warehouse",
                table: "branch",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "supplying_warehouse_id",
                table: "branch",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "restock_recommendation",
                columns: table => new
                {
                    recommendation_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "utf8mb4_general_ci"),
                    branch_id = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "utf8mb4_general_ci"),
                    recommended_quantity = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    generated_stock_transfer_id = table.Column<int>(type: "int", nullable: true),
                    generated_purchase_order_id = table.Column<int>(type: "int", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restock_recommendation", x => x.recommendation_id);
                    table.ForeignKey(
                        name: "fk_restock_recommendation_branch_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branch",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_restock_recommendation_items_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_restock_recommendation_purchase_orders_generated_purchase_ord~",
                        column: x => x.generated_purchase_order_id,
                        principalTable: "purchase_order",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_restock_recommendation_stock_transfers_generated_stock_transf~",
                        column: x => x.generated_stock_transfer_id,
                        principalTable: "stock_transfer",
                        principalColumn: "stock_transfer_id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 9, 4, 4, 50, 51, 336, DateTimeKind.Utc).AddTicks(8637));

            migrationBuilder.CreateIndex(
                name: "ix_branch_supplying_warehouse_id",
                table: "branch",
                column: "supplying_warehouse_id");

            migrationBuilder.CreateIndex(
                name: "ix_restock_recommendation_branch_id",
                table: "restock_recommendation",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_restock_recommendation_generated_purchase_order_id",
                table: "restock_recommendation",
                column: "generated_purchase_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_restock_recommendation_generated_stock_transfer_id",
                table: "restock_recommendation",
                column: "generated_stock_transfer_id");

            migrationBuilder.CreateIndex(
                name: "ix_restock_recommendation_item_id",
                table: "restock_recommendation",
                column: "item_id");

            migrationBuilder.AddForeignKey(
                name: "fk_branch_branch_supplying_warehouse_id",
                table: "branch",
                column: "supplying_warehouse_id",
                principalTable: "branch",
                principalColumn: "branch_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_branch_branch_supplying_warehouse_id",
                table: "branch");

            migrationBuilder.DropTable(
                name: "restock_recommendation");

            migrationBuilder.DropIndex(
                name: "ix_branch_supplying_warehouse_id",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "is_warehouse",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "supplying_warehouse_id",
                table: "branch");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 9, 4, 4, 3, 16, 932, DateTimeKind.Utc).AddTicks(4086));
        }
    }
}
