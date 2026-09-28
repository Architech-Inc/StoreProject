namespace Store.Models.DTOs.Common;

/// <summary>
/// Canonical error codes returned by <c>ApiErrorResponse.Code</c>. UI clients
/// use these to render category-appropriate toasts:
/// <list type="bullet">
///   <item><c>ValidationFailed</c> — yellow info toast ("please check the form")</item>
///   <item><c>NotFound</c> — grey toast ("the item no longer exists")</item>
///   <item><c>Unauthorized</c> / <c>Forbidden</c> — red toast (security-relevant)</item>
///   <item><c>Conflict</c> — orange toast (state conflict, e.g. already actioned)</item>
///   <item><c>RateLimited</c> — yellow toast ("slow down")</item>
///   <item><c>External</c> — orange toast ("provider said no — see logs")</item>
///   <item><c>Internal</c> / <c>Database</c> — red toast ("server error")</item>
/// </list>
/// All codes are stable strings — never rename without a UI migration path.
/// </summary>
public static class ErrorCode
{
    // ─── 400 family ────────────────────────────────────────────────
    public const string ValidationFailed = "validation_failed";
    public const string InvalidRequest = "invalid_request";
    public const string BadRequest = "bad_request";
    public const string MissingField = "missing_field";
    public const string InvalidFormat = "invalid_format";
    public const string InvalidRange = "invalid_range";
    public const string Duplicate = "duplicate";

    // ─── 401 / 403 family ──────────────────────────────────────────
    public const string Unauthorized = "unauthorized";
    public const string InvalidCredentials = "invalid_credentials";
    public const string InvalidTwoFactorCode = "invalid_2fa";
    public const string InvalidRefreshToken = "invalid_refresh_token";
    public const string TokenExpired = "token_expired";
    public const string TwoFactorRequired = "two_factor_required";
    public const string Forbidden = "forbidden";
    public const string TwoFactorDisabled = "two_factor_disabled";

    // ─── 404 family ────────────────────────────────────────────────
    public const string NotFound = "not_found";
    public const string AlreadyActioned = "already_actioned";

    // ─── 503 family ────────────────────────────────────────────────
    /// <summary>MT-02 — payment provider (PayDunya / Stripe / etc.) is unreachable or unconfigured.</summary>
    public const string PaymentProviderUnavailable = "payment_provider_unavailable";

    // ─── 402 family ────────────────────────────────────────────────
    /// <summary>Wave 20 — tenant has hit a plan-tier quota. Upgrade to continue.</summary>
    public const string QuotaExceeded = "quota_exceeded";

    // ─── 409 family ────────────────────────────────────────────────
    public const string Conflict = "conflict";
    public const string InsufficientPoints = "insufficient_points";
    public const string InvalidPoints = "invalid_points";

    // ─── 422 family ────────────────────────────────────────────────
    public const string BusinessRule = "business_rule";
    public const string InvalidState = "invalid_state";

    // ─── 429 family ────────────────────────────────────────────────
    public const string RateLimited = "rate_limited";

    // ─── 5xx family ────────────────────────────────────────────────
    public const string External = "external";
    public const string Database = "database";
    public const string UpdateFailed = "update_failed";
    public const string CreateFailed = "create_failed";
    public const string DeleteFailed = "delete_failed";
    public const string Internal = "internal";

    // ─── Domain codes (kept as-is for API consumers) ──────────────
    public const string CouponInvalid = "coupon_invalid";
    public const string CampaignNotFound = "CAMPAIGN_NOT_FOUND";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string InvalidDates = "INVALID_DATES";
    public const string TwoFactorRequiredDomain = "two_factor_required";
    public const string PasswordRecoveryDisabled = "password_recovery_disabled";

    // ─── Audit / identity ──────────────────────────────────────────
    public const string TwoFactorRevokeFailed = "2fa_revoke_failed";
    public const string TwoFactorDisableFailed = "2fa_disable_failed";
    public const string ContactApproveFailed = "contact_approve_failed";
    public const string ContactRejectFailed = "contact_reject_failed";
    public const string ContactCancelFailed = "contact_cancel_failed";
    public const string PasswordIssueDisabled = "password_issue_disabled";

    // ─── UI / UX helper ────────────────────────────────────────────

    /// <summary>
    /// Maps a code to a default category that the UI can switch on:
    /// <c>validation</c>, <c>notfound</c>, <c>unauthorized</c>, <c>forbidden</c>,
    /// <c>conflict</c>, <c>ratelimited</c>, <c>external</c>, <c>internal</c>.
    /// </summary>
    public static string CategoryFor(string code) => code switch
    {
        ErrorCode.ValidationFailed or ErrorCode.InvalidRequest or ErrorCode.BadRequest or ErrorCode.MissingField
            or ErrorCode.InvalidFormat or ErrorCode.InvalidRange or ErrorCode.Duplicate
            or ErrorCode.ValidationError or ErrorCode.InvalidDates
            => "validation",

        ErrorCode.NotFound or ErrorCode.AlreadyActioned or ErrorCode.CampaignNotFound
            => "notfound",

        ErrorCode.Unauthorized or ErrorCode.InvalidCredentials or ErrorCode.InvalidTwoFactorCode
            or ErrorCode.InvalidRefreshToken or ErrorCode.TokenExpired or ErrorCode.TwoFactorRequired
            or ErrorCode.TwoFactorRequiredDomain
            => "unauthorized",

        ErrorCode.Forbidden or ErrorCode.TwoFactorDisabled or ErrorCode.PasswordRecoveryDisabled
            => "forbidden",

        ErrorCode.Conflict or ErrorCode.InsufficientPoints or ErrorCode.InvalidPoints
            or ErrorCode.BusinessRule or ErrorCode.InvalidState
            => "conflict",

        ErrorCode.RateLimited => "ratelimited",
        ErrorCode.External => "external",

        _ => "internal" // database / update_failed / create_failed / delete_failed / coupon_invalid / internal
    };
}
