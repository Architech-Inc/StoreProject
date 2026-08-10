using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class AddFidoCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fido_credential",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "utf8mb4_general_ci"),
                    credential_id = table.Column<byte[]>(type: "longblob", nullable: false),
                    public_key = table.Column<byte[]>(type: "longblob", nullable: false),
                    user_handle = table.Column<byte[]>(type: "longblob", nullable: false),
                    signature_counter = table.Column<uint>(type: "int unsigned", nullable: false),
                    aa_guid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    cred_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reg_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    date_created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    last_modified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fido_credential", x => x.id);
                    table.ForeignKey(
                        name: "fk_fido_credential_users_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_fido_credential_user_id",
                table: "fido_credential",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fido_credential");
        }
    }
}
