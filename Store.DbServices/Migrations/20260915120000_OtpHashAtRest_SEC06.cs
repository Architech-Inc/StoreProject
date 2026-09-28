using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <summary>
    /// SEC-06 — Hash OTP codes at rest with HMAC-SHA256(pepper, rawCode).
    /// Plaintext `code` column is removed; verification uses <see cref="System.Security.Cryptography.CryptographicOperations.FixedTimeEquals"/>
    /// over the digest stored in `code_hash`. Existing OTPs cannot be migrated
    /// (their plaintext was discarded the moment the service was deployed), so
    /// rows are purged on Up and any user mid-recovery is asked to re-request.
    /// </summary>
    public partial class OtpHashAtRest_SEC06 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop all in-flight OTPs — they were plaintext, can never be hashed,
            //    and 15-minute expiry means very few users are affected. They simply
            //    re-request.
            migrationBuilder.Sql("DELETE FROM otp;");

            // 2. Drop the plaintext column.
            migrationBuilder.DropColumn(
                name: "code",
                table: "otp");

            // 3. Add the HMAC digest column. base64(SHA256) = 44 chars; we use 64 to leave headroom.
            migrationBuilder.AddColumn<string>(
                name: "code_hash",
                table: "otp",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 0);

            // 4. Composite index for the verify-OTP lookup
            //    (WHERE user_id=? AND purpose=? AND is_used=false AND expires_at > now)
            migrationBuilder.Sql(
                "DROP INDEX `ix_otp_user_id` ON otp;");
            migrationBuilder.Sql(
                "CREATE INDEX `ix_otp_user_purpose_used_expires` ON otp (user_id, purpose, is_used, expires_at);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Best-effort revert: drop the composite index, restore the plaintext column.
            migrationBuilder.Sql(
                "DROP INDEX `ix_otp_user_purpose_used_expires` ON otp;");

            migrationBuilder.DropColumn(
                name: "code_hash",
                table: "otp");

            migrationBuilder.AddColumn<string>(
                name: "code",
                table: "otp",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "CREATE INDEX `ix_otp_user_id` ON otp (user_id);");
        }
    }
}
