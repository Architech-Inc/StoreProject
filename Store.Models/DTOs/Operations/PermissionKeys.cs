namespace Store.Models.DTOs.Operations;

public static class PermissionKeys
{
    // ─── Inventory / Catalog ───────────────────────────────────────────────────
    public const string InventoryRead = "inventory.read";
    public const string InventoryWrite = "inventory.write";
    public const string ItemCreate = "item.create";
    public const string ItemUpdate = "item.update";
    public const string ItemDelete = "item.delete";
    public const string ItemImport = "item.import";

    // ─── Pricing ────────────────────────────────────────────────────────────────
    public const string PricingRead = "pricing.read";
    public const string PricingWrite = "pricing.write";

    // ─── Cash / POS ─────────────────────────────────────────────────────────────
    public const string CashRead = "cash.read";
    public const string CashWrite = "cash.write";
    public const string PosCheckout = "pos.checkout";
    public const string PosRefund = "pos.refund";

    // ─── Invoices ──────────────────────────────────────────────────────────────
    public const string InvoiceRead = "invoice.read";
    public const string InvoiceCreate = "invoice.create";
    public const string InvoiceVoid = "invoice.void";
    public const string InvoiceRefund = "invoice.refund";

    // ─── Orders / Purchase Orders ──────────────────────────────────────────────
    public const string OrdersRead = "orders.read";
    public const string OrdersCreate = "orders.create";
    public const string OrdersVoid = "orders.void";
    public const string PurchaseOrderRead = "purchase_order.read";
    public const string PurchaseOrderWrite = "purchase_order.write";

    // ─── Customers ─────────────────────────────────────────────────────────────
    public const string CustomerRead = "customer.read";
    public const string CustomerCreate = "customer.create";
    public const string CustomerUpdate = "customer.update";
    public const string CustomerDelete = "customer.delete";

    // ─── Employees ─────────────────────────────────────────────────────────────
    public const string EmployeeRead = "employee.read";
    public const string EmployeeCreate = "employee.create";
    public const string EmployeeUpdate = "employee.update";
    public const string EmployeeDelete = "employee.delete";

    // ─── Suppliers ─────────────────────────────────────────────────────────────
    public const string SupplierRead = "supplier.read";
    public const string SupplierWrite = "supplier.write";

    // ─── Batches / Inventory Ops ───────────────────────────────────────────────
    public const string BatchRead = "batch.read";
    public const string BatchWrite = "batch.write";
    public const string WastageWrite = "wastage.write";

    // ─── Finance / Reports / Tax ───────────────────────────────────────────────
    public const string ReportsRead = "reports.read";
    public const string FinanceRead = "finance.read";
    public const string FinanceWrite = "finance.write";
    public const string TaxBracketsRead = "tax.read";
    public const string TaxBracketsWrite = "tax.write";

    // ─── Discounts / Loyalty ───────────────────────────────────────────────────
    public const string DiscountRead = "discount.read";
    public const string DiscountWrite = "discount.write";
    public const string LoyaltyRead = "loyalty.read";
    public const string LoyaltyWrite = "loyalty.write";

    // ─── Payroll / HR ──────────────────────────────────────────────────────────
    public const string PayrollRead = "payroll.read";
    public const string PayrollWrite = "payroll.write";

    // ─── Communications ────────────────────────────────────────────────────────
    public const string CommunicationsRead = "communications.read";
    public const string CommunicationsWrite = "communications.write";

    // ─── Audit ─────────────────────────────────────────────────────────────────
    public const string AuditRead = "audit.read";

    // ─── Payments ──────────────────────────────────────────────────────────────
    public const string PaymentsRead = "payments.read";

    // ─── Admin ─────────────────────────────────────────────────────────────────
    public const string AdminRoleMatrix = "admin.rolematrix";
    public const string AdminBranches = "admin.branches";
    public const string AdminUsers = "admin.users";
    public const string AdminSettings = "admin.settings";
    public const string AdminSystem = "admin.system";

    // ─── Cross-cutting ─────────────────────────────────────────────────────────
    public const string ViewCrossBranchStock = "inventory.cross_branch_stock";
    public const string ScannerResolve = "scanner.resolve";

    public static readonly string[] All =
    [
        InventoryRead,
        InventoryWrite,
        ItemCreate,
        ItemUpdate,
        ItemDelete,
        ItemImport,
        PricingRead,
        PricingWrite,
        CashRead,
        CashWrite,
        PosCheckout,
        PosRefund,
        InvoiceRead,
        InvoiceCreate,
        InvoiceVoid,
        InvoiceRefund,
        OrdersRead,
        OrdersCreate,
        OrdersVoid,
        PurchaseOrderRead,
        PurchaseOrderWrite,
        CustomerRead,
        CustomerCreate,
        CustomerUpdate,
        CustomerDelete,
        EmployeeRead,
        EmployeeCreate,
        EmployeeUpdate,
        EmployeeDelete,
        SupplierRead,
        SupplierWrite,
        BatchRead,
        BatchWrite,
        WastageWrite,
        ReportsRead,
        FinanceRead,
        FinanceWrite,
        TaxBracketsRead,
        TaxBracketsWrite,
        DiscountRead,
        DiscountWrite,
        LoyaltyRead,
        LoyaltyWrite,
        PayrollRead,
        PayrollWrite,
        CommunicationsRead,
        CommunicationsWrite,
        AuditRead,
        PaymentsRead,
        AdminRoleMatrix,
        AdminBranches,
        AdminUsers,
        AdminSettings,
        AdminSystem,
        ViewCrossBranchStock,
        ScannerResolve
    ];
}
