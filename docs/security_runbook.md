# Security Runbook — StoreProject

> Operator playbook for production incidents. Print this, pin it to your
> incident channel, and rehearse the steps quarterly.

---

## 1. Suspected compromise

**First 5 minutes:**

1. **Stop the bleeding** — rotate the JWT signing key
   (`Jwt__Key`) and `Payments__MoMoCallbackKey`. Both are env vars; rotate by
   redeploying the API with new values.
2. **Invalidate active sessions** — `POST /api/auth/invalidate-all` (Admin
   only; if the attacker has Admin, do step 1 first).
3. **Force password reset for Admin role** — run:
   ```sql
   UPDATE users SET force_password_change = 1 WHERE role_id = <admin-role-id>;
   ```
4. **Block attacker IP** at Traefik:
   ```yaml
   # In traefik dynamic config
   routers:
     store-api:
       middlewares: [blocklist@file]
   middlewares:
     blocklist:
       ipWhiteList:
         sourceRange: ["<known-bad-range>"]
   ```

**Next 30 minutes:**

5. **Export audit log** for the incident window:
   ```
   GET /api/audit-logs?dateFrom=<t0>&dateTo=<t1>&severity=Security
   ```
6. **Pull Mongo audit log** — `StoreLogsDb.audit_logs` is the authoritative
   source for cross-system events; the EF `AuditLogs` table mirrors it.
7. **Compare both** — discrepancies point at log-tampering attempts.

**Same day:**

8. **Force-rotate every user with `force_password_change`** — see
   `IUserService.ForcePasswordResetAsync`.
9. **Notify tenants** — `Store.ControlPlane` can broadcast via the
   `/api/controlplane/tenants/{id}/notify` endpoint.

## 2. Database compromise

1. **Pause writes** — `docker compose pause store-api store-ui store-tenantportal`.
2. **Snapshot the database NOW** — `mysqldump --single-transaction` for MySQL;
   Atlas automatic snapshot for Mongo.
3. **Restore from the last clean backup** — `scripts/restore-database.sh`
   takes a snapshot path and replays to MySQL.
4. **Rotate every secret** that was stored on disk or in env vars.
5. **Audit the Mongo audit log** for `Severity == Critical`.

## 3. Lost / stolen device (cashier or manager)

1. Revoke all sessions for that user: `POST /api/users/{id}/revoke-all-sessions`.
2. Force password reset.
3. If WebAuthn was enrolled, revoke the credentials:
   `DELETE /api/webauthn/credentials/{credId}`.
4. Lock the device out of WebAuthn by clearing the local `localStorage`.

## 4. MoMo callback integrity

**Symptoms:** mismatched transaction totals, double-credits, missing
debits.

1. Confirm HMAC is verified — check the `Payments:MoMoCallbackKey` value
   matches the one configured with the provider (MTN / Orange).
2. Look at the raw callback body in `audit_logs.metadata_json`. Any
   callback missing `X-Callback-Signature` should be rejected at the
   middleware layer.
3. If `audit_logs` shows callbacks with `Threat = "unknown"` — that means
   HMAC was missing, and the request was accepted. Investigate why.

## 5. Rate-limit alarms

- The `strict-anonymous` policy triggers a 429 after 3 attempts / 15 min
  on login. A spike in 429s in `audit_logs` = someone is brute-forcing.
- The `password-recovery` policy triggers 429 after 3 / 15 min. A spike
  here is either a DoS or an enumeration attack.

**Response:**
1. Pull the top offender IPs from the audit log.
2. Add them to the Traefik blocklist.
3. If you have a SIEM, raise a rule on `severity == Security AND
  category == Authentication`.

## 6. Disclosure process

We follow a responsible-disclosure model. Reports go to
security@example.com. Acknowledgement SLA: 24h. Triage SLA: 72h. Fix SLA:
30d for Critical, 90d for High.

## 7. Backup & restore

- Per-tenant MySQL: nightly `mysqldump` + 30-day retention.
  `docker/backup/Dockerfile.mysql-backup` + `mysql-backup.sh`.
- Per-tenant Mongo: daily `mongodump` to a per-tenant S3 bucket.
  `docker/backup/Dockerfile.mongo-backup` + `mongo-backup.sh`.
- **Test restore quarterly** — a backup you haven't restored is a
  backup you don't have.

## 8. Key contacts

| Role | Owner |
|---|---|
| Platform lead | Alain Kimbu |
| ControlPlane on-call | rotates weekly |
| Tenant-portal on-call | rotates weekly |
| Security IR | security@example.com |

---

Last reviewed: 2026-09-13. Update whenever the deployment topology
changes.
