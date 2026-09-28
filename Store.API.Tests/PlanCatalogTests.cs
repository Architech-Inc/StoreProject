using Store.Models.Billing;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// MT-02 — feature / limit matrix tests for the plan catalog.
/// </summary>
public class PlanCatalogTests
{
    [Theory]
    [InlineData(TenantTier.Starter, false)]
    [InlineData(TenantTier.Professional, true)]
    [InlineData(TenantTier.Enterprise, true)]
    public void AutomatedBackups_only_for_paid_tiers(TenantTier tier, bool expected)
    {
        Assert.Equal(expected, PlanCatalog.IsFeatureEnabled(tier, PlanFeature.AutomatedBackups));
    }

    [Theory]
    [InlineData(TenantTier.Starter, false)]
    [InlineData(TenantTier.Professional, false)]
    [InlineData(TenantTier.Enterprise, true)]
    public void CustomSmtp_enterprise_only(TenantTier tier, bool expected)
    {
        Assert.Equal(expected, PlanCatalog.IsFeatureEnabled(tier, PlanFeature.CustomSmtp));
    }

    [Theory]
    [InlineData(TenantTier.Starter, false)]
    [InlineData(TenantTier.Professional, false)]
    [InlineData(TenantTier.Enterprise, true)]
    public void SandboxEnvironments_enterprise_only(TenantTier tier, bool expected)
    {
        Assert.Equal(expected, PlanCatalog.IsFeatureEnabled(tier, PlanFeature.SandboxEnvironments));
    }

    [Theory]
    [InlineData(TenantTier.Starter)]
    [InlineData(TenantTier.Professional)]
    [InlineData(TenantTier.Enterprise)]
    public void Every_tier_has_ExternalApiAccess(TenantTier tier)
    {
        Assert.True(PlanCatalog.IsFeatureEnabled(tier, PlanFeature.ExternalApiAccess));
    }

    [Fact]
    public void Branch_limits_are_monotonic()
    {
        var starter = PlanCatalog.GetLimits(TenantTier.Starter).MaxBranches;
        var professional = PlanCatalog.GetLimits(TenantTier.Professional).MaxBranches;
        var enterprise = PlanCatalog.GetLimits(TenantTier.Enterprise).MaxBranches;
        Assert.True(starter < professional);
        Assert.True(professional < enterprise);
    }

    [Fact]
    public void User_seat_limits_are_monotonic()
    {
        var starter = PlanCatalog.GetLimits(TenantTier.Starter).MaxUsers;
        var professional = PlanCatalog.GetLimits(TenantTier.Professional).MaxUsers;
        var enterprise = PlanCatalog.GetLimits(TenantTier.Enterprise).MaxUsers;
        Assert.True(starter < professional);
        Assert.True(professional < enterprise);
    }

    [Fact]
    public void Enterprise_has_unlimited_invoices()
    {
        Assert.Equal(int.MaxValue, PlanCatalog.GetLimits(TenantTier.Enterprise).MonthlyInvoices);
    }

    [Fact]
    public void Starter_invoice_quota_is_at_most_professional()
    {
        var starter = PlanCatalog.GetLimits(TenantTier.Starter).MonthlyInvoices;
        var professional = PlanCatalog.GetLimits(TenantTier.Professional).MonthlyInvoices;
        Assert.True(starter <= professional);
    }

    [Fact]
    public void GetFeatures_returns_unique_set()
    {
        var features = PlanCatalog.GetFeatures(TenantTier.Enterprise);
        Assert.Equal(features.Count, features.Distinct().Count());
    }

    [Fact]
    public void GetFeatures_returns_empty_for_unknown_tier_value()
    {
        // Cast a value outside the enum to force the catalog to skip.
        var features = PlanCatalog.GetFeatures((TenantTier)999);
        Assert.Empty(features);
    }

    [Theory]
    [InlineData("starter", TenantTier.Starter)]
    [InlineData("professional", TenantTier.Professional)]
    [InlineData("enterprise", TenantTier.Enterprise)]
    [InlineData("STARTER", TenantTier.Starter)]
    [InlineData("Professional", TenantTier.Professional)]
    public void FromPlanId_recognizes_known_ids_case_insensitively(string planId, TenantTier expected)
    {
        Assert.Equal(expected, PlanCatalog.FromPlanId(planId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("platinum")]
    [InlineData("pro")]
    public void FromPlanId_returns_null_for_unknown(string? planId)
    {
        Assert.Null(PlanCatalog.FromPlanId(planId));
    }

    [Fact]
    public void ComputeNextBillingAt_adds_30_days()
    {
        var paid = new DateTime(2026, 9, 17, 10, 30, 0, DateTimeKind.Utc);
        var next = PlanCatalog.ComputeNextBillingAtUtc(paid);
        Assert.Equal(paid.AddDays(30), next);
    }

    [Fact]
    public void GracePeriod_is_strictly_positive()
    {
        Assert.True(PlanCatalog.GetLimits(TenantTier.Starter).GracePeriodDays > 0);
        Assert.True(PlanCatalog.GetLimits(TenantTier.Professional).GracePeriodDays > 0);
        Assert.True(PlanCatalog.GetLimits(TenantTier.Enterprise).GracePeriodDays > 0);
    }

    [Fact]
    public void GracePeriod_is_monotonic()
    {
        var starter = PlanCatalog.GetLimits(TenantTier.Starter).GracePeriodDays;
        var professional = PlanCatalog.GetLimits(TenantTier.Professional).GracePeriodDays;
        var enterprise = PlanCatalog.GetLimits(TenantTier.Enterprise).GracePeriodDays;
        Assert.True(starter <= professional);
        Assert.True(professional <= enterprise);
    }
}