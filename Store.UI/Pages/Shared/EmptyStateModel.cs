namespace Store.UI.Pages.Shared;

/// <summary>
/// UX-06 — strongly-typed model for the <c>_EmptyState</c> partial.
/// Rendered in pages like Catalog, Suppliers, Customers, Employees, Batches
/// to give operators a meaningful next step instead of "No records found".
/// </summary>
public class EmptyStateModel
{
    public string Icon { get; set; } = "📭";
    public string Title { get; set; } = "Nothing here yet";
    public string Message { get; set; } = "When you add your first item it'll appear here.";
    public string? CtaText { get; set; }
    public string? CtaHref { get; set; }
    public string? SecondaryText { get; set; }
    public string? SecondaryHref { get; set; }
    public string Variant { get; set; } = "info"; // info | warning | muted
}