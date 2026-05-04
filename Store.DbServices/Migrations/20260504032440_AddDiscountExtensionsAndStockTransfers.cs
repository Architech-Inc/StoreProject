using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountExtensionsAndStockTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "category_id",
                table: "discount",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "coupon_code",
                table: "discount",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "discount_type",
                table: "discount",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "fixed_amount",
                table: "discount",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "max_uses",
                table: "discount",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "min_quantity",
                table: "discount",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "target_segment",
                table: "discount",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "used_count",
                table: "discount",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "stock_transfer",
                columns: table => new
                {
                    stock_transfer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    from_branch_id = table.Column<int>(type: "int", nullable: false),
                    to_branch_id = table.Column<int>(type: "int", nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    approved_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    dispatched_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    received_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rejection_reason = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    approved_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    dispatched_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    received_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_transfer", x => x.stock_transfer_id);
                    table.ForeignKey(
                        name: "fk_stock_transfer_branch_from_branch_id",
                        column: x => x.from_branch_id,
                        principalTable: "branch",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stock_transfer_branch_to_branch_id",
                        column: x => x.to_branch_id,
                        principalTable: "branch",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stock_transfer_users_requested_by_user_id",
                        column: x => x.requested_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "stock_transfer_item",
                columns: table => new
                {
                    stock_transfer_item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    stock_transfer_id = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    requested_quantity = table.Column<int>(type: "int", nullable: false),
                    dispatched_quantity = table.Column<int>(type: "int", nullable: true),
                    received_quantity = table.Column<int>(type: "int", nullable: true),
                    notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_transfer_item", x => x.stock_transfer_item_id);
                    table.ForeignKey(
                        name: "fk_stock_transfer_item_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stock_transfer_item_stock_transfer_stock_transfer_id",
                        column: x => x.stock_transfer_id,
                        principalTable: "stock_transfer",
                        principalColumn: "stock_transfer_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_discount_category_id",
                table: "discount",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_discount_coupon_code",
                table: "discount",
                column: "coupon_code",
                unique: true,
                filter: "coupon_code IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_discount_is_active_valid_from_valid_to",
                table: "discount",
                columns: new[] { "is_active", "valid_from", "valid_to" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfer_from_branch_id",
                table: "stock_transfer",
                column: "from_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfer_requested_by_user_id",
                table: "stock_transfer",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfer_status_date_created",
                table: "stock_transfer",
                columns: new[] { "status", "date_created" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfer_to_branch_id",
                table: "stock_transfer",
                column: "to_branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfer_item_item_id",
                table: "stock_transfer_item",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_transfer_item_stock_transfer_id",
                table: "stock_transfer_item",
                column: "stock_transfer_id");

            migrationBuilder.AddForeignKey(
                name: "fk_discount_category_category_id",
                table: "discount",
                column: "category_id",
                principalTable: "category",
                principalColumn: "category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_discount_category_category_id",
                table: "discount");

            migrationBuilder.DropTable(
                name: "stock_transfer_item");

            migrationBuilder.DropTable(
                name: "stock_transfer");

            migrationBuilder.DropIndex(
                name: "ix_discount_category_id",
                table: "discount");

            migrationBuilder.DropIndex(
                name: "ix_discount_coupon_code",
                table: "discount");

            migrationBuilder.DropIndex(
                name: "ix_discount_is_active_valid_from_valid_to",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "coupon_code",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "discount_type",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "fixed_amount",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "max_uses",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "min_quantity",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "target_segment",
                table: "discount");

            migrationBuilder.DropColumn(
                name: "used_count",
                table: "discount");
        }
    }
}
