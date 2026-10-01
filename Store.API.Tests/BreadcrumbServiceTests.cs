using StoreUI.Services;
using Xunit;

namespace Store.API.Tests;

public class BreadcrumbServiceTests
{
    private readonly BreadcrumbService _service = new();

    [Theory]
    [InlineData("/")]
    [InlineData("/Dashboard")]
    [InlineData("/dashboard?tab=overview")]
    public void BuildBreadcrumbs_ReturnsSingleActiveCrumb_ForDashboardOrRoot(string path)
    {
        var crumbs = _service.BuildBreadcrumbs(path);

        Assert.Single(crumbs);
        Assert.Equal("Dashboard", crumbs[0].Label);
        Assert.Equal("/Dashboard", crumbs[0].Url);
        Assert.True(crumbs[0].IsActive);
    }

    [Fact]
    public void BuildBreadcrumbs_BuildsHierarchicalTrail_ForNestedPage()
    {
        var crumbs = _service.BuildBreadcrumbs("/ContactRequests");

        Assert.Equal(3, crumbs.Count);
        Assert.Equal("Home", crumbs[0].Label);
        Assert.Equal("/Dashboard", crumbs[0].Url);
        Assert.False(crumbs[0].IsActive);

        Assert.Equal("User Accounts", crumbs[1].Label);
        Assert.Equal("/Users", crumbs[1].Url);
        Assert.False(crumbs[1].IsActive);

        Assert.Equal("Contact Requests", crumbs[2].Label);
        Assert.Equal("/ContactRequests", crumbs[2].Url);
        Assert.True(crumbs[2].IsActive);
    }

    [Fact]
    public void BuildBreadcrumbs_UsesPageTitle_ForActiveLeafWhenSupplied()
    {
        var crumbs = _service.BuildBreadcrumbs("/Catalog", pageTitle: "Inventory & Items Catalog");

        Assert.Equal(2, crumbs.Count);
        Assert.Equal("Home", crumbs[0].Label);
        Assert.Equal("Inventory & Items Catalog", crumbs[1].Label);
        Assert.True(crumbs[1].IsActive);
    }

    [Fact]
    public void BuildBreadcrumbs_HonorsCustomCrumbs_WhenProvided()
    {
        var custom = new List<BreadcrumbItem>
        {
            new("Root", "/"),
            new("Custom Section", "/custom"),
            new("Item #101", "/custom/101", IsActive: true)
        };

        var crumbs = _service.BuildBreadcrumbs("/custom/101", customCrumbs: custom);

        Assert.Equal(3, crumbs.Count);
        Assert.Equal("Root", crumbs[0].Label);
        Assert.Equal("Item #101", crumbs[2].Label);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Dashboard")]
    public void GetBackLink_ReturnsNull_ForDashboardOrRoot(string path)
    {
        var back = _service.GetBackLink(path);
        Assert.Null(back);
    }

    [Fact]
    public void GetBackLink_ReturnsParentFromRoute_WhenNoReferer()
    {
        var back = _service.GetBackLink("/ContactRequests");

        Assert.NotNull(back);
        Assert.Equal("Back to User Accounts", back.Label);
        Assert.Equal("/Users", back.Url);
    }

    [Fact]
    public void GetBackLink_UsesRefererRoute_WhenValidInternalRefererProvided()
    {
        var back = _service.GetBackLink("/Catalog", refererUrl: "https://localhost:7112/PurchaseOrders?filter=pending");

        Assert.NotNull(back);
        Assert.Equal("Back to Purchase Orders", back.Label);
        Assert.Equal("/PurchaseOrders", back.Url);
    }

    [Fact]
    public void GetBackLink_HonorsCustomBackLink_WhenProvided()
    {
        var custom = new BreadcrumbItem("Back to Sales Ops", "/Operations");

        var back = _service.GetBackLink("/Invoices", customBack: custom);

        Assert.NotNull(back);
        Assert.Equal("Back to Sales Ops", back.Label);
        Assert.Equal("/Operations", back.Url);
    }
}
