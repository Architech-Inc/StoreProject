using System.Security.Cryptography;
using System.Text;
using Store.Models.Billing;
using Store.Models.DTOs.Invoices;
using Store.Models.Entities;
using Store.Models.Enums;

namespace Store.DbServices.Services;

/// <summary>
/// Wave 23.C — server-side validator for POS per-line discount overrides.
///
/// Runs on every checkout that carries <c>PendingDiscountOverrideRequestIds</c>:
///   1. Each ID exists, is <c>Approved</c>, was requested by the calling
///      cashier (anti-cross-user replay).
///   2. Each ID's <c>PosSessionId</c> matches the request's
///      <c>ClientSessionId</c> (anti-replay on a different session).
///   3. Each ID's <c>CartFingerprint</c> matches the fingerprint we
///      recompute from the live cart lines (anti-drift).
///
/// On success, transitions the validated overrides from <c>Approved</c>
/// to <c>Applied</c> (terminal state). Failures return a structured
/// <see cref="PosOverrideValidationResult"/> the caller can serialize
/// to HTTP 422.
/// </summary>
public static class PosOverrideValidator
{
    /// <summary>
    /// Recompute the SHA-256 hex of the sorted (itemId, quantity) pairs
    /// from the request lines. Same algorithm the client uses (mirrors
    /// the JS computeCartFingerprint() in Pos.cshtml).
    /// </summary>
    public static string ComputeFingerprint(IEnumerable<CreateSaleLineRequest> lines)
    {
        var entries = lines
            .Select(l => $"{l.ItemId:N}|{l.Quantity}")
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();
        var joined = string.Join(';', entries);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(joined));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Validate the overrides referenced by the checkout request. Returns
    /// the list of overrides that successfully validated (caller should
    /// transition these to <c>Applied</c>), or a non-empty failure list.
    /// </summary>
    public static PosOverrideValidationResult Validate(
        CreateInvoiceRequest request,
        Guid actingUserId,
        IReadOnlyList<DiscountOverrideRequest> rows)
    {
        var failure = new PosOverrideValidationFailure
        {
            ClientSessionId = request.ClientSessionId,
            CartFingerprint = ComputeFingerprint(request.Lines),
            ActingUserId = actingUserId,
        };

        // Build the set of override IDs the cashier wants to apply.
        var requestedIds = request.Lines
            .Where(l => l.PendingDiscountOverrideRequestIds != null && l.PendingDiscountOverrideRequestIds.Count > 0)
            .SelectMany(l => l.PendingDiscountOverrideRequestIds)
            .Distinct()
            .ToList();

        if (requestedIds.Count == 0)
        {
            return PosOverrideValidationResult.Ok(Array.Empty<DiscountOverrideRequest>());
        }

        var validated = new List<DiscountOverrideRequest>(requestedIds.Count);

        // Look up each requested override by id.
        var byId = rows.ToDictionary(r => r.DiscountOverrideRequestId);
        foreach (var id in requestedIds)
        {
            if (!byId.TryGetValue(id, out var row))
            {
                failure.Reasons.Add($"Override #{id} does not exist.");
                continue;
            }

            if (row.Status != DiscountOverrideStatus.Approved)
            {
                failure.Reasons.Add($"Override #{id} is {row.Status}, not Approved.");
                continue;
            }

            if (row.RequestedByUserId != actingUserId)
            {
                failure.Reasons.Add($"Override #{id} was requested by a different cashier.");
                continue;
            }

            if (!string.IsNullOrEmpty(row.PosSessionId) &&
                !string.Equals(row.PosSessionId, request.ClientSessionId, StringComparison.Ordinal))
            {
                failure.Reasons.Add($"Override #{id} belongs to a different POS session.");
                continue;
            }

            if (!string.IsNullOrEmpty(row.CartFingerprint) &&
                !string.Equals(row.CartFingerprint, failure.CartFingerprint, StringComparison.Ordinal))
            {
                failure.Reasons.Add(
                    $"Override #{id} cart contents drifted since approval (expected {row.CartFingerprint[..8]}…, recomputed {failure.CartFingerprint[..8]}…).");
                continue;
            }

            validated.Add(row);
        }

        return failure.Reasons.Count == 0
            ? PosOverrideValidationResult.Ok(validated)
            : PosOverrideValidationResult.Fail(failure);
    }

    /// <summary>Transition the validated overrides to <c>Applied</c>.</summary>
    public static void MarkApplied(IEnumerable<DiscountOverrideRequest> validated, DateTime appliedAtUtc)
    {
        foreach (var row in validated)
        {
            row.Status = DiscountOverrideStatus.Applied;
            row.AppliedAt = appliedAtUtc;
            row.LastModified = appliedAtUtc;
        }
    }
}

/// <summary>Outcome of <see cref="PosOverrideValidator.Validate"/>.</summary>
public sealed class PosOverrideValidationResult
{
    public bool Allowed { get; init; }
    public IReadOnlyList<DiscountOverrideRequest> Validated { get; init; } = Array.Empty<DiscountOverrideRequest>();
    public PosOverrideValidationFailure? Failure { get; init; }

    public static PosOverrideValidationResult Ok(IReadOnlyList<DiscountOverrideRequest> validated)
        => new() { Allowed = true, Validated = validated };

    public static PosOverrideValidationResult Fail(PosOverrideValidationFailure failure)
        => new() { Allowed = false, Failure = failure };
}



