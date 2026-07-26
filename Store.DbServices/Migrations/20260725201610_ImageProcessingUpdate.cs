using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <inheritdoc />
    public partial class ImageProcessingUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "user",
                newName: "thumbnail_url");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "supplier",
                newName: "thumbnail_url");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "manufacturer",
                newName: "thumbnail_url");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "item",
                newName: "thumbnail_url");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "employee",
                newName: "thumbnail_url");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "customer",
                newName: "thumbnail_url");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "category",
                newName: "thumbnail_url");

            migrationBuilder.AddColumn<string>(
                name: "full_image_url",
                table: "user",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "full_image_url",
                table: "supplier",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "full_image_url",
                table: "manufacturer",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "full_image_url",
                table: "item",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "full_image_url",
                table: "employee",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "full_image_url",
                table: "customer",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "full_image_url",
                table: "category",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "full_image_url",
                table: "user");

            migrationBuilder.DropColumn(
                name: "full_image_url",
                table: "supplier");

            migrationBuilder.DropColumn(
                name: "full_image_url",
                table: "manufacturer");

            migrationBuilder.DropColumn(
                name: "full_image_url",
                table: "item");

            migrationBuilder.DropColumn(
                name: "full_image_url",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "full_image_url",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "full_image_url",
                table: "category");

            migrationBuilder.RenameColumn(
                name: "thumbnail_url",
                table: "user",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "thumbnail_url",
                table: "supplier",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "thumbnail_url",
                table: "manufacturer",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "thumbnail_url",
                table: "item",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "thumbnail_url",
                table: "employee",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "thumbnail_url",
                table: "customer",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "thumbnail_url",
                table: "category",
                newName: "image_path");
        }
    }
}
