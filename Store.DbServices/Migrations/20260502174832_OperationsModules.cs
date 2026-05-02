using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class OperationsModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "tax_profile_id",
                table: "item",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "bundle_rule",
                columns: table => new
                {
                    bundle_rule_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    trigger_item_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    reward_item_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    trigger_quantity = table.Column<int>(type: "int", nullable: false),
                    reward_quantity = table.Column<int>(type: "int", nullable: false),
                    reward_discount_percent = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    valid_from = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    valid_to = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bundle_rule", x => x.bundle_rule_id);
                    table.ForeignKey(
                        name: "fk_bundle_rule_items_reward_item_id",
                        column: x => x.reward_item_id,
                        principalTable: "item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bundle_rule_items_trigger_item_id",
                        column: x => x.trigger_item_id,
                        principalTable: "item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cashier_shift",
                columns: table => new
                {
                    cashier_shift_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    opened_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    closed_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    opened_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    closed_at_utc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    opening_float = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    closing_float = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    expected_closing_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    variance_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cashier_shift", x => x.cashier_shift_id);
                    table.ForeignKey(
                        name: "fk_cashier_shift_users_closed_by_user_id",
                        column: x => x.closed_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cashier_shift_users_opened_by_user_id",
                        column: x => x.opened_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customer_segment_price",
                columns: table => new
                {
                    customer_segment_price_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    item_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    segment = table.Column<int>(type: "int", nullable: false),
                    price_override = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    valid_from = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    valid_to = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_segment_price", x => x.customer_segment_price_id);
                    table.ForeignKey(
                        name: "fk_customer_segment_price_items_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "role_permission",
                columns: table => new
                {
                    role_permission_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    permission_key = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_allowed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permission", x => x.role_permission_id);
                    table.ForeignKey(
                        name: "fk_role_permission_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "role_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "stock_movement",
                columns: table => new
                {
                    stock_movement_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    item_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    invoice_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    items_order_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    performed_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    movement_type = table.Column<int>(type: "int", nullable: false),
                    quantity_delta = table.Column<int>(type: "int", nullable: false),
                    stock_before = table.Column<int>(type: "int", nullable: false),
                    stock_after = table.Column<int>(type: "int", nullable: false),
                    unit_cost = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    unit_price = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    reason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reference_code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_movement", x => x.stock_movement_id);
                    table.ForeignKey(
                        name: "fk_stock_movement_invoice_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoice",
                        principalColumn: "invoice_id");
                    table.ForeignKey(
                        name: "fk_stock_movement_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_stock_movement_items_order_items_order_id",
                        column: x => x.items_order_id,
                        principalTable: "items_order",
                        principalColumn: "items_order_id");
                    table.ForeignKey(
                        name: "fk_stock_movement_users_performed_by_user_id",
                        column: x => x.performed_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tax_profile",
                columns: table => new
                {
                    tax_profile_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rate_percent = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    application_type = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_profile", x => x.tax_profile_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_item_tax_profile_id",
                table: "item",
                column: "tax_profile_id");

            migrationBuilder.CreateIndex(
                name: "ix_bundle_rule_reward_item_id",
                table: "bundle_rule",
                column: "reward_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_bundle_rule_trigger_item_id",
                table: "bundle_rule",
                column: "trigger_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_cashier_shift_closed_by_user_id",
                table: "cashier_shift",
                column: "closed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_cashier_shift_opened_by_user_id",
                table: "cashier_shift",
                column: "opened_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_segment_price_item_id_segment_is_active",
                table: "customer_segment_price",
                columns: new[] { "item_id", "segment", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_role_permission_role_id_permission_key",
                table: "role_permission",
                columns: new[] { "role_id", "permission_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stock_movement_invoice_id",
                table: "stock_movement",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_movement_item_id_date_created",
                table: "stock_movement",
                columns: new[] { "item_id", "date_created" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_movement_items_order_id",
                table: "stock_movement",
                column: "items_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_movement_performed_by_user_id",
                table: "stock_movement",
                column: "performed_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_item_tax_profiles_tax_profile_id",
                table: "item",
                column: "tax_profile_id",
                principalTable: "tax_profile",
                principalColumn: "tax_profile_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_item_tax_profiles_tax_profile_id",
                table: "item");

            migrationBuilder.DropTable(
                name: "bundle_rule");

            migrationBuilder.DropTable(
                name: "cashier_shift");

            migrationBuilder.DropTable(
                name: "customer_segment_price");

            migrationBuilder.DropTable(
                name: "role_permission");

            migrationBuilder.DropTable(
                name: "stock_movement");

            migrationBuilder.DropTable(
                name: "tax_profile");

            migrationBuilder.DropIndex(
                name: "ix_item_tax_profile_id",
                table: "item");

            migrationBuilder.DropColumn(
                name: "tax_profile_id",
                table: "item");
        }
    }
}
