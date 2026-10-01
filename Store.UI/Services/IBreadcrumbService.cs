namespace StoreUI.Services;

public record BreadcrumbItem(
    string Label,
    string Url,
    bool IsActive = false,
    string? Icon = null
);

public interface IBreadcrumbService
{
    IReadOnlyList<BreadcrumbItem> BuildBreadcrumbs(
        string path,
        string? pageTitle = null,
        IReadOnlyList<BreadcrumbItem>? customCrumbs = null);

    BreadcrumbItem? GetBackLink(
        string path,
        string? refererUrl = null,
        BreadcrumbItem? customBack = null);
}
