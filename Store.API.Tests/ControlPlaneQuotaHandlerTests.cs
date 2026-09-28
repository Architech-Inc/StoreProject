using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Store.ControlPlane.Models;
using Store.ControlPlane.Repositories;
using Store.ControlPlane.Services;
using Store.Models.Billing;

namespace Store.API.Tests;

/// <summary>
/// Wave 20 — pure-function tests for <see cref="ControlPlaneQuotaHandler"/>.
/// The handler resolves a tenant + its current usage, then defers to the
/// pure <see cref="PlanQuotaGate"/> for the verdict. Only Branches has a
/// backing count today; Users / MonthlyInvoices are placeholders that
/// always allow (read 0) until a per-tenant counter exists.
/// </summary>
public class ControlPlaneQuotaHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    private static Tenant MakeTenant(TenantTier tier, int branchCount)
    {
        var tenant = new Tenant
        {
            TenantId = TenantId,
            Name = "Acme",
            Slug = "acme",
            AdminEmail = "[email protected]",
            PlanTier = tier,
        };
        for (var i = 0; i < branchCount; i++)
        {
            tenant.Branches.Add(new TenantBranchMapping
            {
                BranchName = $"branch-{i}",
                BranchSlug = $"branch-{i}",
            });
        }
        return tenant;
    }

    private static ControlPlaneQuotaHandler BuildHandler(Tenant? tenant)
    {
        var repo = new Mock<ITenantRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        return new ControlPlaneQuotaHandler(repo.Object, NullLogger<ControlPlaneQuotaHandler>.Instance);
    }

    // ---- Branches (the only quota with a real backing count) ----

    [Theory]
    [InlineData(TenantTier.Starter, 0, true)]   // 0 + 1 = 1 ≤ 1
    [InlineData(TenantTier.Professional, 4, true)]  // 4 + 1 = 5 ≤ 5
    [InlineData(TenantTier.Enterprise, 999, true)]  // Enterprise = unlimited
    public async Task Branch_creation_allowed_when_under_limit(TenantTier tier, int currentBranches, bool expected)
    {
        var handler = BuildHandler(MakeTenant(tier, currentBranches));

        var result = await handler.CheckAsync(TenantId, TenantQuota.Branches, QuotaUsageSource.CurrentPlusOne, CancellationToken.None);

        Assert.Equal(expected, result.Allowed);
        Assert.Equal("branches", result.Quota);
    }

    [Theory]
    [InlineData(TenantTier.Starter, 1)]        // 1 + 1 = 2 > 1
    [InlineData(TenantTier.Professional, 5)]    // 5 + 1 = 6 > 5
    public async Task Branch_creation_blocked_over_limit(TenantTier tier, int currentBranches)
    {
        var handler = BuildHandler(MakeTenant(tier, currentBranches));

        var result = await handler.CheckAsync(TenantId, TenantQuota.Branches, QuotaUsageSource.CurrentPlusOne, CancellationToken.None);

        Assert.False(result.Allowed);
        Assert.Equal("branches", result.Quota);
        Assert.Contains("Upgrade", result.Reason);
        Assert.False(string.IsNullOrEmpty(result.UpgradeTo));
    }

    [Fact]
    public async Task Branch_creation_with_CurrentPlusOne_passes_post_mutation_count_to_gate()
    {
        // CurrentPlusOne means "assume the mutation succeeds": the gate
        // receives the post-mutation count. With 4 existing branches and
        // CurrentPlusOne, the gate sees 5 — which is exactly Professional's
        // MaxBranches, so the request is allowed (boundary).
        var handler = BuildHandler(MakeTenant(TenantTier.Professional, 4));

        var result = await handler.CheckAsync(TenantId, TenantQuota.Branches, QuotaUsageSource.CurrentPlusOne, CancellationToken.None);

        Assert.True(result.Allowed);
        Assert.Equal(5, result.Current);
        Assert.Equal(5, result.Limit);
    }

    // ---- Placeholder quotas (Users / MonthlyInvoices) ----

    [Theory]
    [InlineData(TenantTier.Starter)]
    [InlineData(TenantTier.Professional)]
    [InlineData(TenantTier.Enterprise)]
    public async Task Users_quota_placeholder_always_allows(TenantTier tier)
    {
        var handler = BuildHandler(MakeTenant(tier, branchCount: 0));

        var result = await handler.CheckAsync(TenantId, TenantQuota.Users, QuotaUsageSource.CurrentPlusOne, CancellationToken.None);

        Assert.True(result.Allowed);
        Assert.Equal("users", result.Quota);
    }

    [Theory]
    [InlineData(TenantTier.Starter)]
    [InlineData(TenantTier.Professional)]
    [InlineData(TenantTier.Enterprise)]
    public async Task MonthlyInvoices_quota_placeholder_always_allows(TenantTier tier)
    {
        var handler = BuildHandler(MakeTenant(tier, branchCount: 0));

        var result = await handler.CheckAsync(TenantId, TenantQuota.MonthlyInvoices, QuotaUsageSource.CurrentPlusOne, CancellationToken.None);

        Assert.True(result.Allowed);
        Assert.Equal("invoices", result.Quota);
    }

    // ---- Tenant resolution edge cases ----

    [Fact]
    public async Task Missing_tenant_returns_ok_so_endpoint_returns_404()
    {
        // Tenant not found: the filter must not block (let the endpoint 404).
        var handler = BuildHandler(tenant: null);

        var result = await handler.CheckAsync(TenantId, TenantQuota.Branches, QuotaUsageSource.CurrentPlusOne, CancellationToken.None);

        Assert.True(result.Allowed);
    }

    [Fact]
    public async Task Unknown_quota_returns_ok_defensively()
    {
        // Defensive: an unknown enum value (forward-compat) should not blow
        // up — treat as allowed so the endpoint can decide.
        var handler = BuildHandler(MakeTenant(TenantTier.Starter, 0));

        var result = await handler.CheckAsync(TenantId, (TenantQuota)999, QuotaUsageSource.CurrentPlusOne, CancellationToken.None);

        Assert.True(result.Allowed);
    }
}
