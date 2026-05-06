using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrdersAndCashVariance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cash_variance_record",
                columns: table => new
                {
                    cash_variance_record_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    cashier_shift_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    expected_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    actual_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    reason_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    recorded_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    reviewed_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    review_notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reviewed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cash_variance_record", x => x.cash_variance_record_id);
                    table.ForeignKey(
                        name: "fk_cash_variance_record_cashier_shifts_cashier_shift_id",
                        column: x => x.cashier_shift_id,
                        principalTable: "cashier_shift",
                        principalColumn: "cashier_shift_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cash_variance_record_users_recorded_by_user_id",
                        column: x => x.recorded_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cash_variance_record_users_reviewed_by_user_id",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "purchase_order",
                columns: table => new
                {
                    purchase_order_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    reference_number = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    supplier_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    branch_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<int>(type: "int", nullable: false),
                    expected_delivery_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    notes = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    requested_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    approved_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    received_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_order", x => x.purchase_order_id);
                    table.ForeignKey(
                        name: "fk_purchase_order_branch_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branch",
                        principalColumn: "branch_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_purchase_order_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "supplier",
                        principalColumn: "supplier_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_order_users_approved_by_user_id",
                        column: x => x.approved_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_order_users_requested_by_user_id",
                        column: x => x.requested_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "purchase_order_item",
                columns: table => new
                {
                    purchase_order_item_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    purchase_order_id = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ordered_quantity = table.Column<int>(type: "int", nullable: false),
                    unit_cost = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    received_quantity = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_order_item", x => x.purchase_order_item_id);
                    table.ForeignKey(
                        name: "fk_purchase_order_item_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_purchase_order_item_purchase_order_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_order",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_cash_variance_record_cashier_shift_id",
                table: "cash_variance_record",
                column: "cashier_shift_id");

            migrationBuilder.CreateIndex(
                name: "ix_cash_variance_record_recorded_by_user_id",
                table: "cash_variance_record",
                column: "recorded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_cash_variance_record_reviewed_by_user_id",
                table: "cash_variance_record",
                column: "reviewed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_cash_variance_record_status_date_created",
                table: "cash_variance_record",
                columns: new[] { "status", "date_created" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_approved_by_user_id",
                table: "purchase_order",
                column: "approved_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_branch_id",
                table: "purchase_order",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_reference_number",
                table: "purchase_order",
                column: "reference_number",
                unique: true,
                filter: "reference_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_requested_by_user_id",
                table: "purchase_order",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_status_date_created",
                table: "purchase_order",
                columns: new[] { "status", "date_created" });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_supplier_id",
                table: "purchase_order",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_item_item_id",
                table: "purchase_order_item",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchase_order_item_purchase_order_id",
                table: "purchase_order_item",
                column: "purchase_order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cash_variance_record");

            migrationBuilder.DropTable(
                name: "purchase_order_item");

            migrationBuilder.DropTable(
                name: "purchase_order");
        }
    }
}


