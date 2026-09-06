using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class PruneDeadPrivilegeAndLocationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_location");

            migrationBuilder.DropTable(
                name: "customer_privilege_action");

            migrationBuilder.DropTable(
                name: "employee_location");

            migrationBuilder.DropTable(
                name: "employee_privilege_action");

            migrationBuilder.DropTable(
                name: "user_privilege_action");

            migrationBuilder.DropTable(
                name: "customer_privilege");

            migrationBuilder.DropTable(
                name: "employee_privilege");

            migrationBuilder.DropTable(
                name: "user_privilege");

            migrationBuilder.DropTable(
                name: "privilege");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 9, 6, 18, 35, 38, 100, DateTimeKind.Utc).AddTicks(414));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customer_location",
                columns: table => new
                {
                    customer_location_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    customer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    location_id = table.Column<int>(type: "int", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_primary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_location", x => x.customer_location_id);
                    table.ForeignKey(
                        name: "fk_customer_location_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_customer_location_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "location",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "employee_location",
                columns: table => new
                {
                    employee_location_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    location_id = table.Column<int>(type: "int", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_primary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee_location", x => x.employee_location_id);
                    table.ForeignKey(
                        name: "fk_employee_location_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_employee_location_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "location",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "privilege",
                columns: table => new
                {
                    privilege_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    module = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_privilege", x => x.privilege_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customer_privilege",
                columns: table => new
                {
                    customer_privilege_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    customer_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    privilege_id = table.Column<int>(type: "int", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_privilege", x => x.customer_privilege_id);
                    table.ForeignKey(
                        name: "fk_customer_privilege_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "customer_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_customer_privilege_privileges_privilege_id",
                        column: x => x.privilege_id,
                        principalTable: "privilege",
                        principalColumn: "privilege_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "employee_privilege",
                columns: table => new
                {
                    employee_privilege_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    privilege_id = table.Column<int>(type: "int", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee_privilege", x => x.employee_privilege_id);
                    table.ForeignKey(
                        name: "fk_employee_privilege_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_employee_privilege_privileges_privilege_id",
                        column: x => x.privilege_id,
                        principalTable: "privilege",
                        principalColumn: "privilege_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_privilege",
                columns: table => new
                {
                    user_privilege_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    privilege_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_privilege", x => x.user_privilege_id);
                    table.ForeignKey(
                        name: "fk_user_privilege_privilege_privilege_id",
                        column: x => x.privilege_id,
                        principalTable: "privilege",
                        principalColumn: "privilege_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_privilege_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customer_privilege_action",
                columns: table => new
                {
                    customer_privilege_action_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    customer_privilege_id = table.Column<int>(type: "int", nullable: false),
                    performed_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_privilege_action", x => x.customer_privilege_action_id);
                    table.ForeignKey(
                        name: "fk_customer_privilege_action_customer_privilege_customer_privil~",
                        column: x => x.customer_privilege_id,
                        principalTable: "customer_privilege",
                        principalColumn: "customer_privilege_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_customer_privilege_action_users_performed_by_user_id",
                        column: x => x.performed_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "employee_privilege_action",
                columns: table => new
                {
                    employee_privilege_action_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    employee_privilege_id = table.Column<int>(type: "int", nullable: false),
                    performed_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee_privilege_action", x => x.employee_privilege_action_id);
                    table.ForeignKey(
                        name: "fk_employee_privilege_action_employee_privilege_employee_privil~",
                        column: x => x.employee_privilege_id,
                        principalTable: "employee_privilege",
                        principalColumn: "employee_privilege_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_employee_privilege_action_users_performed_by_user_id",
                        column: x => x.performed_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_privilege_action",
                columns: table => new
                {
                    user_privilege_action_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    performed_by_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_privilege_id = table.Column<int>(type: "int", nullable: false),
                    action = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_privilege_action", x => x.user_privilege_action_id);
                    table.ForeignKey(
                        name: "fk_user_privilege_action_user_performed_by_user_id",
                        column: x => x.performed_by_user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_privilege_action_user_privilege_user_privilege_id",
                        column: x => x.user_privilege_id,
                        principalTable: "user_privilege",
                        principalColumn: "user_privilege_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 9, 6, 7, 56, 25, 3, DateTimeKind.Utc).AddTicks(1687));

            migrationBuilder.CreateIndex(
                name: "ix_customer_location_customer_id",
                table: "customer_location",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_location_location_id",
                table: "customer_location",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_privilege_customer_id",
                table: "customer_privilege",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_privilege_privilege_id",
                table: "customer_privilege",
                column: "privilege_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_privilege_action_customer_privilege_id",
                table: "customer_privilege_action",
                column: "customer_privilege_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_privilege_action_performed_by_user_id",
                table: "customer_privilege_action",
                column: "performed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_location_employee_id",
                table: "employee_location",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_location_location_id",
                table: "employee_location",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_privilege_employee_id",
                table: "employee_privilege",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_privilege_privilege_id",
                table: "employee_privilege",
                column: "privilege_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_privilege_action_employee_privilege_id",
                table: "employee_privilege_action",
                column: "employee_privilege_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_privilege_action_performed_by_user_id",
                table: "employee_privilege_action",
                column: "performed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_privilege_privilege_id",
                table: "user_privilege",
                column: "privilege_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_privilege_user_id",
                table: "user_privilege",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_privilege_action_performed_by_user_id",
                table: "user_privilege_action",
                column: "performed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_privilege_action_user_privilege_id",
                table: "user_privilege_action",
                column: "user_privilege_id");
        }
    }
}
