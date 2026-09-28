using Store.DbServices.Services;
using Store.Models.Billing;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 19 — pure-function tests for <see cref="PlanQuotaGate"/>. The gate
/// resolves the active tier from a <see cref="Func{TenantTier?}"/> so we
/// can drive it from tests without spinning up HttpContext.
/// </summary>
public class PlanQuotaGateTests
{
    private static PlanQuotaGate GateAt(TenantTier tier) => new(() => tier);
    private static PlanQuotaGate GateUnknown() => new(() => null);

    [Theory]
    [InlineData(TenantTier.Starter, 1, true)]
    [InlineData(TenantTier.Starter, 0, true)]
    [InlineData(TenantTier.Professional, 5, true)]
    [InlineData(TenantTier.Professional, 4, true)]
    [InlineData(TenantTier.Enterprise, 1000, true)]
    public void Branch_creation_allowed_within_limit(TenantTier tier, int currentBranches, bool expectedAllowed)
    {
        var gate = GateAt(tier);
        var result = gate.CheckBranchCreation(currentBranches);
        Assert.Equal(expectedAllowed, result.Allowed);
    }

    [Theory]
    [InlineData(TenantTier.Starter, 2)]
    [InlineData(TenantTier.Professional, 6)]
    public void Branch_creation_blocked_over_limit(TenantTier tier, int currentBranches)
    {
        var gate = GateAt(tier);
        var result = gate.CheckBranchCreation(currentBranches);
        Assert.False(result.Allowed);
        Assert.Equal("branches", result.Quota);
        Assert.Contains("Upgrade", result.Reason);
    }

    [Theory]
    [InlineData(TenantTier.Starter, 5, true)]
    [InlineData(TenantTier.Starter, 6, false)]
    [InlineData(TenantTier.Professional, 25, true)]
    [InlineData(TenantTier.Professional, 26, false)]
    [InlineData(TenantTier.Enterprise, 500, true)]
    public void User_seat_blocked_over_limit(TenantTier tier, int currentUsers, bool expectedAllowed)
    {
        var gate = GateAt(tier);
        var result = gate.CheckUserSeat(currentUsers);
        Assert.Equal(expectedAllowed, result.Allowed);
        if (!expectedAllowed)
        {
            Assert.Equal("users", result.Quota);
        }
    }

    [Theory]
    [InlineData(TenantTier.Starter, 500, true)]
    [InlineData(TenantTier.Starter, 501, false)]
    [InlineData(TenantTier.Professional, 5000, true)]
    [InlineData(TenantTier.Professional, 5001, false)]
    [InlineData(TenantTier.Enterprise, 1000000, true)]
    public void Monthly_invoice_blocked_over_limit(TenantTier tier, int currentMonth, bool expectedAllowed)
    {
        var gate = GateAt(tier);
        var result = gate.CheckMonthlyInvoice(currentMonth);
        Assert.Equal(expectedAllowed, result.Allowed);
        if (!expectedAllowed)
        {
            Assert.Equal("invoices", result.Quota);
        }
    }

    [Theory]
    [InlineData(TenantTier.Starter, PlanFeature.CustomSmtp, false)]
    [InlineData(TenantTier.Starter, PlanFeature.AutomatedBackups, false)]
    [InlineData(TenantTier.Professional, PlanFeature.AutomatedBackups, true)]
    [InlineData(TenantTier.Professional, PlanFeature.CustomSmtp, false)]
    [InlineData(TenantTier.Enterprise, PlanFeature.CustomSmtp, true)]
    [InlineData(TenantTier.Enterprise, PlanFeature.SandboxEnvironments, true)]
    public void Feature_check_respects_tier(TenantTier tier, PlanFeature feature, bool expectedAllowed)
    {
        var gate = GateAt(tier);
        var result = gate.CheckFeature(feature);
        Assert.Equal(expectedAllowed, result.Allowed);
    }

    [Fact]
    public void Unknown_tier_falls_back_to_starter()
    {
        // No tier resolved (system caller with no JWT) — gate defaults to Starter
        // so quota checks fail safely rather than allowing unlimited.
        var gate = GateUnknown();
        var result = gate.CheckBranchCreation(0);
        Assert.True(result.Allowed);
        var blocked = gate.CheckBranchCreation(5);
        Assert.False(blocked.Allowed);
    }

    [Theory]
    [InlineData(TenantTier.Starter, TenantTier.Professional)]
    [InlineData(TenantTier.Professional, TenantTier.Enterprise)]
    public void Blocked_verdict_recommends_next_tier_up(TenantTier current, TenantTier expected)
    {
        var gate = GateAt(current);
        var limits = PlanCatalog.GetLimits(current);
        var result = gate.CheckBranchCreation(limits.MaxBranches + 1);
        Assert.False(result.Allowed);
        Assert.Equal(expected.ToString(), result.UpgradeTo);
    }

    [Fact]
    public void Enterprise_blocks_nothing()
    {
        var gate = GateAt(TenantTier.Enterprise);
        Assert.True(gate.CheckBranchCreation(int.MaxValue).Allowed);
        Assert.True(gate.CheckUserSeat(int.MaxValue).Allowed);
        Assert.True(gate.CheckMonthlyInvoice(int.MaxValue).Allowed);
    }

    [Fact]
    public void Empty_resolver_returns_starter_safe_default()
    {
        var gate = new PlanQuotaGate();
        Assert.True(gate.GetCurrentTier() == TenantTier.Starter);
        var ok = gate.CheckBranchCreation(1);
        Assert.True(ok.Allowed);
    }
}