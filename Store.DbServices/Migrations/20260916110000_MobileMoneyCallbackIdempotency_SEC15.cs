using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DbServices.Migrations
{
    /// <summary>
    /// SEC-15 — Mobile money callback idempotency.
    ///
    /// Adds a UNIQUE index on <c>provider_transaction_id</c> (filtered: NULL
    /// rows are pre-callback and allowed to coexist). The DB now dedupes
    /// callback retries — two callbacks for the same provider transaction
    /// will produce one UPDATE, the second becomes a no-op via a constraint
    /// violation we catch in the service.
    /// </summary>
    public partial class MobileMoneyCallbackIdempotency_SEC15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Before adding the unique index, drop any duplicate rows that
            // may already exist (a real prod tenant may have legacy dupes
            // from before SEC-15). We keep the OLDEST row per provider
            // transaction id and revoke the rest by setting their status
            // to Failed so they can be inspected by an operator.
            migrationBuilder.Sql(
                "UPDATE mobile_money_transaction tgt " +
                "JOIN ( " +
                "    SELECT provider_transaction_id, MIN(mobile_money_transaction_id) AS keep_id " +
                "    FROM mobile_money_transaction " +
                "    WHERE provider_transaction_id IS NOT NULL " +
                "      AND provider_transaction_id <> '' " +
                "    GROUP BY provider_transaction_id " +
                "    HAVING COUNT(*) > 1 " +
                ") dups ON tgt.provider_transaction_id = dups.provider_transaction_id " +
                "SET tgt.status = 4 /* Failed */, tgt.completed_at_utc = UTC_TIMESTAMP(6), " +
                "    tgt.callback_payload = CONCAT(IFNULL(tgt.callback_payload, ''), " +
                "                                  '\\n--SEC15-cleanup: dup of ', dups.keep_id) " +
                "WHERE tgt.mobile_money_transaction_id <> dups.keep_id;");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX `ux_mobile_money_provider_tx_id` " +
                "ON mobile_money_transaction (provider_transaction_id) " +
                "WHERE provider_transaction_id IS NOT NULL AND provider_transaction_id <> '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX `ux_mobile_money_provider_tx_id` ON mobile_money_transaction;");
        }
    }
}
