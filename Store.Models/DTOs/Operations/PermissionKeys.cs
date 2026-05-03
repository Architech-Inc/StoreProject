namespace Store.Models.DTOs.Operations;

public static class PermissionKeys
{
    public const string InventoryRead = "inventory.read";
    public const string InventoryWrite = "inventory.write";
    public const string PricingRead = "pricing.read";
    public const string PricingWrite = "pricing.write";
    public const string CashRead = "cash.read";
    public const string CashWrite = "cash.write";
    public const string ReportsRead = "reports.read";
    public const string AdminRoleMatrix = "admin.rolematrix";
    public const string PaymentsRead = "payments.read";
    public const string AdminBranches = "admin.branches";

    public static readonly string[] All =
    [
        InventoryRead,
        InventoryWrite,
        PricingRead,
        PricingWrite,
        CashRead,
        CashWrite,
        ReportsRead,
        AdminRoleMatrix,
        PaymentsRead,
        AdminBranches
    ];
}
