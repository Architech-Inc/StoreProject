# API Endpoint and Client Call Analysis

This report analyzes what happens when an endpoint is hit, its inputs/outputs, and how the client processes it.

## AdminRoleMatrix Controller

### `GET api/admin/role-matrix`
- **Backend Method**: `GetRoleMatrix` in `AdminRoleMatrixController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/admin/role-matrix/permission`
- **Backend Method**: `UpdatePermission` in `AdminRoleMatrixController.cs`
- **Receives (Parameters)**: `[FromBody] UpdateRolePermissionRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## AuditLogs Controller

### `GET api/audit-logs/metrics`
- **Backend Method**: `GetMetrics` in `AuditLogsController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/audit-logs/paged`
- **Backend Method**: `GetPaged` in `AuditLogsController.cs`
- **Receives (Parameters)**: `[FromQuery] AuditLogFilterRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/audit-logs/{id:long}`
- **Backend Method**: `GetById` in `AuditLogsController.cs`
- **Receives (Parameters)**: `long id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/audit-logs`
- **Backend Method**: `Create` in `AuditLogsController.cs`
- **Receives (Parameters)**: `[FromBody] CreateAuditLogEntryRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/audit-logs/export/csv`
- **Backend Method**: `ExportCsv` in `AuditLogsController.cs`
- **Receives (Parameters)**: `[FromQuery] AuditLogFilterRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/audit-logs/export/json`
- **Backend Method**: `ExportJson` in `AuditLogsController.cs`
- **Receives (Parameters)**: `[FromQuery] AuditLogFilterRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Auth Controller

### `POST api/auth/login`
- **Backend Method**: `Login` in `AuthController.cs`
- **Receives (Parameters)**: `[FromBody] LoginRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/auth/login/email`
- **Backend Method**: `LoginWithEmail` in `AuthController.cs`
- **Receives (Parameters)**: `[FromBody] LoginWithEmailRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/auth/login/phone`
- **Backend Method**: `LoginWithPhone` in `AuthController.cs`
- **Receives (Parameters)**: `[FromBody] LoginWithPhoneRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/auth/login/2fa`
- **Backend Method**: `Login2FA` in `AuthController.cs`
- **Receives (Parameters)**: `[FromBody] Login2FARequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/auth/refresh`
- **Backend Method**: `Refresh` in `AuthController.cs`
- **Receives (Parameters)**: `[FromBody] RefreshTokenRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/auth/logout`
- **Backend Method**: `Logout` in `AuthController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/auth/reset-password`
- **Backend Method**: `ResetPassword` in `AuthController.cs`
- **Receives (Parameters)**: `[FromBody] ResetPasswordRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/auth/avatar/{username}`
- **Backend Method**: `GetAvatar` in `AuthController.cs`
- **Receives (Parameters)**: `string username, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Batches Controller

### `GET api/batches`
- **Backend Method**: `GetAll` in `BatchesController.cs`
- **Receives (Parameters)**: `[FromQuery] Guid? itemId, [FromQuery] string? expiryStatus`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/batches/paged`
- **Backend Method**: `GetPaged` in `BatchesController.cs`
- **Receives (Parameters)**: `[FromQuery] BatchFilterRequest request, CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/batches/metrics`
- **Backend Method**: `GetMetrics` in `BatchesController.cs`
- **Receives (Parameters)**: `CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/batches/expiring`
- **Backend Method**: `GetExpiring` in `BatchesController.cs`
- **Receives (Parameters)**: `[FromQuery] int withinDays = 30`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/batches/write-off`
- **Backend Method**: `WriteOff` in `BatchesController.cs`
- **Receives (Parameters)**: `[FromBody] WriteOffBatchRequest request, CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/batches/{id:guid}`
- **Backend Method**: `GetById` in `BatchesController.cs`
- **Receives (Parameters)**: `Guid id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/batches`
- **Backend Method**: `Create` in `BatchesController.cs`
- **Receives (Parameters)**: `[FromBody] CreateBatchRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/batches/{id:guid}`
- **Backend Method**: `Update` in `BatchesController.cs`
- **Receives (Parameters)**: `Guid id, [FromBody] UpdateBatchRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/batches/{id:guid}`
- **Backend Method**: `Delete` in `BatchesController.cs`
- **Receives (Parameters)**: `Guid id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Branch Controller

### `GET api/admin/branches`
- **Backend Method**: `GetBranches` in `BranchController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/admin/branches`
- **Backend Method**: `UpsertBranch` in `BranchController.cs`
- **Receives (Parameters)**: `[FromBody] UpsertBranchRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/admin/branches/assignments`
- **Backend Method**: `GetAssignments` in `BranchController.cs`
- **Receives (Parameters)**: `[FromQuery] int? branchId, [FromQuery] Guid? userId, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/admin/branches/assignments`
- **Backend Method**: `AssignUserBranchRole` in `BranchController.cs`
- **Receives (Parameters)**: `[FromBody] AssignUserBranchRoleRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/admin/branches/assignments/{id:long}`
- **Backend Method**: `RemoveAssignment` in `BranchController.cs`
- **Receives (Parameters)**: `long id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/admin/branches/{id:int}/performance`
- **Backend Method**: `GetPerformance` in `BranchController.cs`
- **Receives (Parameters)**: `int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/admin/branches/transfers`
- **Backend Method**: `TransferPersonnel` in `BranchController.cs`
- **Receives (Parameters)**: `[FromBody] TransferEmployeeRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/admin/branches/transfers`
- **Backend Method**: `GetTransfers` in `BranchController.cs`
- **Receives (Parameters)**: `[FromQuery] Guid? employeeId, [FromQuery] int? branchId, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/admin/branches/{id:int}/stock`
- **Backend Method**: `GetBranchStock` in `BranchController.cs`
- **Receives (Parameters)**: `int id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/admin/branches/{id:int}/stock`
- **Backend Method**: `UpdateBranchStock` in `BranchController.cs`
- **Receives (Parameters)**: `int id, [FromBody] UpdateBranchStockRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## CashManagement Controller

### `GET api/cash/shift/active`
- **Backend Method**: `GetActiveShift` in `CashManagementController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/cash/shift/open`
- **Backend Method**: `OpenShift` in `CashManagementController.cs`
- **Receives (Parameters)**: `[FromBody] ShiftOpenRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/cash/shift/close`
- **Backend Method**: `CloseShift` in `CashManagementController.cs`
- **Receives (Parameters)**: `[FromBody] ShiftCloseRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/cash/shifts`
- **Backend Method**: `GetShifts` in `CashManagementController.cs`
- **Receives (Parameters)**: `[FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/cash/report/z`
- **Backend Method**: `DailyZReport` in `CashManagementController.cs`
- **Receives (Parameters)**: `[FromQuery] DateTime? dateUtc, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/cash/reconciliation`
- **Backend Method**: `DayEndReconciliation` in `CashManagementController.cs`
- **Receives (Parameters)**: `[FromQuery] DateOnly? date, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## CashVariance Controller

### `GET api/cash/variances/metrics`
- **Backend Method**: `GetMetrics` in `CashVarianceController.cs`
- **Receives (Parameters)**: `None`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/cash/variances`
- **Backend Method**: `GetAll` in `CashVarianceController.cs`
- **Receives (Parameters)**: `
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/cash/variances/{id:int}`
- **Backend Method**: `GetById` in `CashVarianceController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/cash/variances/by-shift/{shiftId:guid}`
- **Backend Method**: `GetByShift` in `CashVarianceController.cs`
- **Receives (Parameters)**: `Guid shiftId`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/cash/variances`
- **Backend Method**: `Record` in `CashVarianceController.cs`
- **Receives (Parameters)**: `[FromBody] RecordCashVarianceRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/cash/variances/{id:int}/review`
- **Backend Method**: `Review` in `CashVarianceController.cs`
- **Receives (Parameters)**: `int id, [FromBody] ReviewCashVarianceRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/cash/variances/export/csv`
- **Backend Method**: `ExportCsv` in `CashVarianceController.cs`
- **Receives (Parameters)**: `[FromQuery] string? status`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## CommunicationLogs Controller

### `GET api/communicationlogs`
- **Backend Method**: `GetLogs` in `CommunicationLogsController.cs`
- **Receives (Parameters)**: `[FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? channel = null, [FromQuery] string? status = null, CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Customers Controller

### `GET api/customers`
- **Backend Method**: `GetAll` in `CustomersController.cs`
- **Receives (Parameters)**: `[FromQuery] PagedRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/customers/{id:guid}`
- **Backend Method**: `GetById` in `CustomersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/customers/{id:guid}/invoices`
- **Backend Method**: `GetInvoices` in `CustomersController.cs`
- **Receives (Parameters)**: `Guid id, [FromQuery] int take = 20, CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/customers/{id:guid}/loyalty-transactions`
- **Backend Method**: `GetLoyaltyTransactions` in `CustomersController.cs`
- **Receives (Parameters)**: `Guid id, [FromQuery] int take = 20, CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/customers`
- **Backend Method**: `Create` in `CustomersController.cs`
- **Receives (Parameters)**: `[FromBody] CreateCustomerRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/customers/{id:guid}`
- **Backend Method**: `Update` in `CustomersController.cs`
- **Receives (Parameters)**: `Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/customers/{id:guid}`
- **Backend Method**: `Delete` in `CustomersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## DiscountOverrides Controller

### `GET api/discount-overrides/metrics`
- **Backend Method**: `GetMetrics` in `DiscountOverridesController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/discount-overrides/paged`
- **Backend Method**: `GetPaged` in `DiscountOverridesController.cs`
- **Receives (Parameters)**: `[FromQuery] DiscountOverrideFilterRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/discount-overrides`
- **Backend Method**: `GetAll` in `DiscountOverridesController.cs`
- **Receives (Parameters)**: `[FromQuery] string? status`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/discount-overrides/{id:int}`
- **Backend Method**: `GetById` in `DiscountOverridesController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/discount-overrides`
- **Backend Method**: `Create` in `DiscountOverridesController.cs`
- **Receives (Parameters)**: `[FromBody] CreateDiscountOverrideRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/discount-overrides/{id:int}/review`
- **Backend Method**: `Review` in `DiscountOverridesController.cs`
- **Receives (Parameters)**: `int id, [FromBody] ReviewDiscountOverrideRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/discount-overrides/{id:int}/cancel`
- **Backend Method**: `Cancel` in `DiscountOverridesController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/discount-overrides/export/csv`
- **Backend Method**: `ExportCsv` in `DiscountOverridesController.cs`
- **Receives (Parameters)**: `[FromQuery] DiscountOverrideFilterRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Discounts Controller

### `GET api/discounts/metrics`
- **Backend Method**: `GetMetrics` in `DiscountsController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/discounts/paged`
- **Backend Method**: `GetPaged` in `DiscountsController.cs`
- **Receives (Parameters)**: `[FromQuery] DiscountFilterRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/discounts`
- **Backend Method**: `GetAll` in `DiscountsController.cs`
- **Receives (Parameters)**: `[FromQuery] bool? activeOnly, [FromQuery] string? couponCode`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/discounts/{id:int}`
- **Backend Method**: `GetById` in `DiscountsController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/discounts/simulate`
- **Backend Method**: `Simulate` in `DiscountsController.cs`
- **Receives (Parameters)**: `[FromBody] DiscountSimulationRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/discounts/validate-coupon`
- **Backend Method**: `ValidateCoupon` in `DiscountsController.cs`
- **Receives (Parameters)**: `[FromQuery] string code, [FromQuery] int? branchId`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/discounts`
- **Backend Method**: `Create` in `DiscountsController.cs`
- **Receives (Parameters)**: `[FromBody] CreateDiscountRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/discounts/{id:int}`
- **Backend Method**: `Update` in `DiscountsController.cs`
- **Receives (Parameters)**: `int id, [FromBody] UpdateDiscountRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/discounts/{id:int}`
- **Backend Method**: `Delete` in `DiscountsController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/discounts/{id:int}/increment-usage`
- **Backend Method**: `IncrementUsage` in `DiscountsController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/discounts/export/csv`
- **Backend Method**: `ExportCsv` in `DiscountsController.cs`
- **Receives (Parameters)**: `[FromQuery] DiscountFilterRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Employees Controller

### `GET api/employees`
- **Backend Method**: `GetAll` in `EmployeesController.cs`
- **Receives (Parameters)**: `[FromQuery] EmployeeFilterRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/employees/metrics`
- **Backend Method**: `GetMetrics` in `EmployeesController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/employees/{id:guid}`
- **Backend Method**: `GetById` in `EmployeesController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/employees/{id:guid}/360`
- **Backend Method**: `Get360ById` in `EmployeesController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/employees`
- **Backend Method**: `Create` in `EmployeesController.cs`
- **Receives (Parameters)**: `[FromBody] CreateEmployeeRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/employees/{id:guid}`
- **Backend Method**: `Update` in `EmployeesController.cs`
- **Receives (Parameters)**: `Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/employees/{id:guid}`
- **Backend Method**: `Delete` in `EmployeesController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Files Controller

### `POST api/files/upload`
- **Backend Method**: `UploadFile` in `FilesController.cs`
- **Receives (Parameters)**: `
        IFormFile file,
        [FromQuery] string folder = "misc",
        [FromQuery] int? cropX = null,
        [FromQuery] int? cropY = null,
        [FromQuery] int? cropW = null,
        [FromQuery] int? cropH = null`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/files`
- **Backend Method**: `DeleteFile` in `FilesController.cs`
- **Receives (Parameters)**: `[FromQuery] string relativePath`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## InventoryOperations Controller

### `GET api/inventory/movements`
- **Backend Method**: `GetMovements` in `InventoryOperationsController.cs`
- **Receives (Parameters)**: `[FromQuery] StockMovementFilterRequest request, CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/inventory/metrics`
- **Backend Method**: `GetMetrics` in `InventoryOperationsController.cs`
- **Receives (Parameters)**: `CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/inventory/receive`
- **Backend Method**: `ReceiveGoods` in `InventoryOperationsController.cs`
- **Receives (Parameters)**: `[FromBody] GoodsReceiptRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/inventory/return`
- **Backend Method**: `ProcessReturn` in `InventoryOperationsController.cs`
- **Receives (Parameters)**: `[FromBody] StockReturnRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/inventory/adjust`
- **Backend Method**: `AdjustStock` in `InventoryOperationsController.cs`
- **Receives (Parameters)**: `[FromBody] StockAdjustmentAuditRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/inventory/reorder`
- **Backend Method**: `ReorderSuggestions` in `InventoryOperationsController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Invoices Controller

### `GET api/invoices`
- **Backend Method**: `GetAll` in `InvoicesController.cs`
- **Receives (Parameters)**: `[FromQuery] InvoicePagedRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/invoices/summary`
- **Backend Method**: `GetSummary` in `InvoicesController.cs`
- **Receives (Parameters)**: `[FromQuery] InvoicePagedRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/invoices/{id:guid}`
- **Backend Method**: `GetById` in `InvoicesController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/invoices/public/{id:guid}`
- **Backend Method**: `GetPublicReceipt` in `InvoicesController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/invoices`
- **Backend Method**: `Create` in `InvoicesController.cs`
- **Receives (Parameters)**: `[FromBody] CreateInvoiceRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/invoices/{id:guid}/void`
- **Backend Method**: `Void` in `InvoicesController.cs`
- **Receives (Parameters)**: `Guid id, [FromQuery] string? reason, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/invoices/{id:guid}/void`
- **Backend Method**: `VoidCompat` in `InvoicesController.cs`
- **Receives (Parameters)**: `Guid id, [FromQuery] string? reason, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/invoices/{id:guid}/refund`
- **Backend Method**: `Refund` in `InvoicesController.cs`
- **Receives (Parameters)**: `Guid id, [FromBody] RefundInvoiceRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/invoices/{id:guid}/tender`
- **Backend Method**: `AddTender` in `InvoicesController.cs`
- **Receives (Parameters)**: `Guid id, [FromBody] AddTenderRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Item Controller

### `GET api/item`
- **Backend Method**: `GetAll` in `ItemController.cs`
- **Receives (Parameters)**: `[FromQuery] PagedRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/item/{id:guid}`
- **Backend Method**: `GetById` in `ItemController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/item/low-stock`
- **Backend Method**: `GetLowStock` in `ItemController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/item`
- **Backend Method**: `Create` in `ItemController.cs`
- **Receives (Parameters)**: `[FromBody] CreateItemRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/item/{id:guid}`
- **Backend Method**: `Update` in `ItemController.cs`
- **Receives (Parameters)**: `Guid id, [FromBody] UpdateItemRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PATCH api/item/{id:guid}/stock`
- **Backend Method**: `AdjustStock` in `ItemController.cs`
- **Receives (Parameters)**: `Guid id, [FromBody] AdjustStockRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/item/{id:guid}/adjust-stock`
- **Backend Method**: `AdjustStockCompat` in `ItemController.cs`
- **Receives (Parameters)**: `Guid id, [FromBody] AdjustStockRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/item/{id:guid}`
- **Backend Method**: `Delete` in `ItemController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Lookups Controller

### `GET api/lookups`
- **Backend Method**: `GetAll` in `LookupControllers.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/lookups/{id:int}`
- **Backend Method**: `GetById` in `LookupControllers.cs`
- **Receives (Parameters)**: `int id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/lookups`
- **Backend Method**: `Create` in `LookupControllers.cs`
- **Receives (Parameters)**: `[FromBody] CreateCategoryRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/lookups/{id:int}`
- **Backend Method**: `Update` in `LookupControllers.cs`
- **Receives (Parameters)**: `int id, [FromBody] CreateCategoryRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/lookups/{id:int}`
- **Backend Method**: `Delete` in `LookupControllers.cs`
- **Receives (Parameters)**: `int id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/lookups`
- **Backend Method**: `GetAll` in `LookupControllers.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/lookups`
- **Backend Method**: `Create` in `LookupControllers.cs`
- **Receives (Parameters)**: `[FromBody] CreateUnitRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/lookups/{id:int}`
- **Backend Method**: `Update` in `LookupControllers.cs`
- **Receives (Parameters)**: `int id, [FromBody] CreateUnitRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/lookups/{id:int}`
- **Backend Method**: `Delete` in `LookupControllers.cs`
- **Receives (Parameters)**: `int id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/lookups`
- **Backend Method**: `GetAll` in `LookupControllers.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/lookups`
- **Backend Method**: `Create` in `LookupControllers.cs`
- **Receives (Parameters)**: `[FromBody] CreateLookupRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/lookups/{id:int}`
- **Backend Method**: `Update` in `LookupControllers.cs`
- **Receives (Parameters)**: `int id, [FromBody] CreateLookupRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/lookups/{id:int}`
- **Backend Method**: `Delete` in `LookupControllers.cs`
- **Receives (Parameters)**: `int id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/lookups`
- **Backend Method**: `GetAll` in `LookupControllers.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## LoyaltyCampaigns Controller

### `GET api/loyaltycampaigns`
- **Backend Method**: `GetAll` in `LoyaltyCampaignsController.cs`
- **Receives (Parameters)**: `[FromQuery] bool? activeOnly, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/loyaltycampaigns/{id:int}`
- **Backend Method**: `GetById` in `LoyaltyCampaignsController.cs`
- **Receives (Parameters)**: `int id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/loyaltycampaigns/active`
- **Backend Method**: `GetActiveForSegment` in `LoyaltyCampaignsController.cs`
- **Receives (Parameters)**: `[FromQuery] string segment, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/loyaltycampaigns`
- **Backend Method**: `Create` in `LoyaltyCampaignsController.cs`
- **Receives (Parameters)**: `[FromBody] CreateCampaignRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/loyaltycampaigns/{id:int}`
- **Backend Method**: `Update` in `LoyaltyCampaignsController.cs`
- **Receives (Parameters)**: `int id, [FromBody] UpdateCampaignRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/loyaltycampaigns/{id:int}`
- **Backend Method**: `Delete` in `LoyaltyCampaignsController.cs`
- **Receives (Parameters)**: `int id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Loyalty Controller

### `GET api/loyalty/metrics`
- **Backend Method**: `GetMetrics` in `LoyaltyController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/loyalty/members`
- **Backend Method**: `GetAllMembers` in `LoyaltyController.cs`
- **Receives (Parameters)**: `
        [FromQuery] string? search = null,
        [FromQuery] string? tier = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/loyalty/customers/{customerId:guid}/profile`
- **Backend Method**: `GetProfile` in `LoyaltyController.cs`
- **Receives (Parameters)**: `Guid customerId, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/loyalty/customers/{customerId:guid}`
- **Backend Method**: `GetAccount` in `LoyaltyController.cs`
- **Receives (Parameters)**: `Guid customerId, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/loyalty/customers/{customerId:guid}/transactions`
- **Backend Method**: `GetTransactions` in `LoyaltyController.cs`
- **Receives (Parameters)**: `Guid customerId, [FromQuery] int take = 50, CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/loyalty/transactions`
- **Backend Method**: `GetGlobalTransactions` in `LoyaltyController.cs`
- **Receives (Parameters)**: `
        [FromQuery] string? search = null,
        [FromQuery] string? transactionType = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int take = 50,
        CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/loyalty/earn`
- **Backend Method**: `Earn` in `LoyaltyController.cs`
- **Receives (Parameters)**: `[FromBody] EarnPointsRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/loyalty/redeem`
- **Backend Method**: `Redeem` in `LoyaltyController.cs`
- **Receives (Parameters)**: `[FromBody] RedeemPointsRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/loyalty/adjust`
- **Backend Method**: `Adjust` in `LoyaltyController.cs`
- **Receives (Parameters)**: `[FromBody] AdjustPointsRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/loyalty/manage`
- **Backend Method**: `ManagePoints` in `LoyaltyController.cs`
- **Receives (Parameters)**: `[FromBody] ManagePointsRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Orders Controller

### `GET api/orders`
- **Backend Method**: `GetAll` in `OrdersController.cs`
- **Receives (Parameters)**: `[FromQuery] PagedRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/orders/{id:guid}`
- **Backend Method**: `GetById` in `OrdersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/orders`
- **Backend Method**: `Create` in `OrdersController.cs`
- **Receives (Parameters)**: `[FromBody] CreateOrderRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/orders/{id:guid}/receive`
- **Backend Method**: `Receive` in `OrdersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/orders/{id:guid}/cancel`
- **Backend Method**: `Cancel` in `OrdersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## PasswordRecovery Controller

### `POST api/auth/recovery/request`
- **Backend Method**: `RequestOtp` in `PasswordRecoveryController.cs`
- **Receives (Parameters)**: `[FromBody] RequestOtpRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/auth/recovery/verify`
- **Backend Method**: `VerifyOtp` in `PasswordRecoveryController.cs`
- **Receives (Parameters)**: `[FromBody] VerifyOtpRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/auth/recovery/reset`
- **Backend Method**: `ResetPassword` in `PasswordRecoveryController.cs`
- **Receives (Parameters)**: `[FromBody] RecoverPasswordWithTokenRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Payments Controller

### `POST api/payments/momo/initiate`
- **Backend Method**: `Initiate` in `PaymentsController.cs`
- **Receives (Parameters)**: `[FromBody] InitiateMobileMoneyRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/payments/momo/callback`
- **Backend Method**: `MtnMomoCallback` in `PaymentsController.cs`
- **Receives (Parameters)**: `[FromBody] MtnMomoCallbackRequest callback, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/payments/orange/callback`
- **Backend Method**: `OrangeMoneyCallback` in `PaymentsController.cs`
- **Receives (Parameters)**: `[FromBody] OrangeMoneyCallbackRequest callback, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/payments/settlement`
- **Backend Method**: `GetSettlement` in `PaymentsController.cs`
- **Receives (Parameters)**: `
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/payments/momo`
- **Backend Method**: `GetTransactions` in `PaymentsController.cs`
- **Receives (Parameters)**: `
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] MobileMoneyStatus? status = null,
        CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/payments/momo/{id:guid}`
- **Backend Method**: `GetTransactionById` in `PaymentsController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Payroll Controller

### `GET api/payroll`
- **Backend Method**: `GetAllRuns` in `PayrollController.cs`
- **Receives (Parameters)**: `None`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/payroll/{id:guid}`
- **Backend Method**: `GetRunById` in `PayrollController.cs`
- **Receives (Parameters)**: `Guid id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/payroll/{id:guid}/payslips`
- **Backend Method**: `GetPayslips` in `PayrollController.cs`
- **Receives (Parameters)**: `Guid id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/payroll/draft`
- **Backend Method**: `DraftPayrollRun` in `PayrollController.cs`
- **Receives (Parameters)**: `[FromBody] DraftPayrollRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/payroll/{id:guid}/approve`
- **Backend Method**: `ApprovePayroll` in `PayrollController.cs`
- **Receives (Parameters)**: `Guid id`
- **Returns**: `IActionResult`
- **Client Call(s)**:
  - Found in `PayrollManager.cs` using `POST`
  - Client expects generic type: `ApiResponse`

### `POST api/payroll/{id:guid}/pay`
- **Backend Method**: `PayPayroll` in `PayrollController.cs`
- **Receives (Parameters)**: `Guid id`
- **Returns**: `IActionResult`
- **Client Call(s)**:
  - Found in `PayrollManager.cs` using `POST`
  - Client expects generic type: `ApiResponse`

## Pricing Controller

### `GET api/pricing/metrics`
- **Backend Method**: `GetMetrics` in `PricingController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/pricing/tax-profiles`
- **Backend Method**: `GetTaxProfiles` in `PricingController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/pricing/tax-profiles`
- **Backend Method**: `UpsertTaxProfile` in `PricingController.cs`
- **Receives (Parameters)**: `[FromBody] UpsertTaxProfileRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/pricing/bundles`
- **Backend Method**: `GetBundleRules` in `PricingController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/pricing/bundles`
- **Backend Method**: `UpsertBundleRule` in `PricingController.cs`
- **Receives (Parameters)**: `[FromBody] UpsertBundleRuleRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/pricing/segment-pricing`
- **Backend Method**: `GetSegmentPricing` in `PricingController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/pricing/segment-pricing`
- **Backend Method**: `UpsertSegmentPricing` in `PricingController.cs`
- **Receives (Parameters)**: `[FromBody] UpsertSegmentPricingRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/pricing/preview`
- **Backend Method**: `Preview` in `PricingController.cs`
- **Receives (Parameters)**: `[FromBody] PricingPreviewRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/pricing/promotions/effectiveness`
- **Backend Method**: `GetPromotionEffectiveness` in `PricingController.cs`
- **Receives (Parameters)**: `
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/pricing/promotions/export/csv`
- **Backend Method**: `ExportPromotionEffectivenessCsv` in `PricingController.cs`
- **Receives (Parameters)**: `
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? section,
        CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/pricing/export/csv`
- **Backend Method**: `ExportCsv` in `PricingController.cs`
- **Receives (Parameters)**: `[FromQuery] string? type, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## PurchaseOrders Controller

### `GET api/purchase-orders/metrics`
- **Backend Method**: `GetMetrics` in `PurchaseOrdersController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/purchase-orders/paged`
- **Backend Method**: `GetPaged` in `PurchaseOrdersController.cs`
- **Receives (Parameters)**: `[FromQuery] PurchaseOrderFilterRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/purchase-orders`
- **Backend Method**: `GetAll` in `PurchaseOrdersController.cs`
- **Receives (Parameters)**: `
        [FromQuery] string? status,
        [FromQuery] Guid? supplierId`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/purchase-orders/{id:int}`
- **Backend Method**: `GetById` in `PurchaseOrdersController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/purchase-orders`
- **Backend Method**: `Create` in `PurchaseOrdersController.cs`
- **Receives (Parameters)**: `[FromBody] CreatePurchaseOrderRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/purchase-orders/{id:int}/submit`
- **Backend Method**: `Submit` in `PurchaseOrdersController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/purchase-orders/{id:int}/approve`
- **Backend Method**: `Approve` in `PurchaseOrdersController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/purchase-orders/{id:int}/receive`
- **Backend Method**: `Receive` in `PurchaseOrdersController.cs`
- **Receives (Parameters)**: `int id, [FromBody] ReceivePurchaseOrderRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/purchase-orders/{id:int}/cancel`
- **Backend Method**: `Cancel` in `PurchaseOrdersController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/purchase-orders/{id:int}/pay`
- **Backend Method**: `Pay` in `PurchaseOrdersController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/purchase-orders/auto-reorder/trigger`
- **Backend Method**: `TriggerAutoReorder` in `PurchaseOrdersController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Restock Controller

### `GET api/restock/pending`
- **Backend Method**: `GetPendingRecommendations` in `RestockController.cs`
- **Receives (Parameters)**: `[FromQuery] int? branchId, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/restock/{recommendationId:guid}/convert-to-transfer`
- **Backend Method**: `ConvertToTransfer` in `RestockController.cs`
- **Receives (Parameters)**: `Guid recommendationId, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/restock/{recommendationId:guid}/convert-to-po`
- **Backend Method**: `ConvertToPurchaseOrder` in `RestockController.cs`
- **Receives (Parameters)**: `Guid recommendationId, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/restock/{recommendationId:guid}/dismiss`
- **Backend Method**: `DismissRecommendation` in `RestockController.cs`
- **Receives (Parameters)**: `Guid recommendationId, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Scanner Controller

### `GET api/scanner/resolve`
- **Backend Method**: `Resolve` in `ScannerController.cs`
- **Receives (Parameters)**: `[FromQuery] string code, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## StockTransfers Controller

### `GET api/stocktransfers`
- **Backend Method**: `GetAll` in `StockTransfersController.cs`
- **Receives (Parameters)**: `[FromQuery] int? branchId, [FromQuery] string? status`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/stocktransfers/paged`
- **Backend Method**: `GetPaged` in `StockTransfersController.cs`
- **Receives (Parameters)**: `[FromQuery] TransferFilterRequest request, CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/stocktransfers/metrics`
- **Backend Method**: `GetMetrics` in `StockTransfersController.cs`
- **Receives (Parameters)**: `CancellationToken ct = default`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/stocktransfers/{id:int}`
- **Backend Method**: `GetById` in `StockTransfersController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/stocktransfers`
- **Backend Method**: `Create` in `StockTransfersController.cs`
- **Receives (Parameters)**: `[FromBody] CreateTransferRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/stocktransfers/{id:int}/approve`
- **Backend Method**: `Approve` in `StockTransfersController.cs`
- **Receives (Parameters)**: `int id, [FromBody] ApproveTransferRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/stocktransfers/{id:int}/reject`
- **Backend Method**: `Reject` in `StockTransfersController.cs`
- **Receives (Parameters)**: `int id, [FromBody] RejectTransferRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/stocktransfers/{id:int}/dispatch`
- **Backend Method**: `Dispatch` in `StockTransfersController.cs`
- **Receives (Parameters)**: `int id, [FromBody] DispatchTransferRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/stocktransfers/{id:int}/receive`
- **Backend Method**: `Receive` in `StockTransfersController.cs`
- **Receives (Parameters)**: `int id, [FromBody] ReceiveTransferRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/stocktransfers/{id:int}/cancel`
- **Backend Method**: `Cancel` in `StockTransfersController.cs`
- **Receives (Parameters)**: `int id, [FromBody] string? reason`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Suppliers Controller

### `GET api/suppliers`
- **Backend Method**: `GetAll` in `SuppliersController.cs`
- **Receives (Parameters)**: `
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null,
        [FromQuery] string? sortBy = null`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/suppliers/paged`
- **Backend Method**: `GetPaged` in `SuppliersController.cs`
- **Receives (Parameters)**: `[FromQuery] PagedRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/suppliers/metrics`
- **Backend Method**: `GetMetrics` in `SuppliersController.cs`
- **Receives (Parameters)**: `None`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/suppliers/{id:guid}`
- **Backend Method**: `GetById` in `SuppliersController.cs`
- **Receives (Parameters)**: `Guid id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/suppliers/{id:guid}/profile`
- **Backend Method**: `GetProfile` in `SuppliersController.cs`
- **Receives (Parameters)**: `Guid id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/suppliers`
- **Backend Method**: `Create` in `SuppliersController.cs`
- **Receives (Parameters)**: `[FromBody] CreateSupplierRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/suppliers/{id:guid}`
- **Backend Method**: `Update` in `SuppliersController.cs`
- **Receives (Parameters)**: `Guid id, [FromBody] UpdateSupplierRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/suppliers/{id:guid}`
- **Backend Method**: `Delete` in `SuppliersController.cs`
- **Receives (Parameters)**: `Guid id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## SystemSettings Controller

### `GET api/settings/{*key}`
- **Backend Method**: `GetSetting` in `SystemSettingsController.cs`
- **Receives (Parameters)**: `string key, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/settings/{*key}`
- **Backend Method**: `UpdateSetting` in `SystemSettingsController.cs`
- **Receives (Parameters)**: `string key, [FromBody] UpdateSettingRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Users Controller

### `GET api/users`
- **Backend Method**: `GetAll` in `UsersController.cs`
- **Receives (Parameters)**: `[FromQuery] PagedRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/users/{id:guid}`
- **Backend Method**: `GetById` in `UsersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/users/{id:guid}/360`
- **Backend Method**: `Get360ById` in `UsersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users`
- **Backend Method**: `Create` in `UsersController.cs`
- **Receives (Parameters)**: `[FromBody] CreateUserRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/users/{id:guid}`
- **Backend Method**: `Update` in `UsersController.cs`
- **Receives (Parameters)**: `Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/users/{id:guid}`
- **Backend Method**: `Delete` in `UsersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users/change-password`
- **Backend Method**: `ChangePassword` in `UsersController.cs`
- **Receives (Parameters)**: `[FromBody] ChangePasswordRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/users/profile/avatar`
- **Backend Method**: `UpdateAvatar` in `UsersController.cs`
- **Receives (Parameters)**: `[FromBody] UpdateUserRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `PUT api/users/profile/contacts`
- **Backend Method**: `UpdateContacts` in `UsersController.cs`
- **Receives (Parameters)**: `[FromBody] UpdateUserContactsRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users/{id:guid}/issue-temp-password`
- **Backend Method**: `IssueTempPassword` in `UsersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users/profile/2fa/enable`
- **Backend Method**: `Enable2FA` in `UsersController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users/profile/2fa/verify`
- **Backend Method**: `Verify2FA` in `UsersController.cs`
- **Receives (Parameters)**: `[FromBody] Verify2FARequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users/profile/2fa/disable`
- **Backend Method**: `Disable2FA` in `UsersController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/users/profile/activity`
- **Backend Method**: `GetRecentActivity` in `UsersController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users/profile/sessions/revoke`
- **Backend Method**: `RevokeAllSessions` in `UsersController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users/{id}/sessions/revoke`
- **Backend Method**: `RevokeUserSessions` in `UsersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users/profile/contact-change`
- **Backend Method**: `RequestContactChange` in `UsersController.cs`
- **Receives (Parameters)**: `[FromBody] CreateContactChangeDto request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/users/profile/contact-change/verify`
- **Backend Method**: `VerifyContactChange` in `UsersController.cs`
- **Receives (Parameters)**: `[FromQuery] string token, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/users/contact-changes/pending`
- **Backend Method**: `GetPendingContactChanges` in `UsersController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users/contact-changes/{id}/approve`
- **Backend Method**: `ApproveContactChange` in `UsersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users/contact-changes/{id}/reject`
- **Backend Method**: `RejectContactChange` in `UsersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/users/contact-changes/{id}/cancel`
- **Backend Method**: `CancelContactChange` in `UsersController.cs`
- **Receives (Parameters)**: `Guid id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/users/contact-changes/history`
- **Backend Method**: `GetContactChangeHistory` in `UsersController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## Wastage Controller

### `GET api/wastage/metrics`
- **Backend Method**: `GetMetrics` in `WastageController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/wastage/paged`
- **Backend Method**: `GetPaged` in `WastageController.cs`
- **Receives (Parameters)**: `[FromQuery] WastageFilterRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/wastage`
- **Backend Method**: `GetAll` in `WastageController.cs`
- **Receives (Parameters)**: `[FromQuery] Guid? itemId, [FromQuery] string? wastageType`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/wastage/{id:int}`
- **Backend Method**: `GetById` in `WastageController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/wastage`
- **Backend Method**: `Record` in `WastageController.cs`
- **Receives (Parameters)**: `[FromBody] RecordWastageRequest request`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/wastage/{id:int}`
- **Backend Method**: `Delete` in `WastageController.cs`
- **Receives (Parameters)**: `int id`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

## WebAuthn Controller

### `POST api/webauthn/makeCredentialOptions`
- **Backend Method**: `MakeCredentialOptions` in `WebAuthnController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/webauthn/makeCredential`
- **Backend Method**: `MakeCredential` in `WebAuthnController.cs`
- **Receives (Parameters)**: `[FromBody] AuthenticatorAttestationRawResponse response, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/webauthn/assertionOptions`
- **Backend Method**: `AssertionOptions` in `WebAuthnController.cs`
- **Receives (Parameters)**: `[FromBody] AssertionOptionsRequest request, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `POST api/webauthn/makeAssertion`
- **Backend Method**: `MakeAssertion` in `WebAuthnController.cs`
- **Receives (Parameters)**: `
        [FromBody] AuthenticatorAssertionRawResponse response, 
        [FromServices] Store.API.Application.Auth.Ports.IAuthPort authPort,
        CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `GET api/webauthn/credentials`
- **Backend Method**: `GetCredentials` in `WebAuthnController.cs`
- **Receives (Parameters)**: `CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

### `DELETE api/webauthn/credentials/{id}`
- **Backend Method**: `DeleteCredential` in `WebAuthnController.cs`
- **Receives (Parameters)**: `int id, CancellationToken ct`
- **Returns**: `IActionResult`
- **Client Call(s)**: No direct matching UI call found (or URL is dynamically constructed).

