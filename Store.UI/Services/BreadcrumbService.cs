using System.Text.RegularExpressions;

namespace StoreUI.Services;

public class BreadcrumbService : IBreadcrumbService
{
    private class RouteInfo
    {
        public string Title { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public string? ParentUrl { get; init; }
        public string? SectionName { get; init; }
    }

    private static readonly Dictionary<string, RouteInfo> Routes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/Dashboard"] = new() { Title = "Dashboard", Url = "/Dashboard" },
        ["/"] = new() { Title = "Dashboard", Url = "/Dashboard" },

        // Sales & Checkout
        ["/Pos"] = new() { Title = "POS Terminal", Url = "/Pos", ParentUrl = "/Dashboard", SectionName = "Sales" },
        ["/Invoices"] = new() { Title = "Invoices & Sales", Url = "/Invoices", ParentUrl = "/Dashboard", SectionName = "Sales" },
        ["/Payments"] = new() { Title = "Payments & Tender", Url = "/Payments", ParentUrl = "/Invoices", SectionName = "Sales" },
        ["/Receipt"] = new() { Title = "Receipt View", Url = "/Receipt", ParentUrl = "/Invoices", SectionName = "Sales" },

        // Inventory & Operations
        ["/Catalog"] = new() { Title = "Catalog & Stock", Url = "/Catalog", ParentUrl = "/Dashboard", SectionName = "Inventory" },
        ["/BatchTracking"] = new() { Title = "Batch Tracking", Url = "/BatchTracking", ParentUrl = "/Catalog", SectionName = "Inventory" },
        ["/InventoryOps"] = new() { Title = "Inventory Operations", Url = "/InventoryOps", ParentUrl = "/Catalog", SectionName = "Inventory" },
        ["/StockTakes"] = new() { Title = "Stock Takes", Url = "/StockTakes", ParentUrl = "/Catalog", SectionName = "Inventory" },

        // Supply Chain
        ["/PurchaseOrders"] = new() { Title = "Purchase Orders", Url = "/PurchaseOrders", ParentUrl = "/Dashboard", SectionName = "Supply Chain" },
        ["/Suppliers"] = new() { Title = "Suppliers Directory", Url = "/Suppliers", ParentUrl = "/Dashboard", SectionName = "Supply Chain" },
        ["/Restock"] = new() { Title = "Restock Alerts", Url = "/Restock", ParentUrl = "/PurchaseOrders", SectionName = "Supply Chain" },
        ["/RestockRecommendations"] = new() { Title = "Restock Alerts", Url = "/RestockRecommendations", ParentUrl = "/PurchaseOrders", SectionName = "Supply Chain" },

        // Pricing & Promotions
        ["/Discounts"] = new() { Title = "Promotions & Discounts", Url = "/Discounts", ParentUrl = "/Dashboard", SectionName = "Pricing" },
        ["/DiscountOverrides"] = new() { Title = "Discount Overrides", Url = "/DiscountOverrides", ParentUrl = "/Discounts", SectionName = "Pricing" },

        // CRM & Loyalty
        ["/Customers"] = new() { Title = "Customer Directory", Url = "/Customers", ParentUrl = "/Dashboard", SectionName = "Customers" },
        ["/Campaigns"] = new() { Title = "Loyalty Campaigns", Url = "/Campaigns", ParentUrl = "/Customers", SectionName = "Customers" },

        // Finance & Cash Operations
        ["/CashManagement"] = new() { Title = "Cash Operations", Url = "/CashManagement", ParentUrl = "/Dashboard", SectionName = "Finance" },
        ["/CashVariance"] = new() { Title = "Cash Variance", Url = "/CashVariance", ParentUrl = "/CashManagement", SectionName = "Finance" },

        // Administration & Governance
        ["/Users"] = new() { Title = "User Accounts", Url = "/Users", ParentUrl = "/Dashboard", SectionName = "Administration" },
        ["/ContactRequests"] = new() { Title = "Contact Requests", Url = "/ContactRequests", ParentUrl = "/Users", SectionName = "Administration" },
        ["/RoleMatrix"] = new() { Title = "Role & Permissions Matrix", Url = "/RoleMatrix", ParentUrl = "/Users", SectionName = "Administration" },
        ["/BranchAdmin"] = new() { Title = "Branch Governance", Url = "/BranchAdmin", ParentUrl = "/Dashboard", SectionName = "Administration" },
        ["/BranchDashboard"] = new() { Title = "Branch Performance", Url = "/BranchDashboard", ParentUrl = "/Dashboard", SectionName = "Performance" },
        ["/SystemSettings"] = new() { Title = "System Settings", Url = "/SystemSettings", ParentUrl = "/Dashboard", SectionName = "Administration" },
        ["/Lookup"] = new() { Title = "Lookup Tables", Url = "/Lookup", ParentUrl = "/Dashboard", SectionName = "Administration" },
        ["/AuditLogs"] = new() { Title = "Security & Audit Logs", Url = "/AuditLogs", ParentUrl = "/Dashboard", SectionName = "Security" },
        ["/Payroll"] = new() { Title = "Payroll & Compensation", Url = "/Payroll", ParentUrl = "/Dashboard", SectionName = "Administration" },

        // Account
        ["/Profile"] = new() { Title = "User Profile", Url = "/Profile", ParentUrl = "/Dashboard", SectionName = "Account" },
        ["/AccessDenied"] = new() { Title = "Access Denied", Url = "/AccessDenied", ParentUrl = "/Dashboard" }
    };

    public IReadOnlyList<BreadcrumbItem> BuildBreadcrumbs(
        string path,
        string? pageTitle = null,
        IReadOnlyList<BreadcrumbItem>? customCrumbs = null)
    {
        if (customCrumbs != null && customCrumbs.Count > 0)
        {
            return customCrumbs;
        }

        var normalizedPath = NormalizePath(path);
        var crumbs = new List<BreadcrumbItem>
        {
            new("Home", "/Dashboard")
        };

        if (string.Equals(normalizedPath, "/Dashboard", StringComparison.OrdinalIgnoreCase) || normalizedPath == "/")
        {
            crumbs[0] = new("Dashboard", "/Dashboard", IsActive: true);
            return crumbs;
        }

        var chain = new List<RouteInfo>();
        var curr = normalizedPath;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (!string.IsNullOrEmpty(curr) && visited.Add(curr))
        {
            if (Routes.TryGetValue(curr, out var info))
            {
                chain.Insert(0, info);
                curr = info.ParentUrl ?? string.Empty;
                if (string.Equals(curr, "/Dashboard", StringComparison.OrdinalIgnoreCase) || curr == "/") break;
            }
            else
            {
                // Fallback for unmapped dynamic route
                var label = !string.IsNullOrWhiteSpace(pageTitle) ? pageTitle : FormatSlug(curr);
                chain.Insert(0, new RouteInfo { Title = label, Url = curr, ParentUrl = "/Dashboard" });
                break;
            }
        }

        foreach (var item in chain)
        {
            var isCurrent = string.Equals(item.Url, normalizedPath, StringComparison.OrdinalIgnoreCase);
            var title = isCurrent && !string.IsNullOrWhiteSpace(pageTitle) ? pageTitle : item.Title;
            crumbs.Add(new BreadcrumbItem(title, item.Url, IsActive: isCurrent));
        }

        return crumbs;
    }

    public BreadcrumbItem? GetBackLink(
        string path,
        string? refererUrl = null,
        BreadcrumbItem? customBack = null)
    {
        if (customBack != null) return customBack;

        var normalizedPath = NormalizePath(path);
        if (string.Equals(normalizedPath, "/Dashboard", StringComparison.OrdinalIgnoreCase) || normalizedPath == "/") return null;

        // Check if referer is internal and different from current page
        if (!string.IsNullOrWhiteSpace(refererUrl) && Uri.TryCreate(refererUrl, UriKind.RelativeOrAbsolute, out var uri))
        {
            var refererPath = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString.Split('?')[0];
            refererPath = NormalizePath(refererPath);

            if (!string.Equals(refererPath, normalizedPath, StringComparison.OrdinalIgnoreCase)
                && refererPath.StartsWith("/")
                && Routes.TryGetValue(refererPath, out var refRoute))
            {
                return new BreadcrumbItem($"Back to {refRoute.Title}", refRoute.Url);
            }
        }

        // Fallback to parent in route map
        if (Routes.TryGetValue(normalizedPath, out var route) && !string.IsNullOrEmpty(route.ParentUrl))
        {
            if (Routes.TryGetValue(route.ParentUrl, out var parentRoute))
            {
                return new BreadcrumbItem($"Back to {parentRoute.Title}", parentRoute.Url);
            }
            return new BreadcrumbItem("Back", route.ParentUrl);
        }

        return new BreadcrumbItem("Back to Dashboard", "/Dashboard");
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "/Dashboard";
        var clean = path.Split('?')[0].TrimEnd('/');
        return string.IsNullOrEmpty(clean) ? "/" : clean;
    }

    private static string FormatSlug(string path)
    {
        var slug = path.TrimStart('/');
        if (string.IsNullOrEmpty(slug)) return "Page";
        // Convert PascalCase or kebab-case to words
        var words = Regex.Replace(slug, "(\\B[A-Z])", " $1").Replace('-', ' ');
        return words;
    }
}
