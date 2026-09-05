using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class AddHRPayrollModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employee_contract",
                columns: table => new
                {
                    employee_contract_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "utf8mb4_general_ci"),
                    employee_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "utf8mb4_general_ci"),
                    salary_id = table.Column<int>(type: "int", nullable: true),
                    start_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    payroll_type = table.Column<int>(type: "int", nullable: false),
                    calculate_tax_on_gross = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employee_contract", x => x.employee_contract_id);
                    table.ForeignKey(
                        name: "fk_employee_contract_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_employee_contract_salaries_salary_id",
                        column: x => x.salary_id,
                        principalTable: "salary",
                        principalColumn: "salary_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payroll_run",
                columns: table => new
                {
                    payroll_run_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "utf8mb4_general_ci"),
                    run_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    period_start_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    period_end_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    total_gross = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    total_net = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    total_tax = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    total_allowances = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    approved_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    approved_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payroll_run", x => x.payroll_run_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tax_bracket",
                columns: table => new
                {
                    tax_bracket_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    min_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    max_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    tax_percentage = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    fixed_tax_amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_bracket", x => x.tax_bracket_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payslip",
                columns: table => new
                {
                    payslip_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "utf8mb4_general_ci"),
                    payroll_run_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "utf8mb4_general_ci"),
                    employee_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "utf8mb4_general_ci"),
                    basic_pay = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    allowances = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    gross_pay = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    tax_deducted = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    net_pay = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payslip", x => x.payslip_id);
                    table.ForeignKey(
                        name: "fk_payslip_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employee",
                        principalColumn: "employee_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_payslip_payroll_run_payroll_run_id",
                        column: x => x.payroll_run_id,
                        principalTable: "payroll_run",
                        principalColumn: "payroll_run_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 9, 4, 4, 3, 16, 932, DateTimeKind.Utc).AddTicks(4086));

            migrationBuilder.CreateIndex(
                name: "ix_employee_contract_employee_id",
                table: "employee_contract",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_employee_contract_salary_id",
                table: "employee_contract",
                column: "salary_id");

            migrationBuilder.CreateIndex(
                name: "ix_payslip_employee_id",
                table: "payslip",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_payslip_payroll_run_id",
                table: "payslip",
                column: "payroll_run_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employee_contract");

            migrationBuilder.DropTable(
                name: "payslip");

            migrationBuilder.DropTable(
                name: "tax_bracket");

            migrationBuilder.DropTable(
                name: "payroll_run");

            migrationBuilder.UpdateData(
                table: "system_setting",
                keyColumn: "setting_key",
                keyValue: "Auth:PasswordRecoveryMethod",
                column: "last_modified",
                value: new DateTime(2026, 9, 4, 3, 31, 37, 377, DateTimeKind.Utc).AddTicks(40));
        }
    }
}
