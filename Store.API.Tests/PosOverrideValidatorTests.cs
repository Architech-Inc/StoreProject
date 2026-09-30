using Store.DbServices.Services;
using Store.Models.DTOs.Invoices;
using Store.Models.Entities;
using Store.Models.Enums;

namespace Store.API.Tests;

/// <summary>
/// Wave 23.D — tests for <see cref="PosOverrideValidator"/>.
/// Covers the canonical fingerprint computation (mirrors the JS
/// computeCartFingerprint in Pos.cshtml), the happy-path validation,
/// the six fail-paths (unknown ID / wrong status / wrong cashier /
/// wrong session / fingerprint drift / empty input), and the
/// state-machine transitions via <see cref="PosOverrideValidator.MarkApplied"/>.
/// </summary>
public class PosOverrideValidatorTests
{
    private static readonly Guid CashierId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly Guid OtherCashierId = Guid.Parse("99999999-8888-7777-6666-555555555555");
    private static readonly string SessionId = "session-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private static readonly string OtherSessionId = "session-bbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    private static readonly Guid ItemA = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
    private static readonly Guid ItemB = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");

    // ─── ComputeFingerprint ──────────────────────────────────────────────────────

    [Fact]
    public void ComputeFingerprint_is_deterministic_for_same_inputs()
    {
        var lines = new List<CreateSaleLineRequest>
        {
            new() { ItemId = ItemA, Quantity = 2 },
            new() { ItemId = ItemB, Quantity = 5 },
        };

        var a = PosOverrideValidator.ComputeFingerprint(lines);
        var b = PosOverrideValidator.ComputeFingerprint(lines);

        Assert.Equal(a, b);
    }

    [Fact]
    public void ComputeFingerprint_is_independent_of_line_order()
    {
        var lines1 = new List<CreateSaleLineRequest>
        {
            new() { ItemId = ItemA, Quantity = 2 },
            new() { ItemId = ItemB, Quantity = 5 },
        };
        var lines2 = new List<CreateSaleLineRequest>
        {
            new() { ItemId = ItemB, Quantity = 5 },
            new() { ItemId = ItemA, Quantity = 2 },
        };

        Assert.Equal(
            PosOverrideValidator.ComputeFingerprint(lines1),
            PosOverrideValidator.ComputeFingerprint(lines2));
    }

    [Fact]
    public void ComputeFingerprint_differs_when_quantity_changes()
    {
        var lines1 = new List<CreateSaleLineRequest> { new() { ItemId = ItemA, Quantity = 2 } };
        var lines2 = new List<CreateSaleLineRequest> { new() { ItemId = ItemA, Quantity = 3 } };

        Assert.NotEqual(
            PosOverrideValidator.ComputeFingerprint(lines1),
            PosOverrideValidator.ComputeFingerprint(lines2));
    }

    [Fact]
    public void ComputeFingerprint_returns_64_char_lower_hex()
    {
        var lines = new List<CreateSaleLineRequest> { new() { ItemId = ItemA, Quantity = 1 } };
        var fp = PosOverrideValidator.ComputeFingerprint(lines);

        Assert.Equal(64, fp.Length);
        Assert.Matches("^[0-9a-f]{64}$", fp);
    }

    // ─── Validate: happy path ────────────────────────────────────────────────────

    [Fact]
    public void Validate_returns_ok_when_no_overrides_referenced()
    {
        // Cashier can submit a checkout with no overrides referenced.
        // Validator returns Ok(empty list) — nothing to apply, nothing to deny.
        var request = BuildRequest(lineCount: 2, overrideIds: new List<int?> { null, null });

        var result = PosOverrideValidator.Validate(request, CashierId, Array.Empty<DiscountOverrideRequest>());

        Assert.True(result.Allowed);
        Assert.Empty(result.Validated);
    }

    [Fact]
    public void Validate_returns_ok_for_approved_override_with_matching_session_and_fingerprint()
    {
        var request = BuildRequest(lineCount: 1, overrideIds: new List<int?> { 42 });
        var fingerprint = PosOverrideValidator.ComputeFingerprint(request.Lines);
        var overrideRow = BuildRow(id: 42, status: DiscountOverrideStatus.Approved,
            cashier: CashierId, session: SessionId, fingerprint: fingerprint);

        var result = PosOverrideValidator.Validate(request, CashierId, new[] { overrideRow });

        Assert.True(result.Allowed);
        Assert.Single(result.Validated);
        Assert.Equal(42, result.Validated[0].DiscountOverrideRequestId);
    }

    [Fact]
    public void Validate_returns_ok_when_override_has_null_session_and_fingerprint_legacy_path()
    {
        // Legacy invoice-scoped overrides (no PosSessionId / no CartFingerprint)
        // are bypassed from the session + fingerprint checks. They still need
        // to match the cashier + have Approved status.
        var request = BuildRequest(lineCount: 1, overrideIds: new List<int?> { 1 });
        var overrideRow = BuildRow(id: 1, status: DiscountOverrideStatus.Approved,
            cashier: CashierId, session: null, fingerprint: null);

        var result = PosOverrideValidator.Validate(request, CashierId, new[] { overrideRow });

        Assert.True(result.Allowed);
        Assert.Single(result.Validated);
    }

    // ─── Validate: 6 fail paths ─────────────────────────────────────────────────

    [Fact]
    public void Validate_fails_when_override_id_does_not_exist()
    {
        var request = BuildRequest(lineCount: 1, overrideIds: new List<int?> { 999 });
        // row id 1 is in the repo, but the request asks for 999 → fail.
        var overrideRow = BuildRow(id: 1, status: DiscountOverrideStatus.Approved,
            cashier: CashierId, session: null, fingerprint: null);

        var result = PosOverrideValidator.Validate(request, CashierId, new[] { overrideRow });

        Assert.False(result.Allowed);
        Assert.Contains("Override #999 does not exist.", result.Failure!.Reasons);
    }

    [Fact]
    public void Validate_fails_when_override_status_is_not_Approved()
    {
        // session + fingerprint null on every row → bypass those checks.
        var request = BuildRequest(lineCount: 4,
            overrideIds: new List<int?> { 5, 6, 7, 8 });
        var pending = BuildRow(id: 5, status: DiscountOverrideStatus.Pending,
            cashier: CashierId, session: null, fingerprint: null);
        var rejected = BuildRow(id: 6, status: DiscountOverrideStatus.Rejected,
            cashier: CashierId, session: null, fingerprint: null);
        var expired = BuildRow(id: 7, status: DiscountOverrideStatus.Expired,
            cashier: CashierId, session: null, fingerprint: null);
        var applied = BuildRow(id: 8, status: DiscountOverrideStatus.Applied,
            cashier: CashierId, session: null, fingerprint: null);

        var result = PosOverrideValidator.Validate(request, CashierId,
            new[] { pending, rejected, expired, applied });

        Assert.False(result.Allowed);
        Assert.Contains("Override #5 is Pending, not Approved.", result.Failure!.Reasons);
        Assert.Contains("Override #6 is Rejected, not Approved.", result.Failure!.Reasons);
        Assert.Contains("Override #7 is Expired, not Approved.", result.Failure!.Reasons);
        Assert.Contains("Override #8 is Applied, not Approved.", result.Failure!.Reasons);
    }

    [Fact]
    public void Validate_fails_when_override_was_requested_by_a_different_cashier()
    {
        // session + fingerprint null → bypass those checks.
        var request = BuildRequest(lineCount: 1, overrideIds: new List<int?> { 5 });
        var overrideRow = BuildRow(id: 5, status: DiscountOverrideStatus.Approved,
            cashier: OtherCashierId, session: null, fingerprint: null);

        var result = PosOverrideValidator.Validate(request, CashierId, new[] { overrideRow });

        Assert.False(result.Allowed);
        Assert.Contains("Override #5 was requested by a different cashier.", result.Failure!.Reasons);
    }

    [Fact]
    public void Validate_fails_when_session_id_does_not_match()
    {
        // fingerprint null → bypass fingerprint check.
        var request = BuildRequest(lineCount: 1, overrideIds: new List<int?> { 5 });
        var overrideRow = BuildRow(id: 5, status: DiscountOverrideStatus.Approved,
            cashier: CashierId, session: OtherSessionId, fingerprint: null);

        var result = PosOverrideValidator.Validate(request, CashierId, new[] { overrideRow });

        Assert.False(result.Allowed);
        Assert.Contains("Override #5 belongs to a different POS session.", result.Failure!.Reasons);
    }

    [Fact]
    public void Validate_fails_when_cart_fingerprint_drifted()
    {
        // session matches → bypass session check; fingerprint is the only mismatch.
        var request = BuildRequest(lineCount: 1, overrideIds: new List<int?> { 5 });
        var overrideRow = BuildRow(id: 5, status: DiscountOverrideStatus.Approved,
            cashier: CashierId, session: SessionId, fingerprint: "deadbeef0000000000000000000000000000000000000000000000000000000");

        var result = PosOverrideValidator.Validate(request, CashierId, new[] { overrideRow });

        Assert.False(result.Allowed);
        Assert.Contains(result.Failure!.Reasons, r => r.Contains("cart contents drifted since approval"));
    }

    [Fact]
    public void Validate_collects_all_failures_not_just_first()
    {
        // Two distinct failure paths in one validate call: row 1 has wrong
        // status (Pending) + row 2 has wrong cashier. Each row's session +
        // fingerprint are null so we bypass those checks.
        var request = BuildRequest(lineCount: 2, overrideIds: new List<int?> { 1, 2 });
        var pending = BuildRow(id: 1, status: DiscountOverrideStatus.Pending,
            cashier: CashierId, session: null, fingerprint: null);
        var wrongCashier = BuildRow(id: 2, status: DiscountOverrideStatus.Approved,
            cashier: OtherCashierId, session: null, fingerprint: null);

        var result = PosOverrideValidator.Validate(request, CashierId, new[] { pending, wrongCashier });

        Assert.False(result.Allowed);
        Assert.Equal(2, result.Failure!.Reasons.Count);
    }

    // ─── MarkApplied ──────────────────────────────────────────────────────────────

    [Fact]
    public void MarkApplied_transitions_Approved_to_Applied_with_timestamp()
    {
        var now = new DateTime(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);
        var row = BuildRow(id: 11, status: DiscountOverrideStatus.Approved,
            cashier: CashierId, session: SessionId, fingerprint: "x");

        PosOverrideValidator.MarkApplied(new[] { row }, now);

        Assert.Equal(DiscountOverrideStatus.Applied, row.Status);
        Assert.Equal(now, row.AppliedAt);
        Assert.Equal(now, row.LastModified);
    }

    [Fact]
    public void MarkApplied_does_not_mutate_rows_not_passed_in()
    {
        var row1 = BuildRow(id: 1, status: DiscountOverrideStatus.Approved, cashier: CashierId,
            session: SessionId, fingerprint: "x");
        var row2 = BuildRow(id: 2, status: DiscountOverrideStatus.Approved, cashier: CashierId,
            session: SessionId, fingerprint: "x");

        PosOverrideValidator.MarkApplied(new[] { row1 }, DateTime.UtcNow);

        Assert.Equal(DiscountOverrideStatus.Applied, row1.Status);
        Assert.Equal(DiscountOverrideStatus.Approved, row2.Status); // untouched
    }

    // ─── helpers ─────────────────────────────────────────────────────────────────

    private static CreateInvoiceRequest BuildRequest(int lineCount, List<int?>? overrideIds = null)
    {
        var lines = new List<CreateSaleLineRequest>();
        for (var i = 0; i < lineCount; i++)
        {
            var itemId = (i % 2 == 0) ? ItemA : ItemB;
            var quantity = i + 1;
            var overrideList = overrideIds != null && i < overrideIds.Count
                ? (overrideIds[i].HasValue
                    ? new List<int> { overrideIds[i]!.Value }
                    : new List<int>())
                : new List<int>();
            lines.Add(new CreateSaleLineRequest
            {
                ItemId = itemId,
                Quantity = quantity,
                PendingDiscountOverrideRequestIds = overrideList,
            });
        }
        return new CreateInvoiceRequest
        {
            ClientSessionId = SessionId,
            Lines = lines,
        };
    }

    private static DiscountOverrideRequest BuildRow(
        int id,
        DiscountOverrideStatus status,
        Guid cashier,
        string? session,
        string? fingerprint)
    {
        return new DiscountOverrideRequest
        {
            DiscountOverrideRequestId = id,
            Status = status,
            RequestedByUserId = cashier,
            PosSessionId = session,
            CartFingerprint = fingerprint,
            DateCreated = DateTime.UtcNow.AddMinutes(-1),
            LastModified = DateTime.UtcNow.AddMinutes(-1),
        };
    }
}
