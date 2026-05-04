using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class AddWastageAndDiscountOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "discount_override_request",
                columns: table => new
                {
                    discount_override_request_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    invoice_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    item_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    override_type = table.Column<int>(type: "int", nullable: false),
                    override_value = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    justification = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<int>(type: "int", nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    reviewed_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    review_notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reviewed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discount_override_request", x => x.discount_override_request_id);
                    table.ForeignKey(
                        name: "fk_discount_override_request_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoice",
                        principalColumn: "invoice_id");
                    table.ForeignKey(
                        name: "fk_discount_override_request_items_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "item_id");
                    table.ForeignKey(
                        name: "fk_discount_override_request_users_requested_by_user_id",
                        column: x => x.requested_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_discount_override_request_users_reviewed_by_user_id",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "wastage_entry",
                columns: table => new
                {
                    wastage_entry_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    item_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    wastage_type = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reference_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    recorded_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wastage_entry", x => x.wastage_entry_id);
                    table.ForeignKey(
                        name: "fk_wastage_entry_item_item_id",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_wastage_entry_user_recorded_by_user_id",
                        column: x => x.recorded_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_discount_override_request_invoice_id",
                table: "discount_override_request",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_discount_override_request_item_id",
                table: "discount_override_request",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_discount_override_request_requested_by_user_id",
                table: "discount_override_request",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_discount_override_request_reviewed_by_user_id",
                table: "discount_override_request",
                column: "reviewed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_discount_override_request_status_date_created",
                table: "discount_override_request",
                columns: new[] { "status", "date_created" });

            migrationBuilder.CreateIndex(
                name: "ix_wastage_entry_item_id_date_created",
                table: "wastage_entry",
                columns: new[] { "item_id", "date_created" });

            migrationBuilder.CreateIndex(
                name: "ix_wastage_entry_recorded_by_user_id",
                table: "wastage_entry",
                column: "recorded_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discount_override_request");

            migrationBuilder.DropTable(
                name: "wastage_entry");
        }
    }
}

