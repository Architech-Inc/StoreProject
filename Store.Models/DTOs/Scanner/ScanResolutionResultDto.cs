namespace Store.Models.DTOs.Scanner;

public class ScanResolutionResultDto
{
    public string EntityType { get; set; } = "Unknown";
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? EntityId { get; set; }
    public Dictionary<string, string> Details { get; set; } = new();
    public List<ScanActionDto> Actions { get; set; } = new();
}

public class ScanActionDto
{
    public string ActionId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string TargetUrl { get; set; } = string.Empty;
    public string ButtonClass { get; set; } = "button-primary";
    public string? ShortcutKey { get; set; }
    public string? PermissionRequired { get; set; }
}

public static class ScanEntityTypes
{
    public const string Item = "Item";
    public const string Invoice = "Invoice";
    public const string User = "User";
    public const string Customer = "Customer";
    public const string Supplier = "Supplier";
    public const string Batch = "Batch";
    public const string Unknown = "Unknown";
}
