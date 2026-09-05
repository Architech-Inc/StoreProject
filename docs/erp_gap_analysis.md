# StoreProject ERP Gap Analysis

This document analyzes the current state of the StoreProject codebase against the defining pillars of an Enterprise Resource Planning (ERP) system. The goal is to determine what is currently implemented, what is missing, and how to bridge the gap to evolve the platform from a POS/Inventory system into a fully-fledged ERP.

---

## 1. Core Finance

**Definition:** Automatically updating a general ledger, managing accounts payable/receivable, and generating profit/loss statements.

### Current State:
- **What Exists:** `Invoice`, `PurchaseOrder`, `Sale`, `CashVarianceRecord`, `CashierShift`, `LoyaltyTransaction`, `StockMovement`, `Expense` (limited/missing tracking), basic unit pricing on `Item`.
- **The Gap:** The system tracks transactional events (a sale was made, an invoice was paid) but lacks a double-entry accounting engine. There is no concept of Chart of Accounts, Journal Entries, or Ledgers. Financial reporting (P&L, Balance Sheet) cannot be generated dynamically because Cost of Goods Sold (COGS) is not tracked via FIFO/LIFO layer costing, and operational expenses (rent, payroll, utilities) are not captured into a ledger.

### Path to ERP:
1. **General Ledger Engine:** Introduce `Account` (Chart of Accounts), `JournalEntry`, and `JournalEntryLine` models.
2. **Automated Posting:** Every financial action (e.g., creating an invoice, receiving a PO, voiding a sale, adjusting stock) must automatically post balanced debit/credit entries to the General Ledger.
3. **AP/AR Tracking:** Upgrade `PurchaseOrder` to link to Accounts Payable and `Invoice` to link to Accounts Receivable ledgers.
4. **Financial Reporting:** Build engines to aggregate ledger entries by date range to generate standard Income Statements (P&L) and Balance Sheets.

---

## 2. Supply Chain & Procurement

**Definition:** Automatically generating and sending purchase orders to external suppliers when stock hits a critical threshold.

### Current State:
- **What Exists:** `PurchaseOrder`, `PurchaseOrderItem`, `Supplier`, `StockMovement`, `Item` (with basic stock tracking).
- **The Gap:** Purchase orders are entirely manual. While the system knows current stock levels, it lacks automated triggers, reorder point configurations per item/branch, and a background processing engine to autonomously draft and dispatch POs via email to suppliers.

### Path to ERP:
1. **Inventory Min/Max Thresholds:** Add `ReorderPoint`, `ReorderQuantity`, and `LeadTimeDays` to the `BranchItemStock` entity.
2. **Automated Procurement Engine:** Implement a background worker (e.g., using Hangfire, Quartz, or a simple HostedService) that scans inventory levels daily against reorder points.
3. **Auto-Drafting & Dispatch:** When stock dips below the reorder point, the engine groups items by `Supplier`, drafts a `PurchaseOrder`, and automatically sends an email to the supplier (or alerts a manager for 1-click approval).

---

## 3. Human Resources & Payroll

**Definition:** Tracking employee shifts, calculating sales commissions, and handling payroll directly within the same system.

### Current State:
- **What Exists:** `Employee`, `User`, `Role`, `Salary`, `PersonnelTransferHistory`, `CashierShift` (specifically for POS till management).
- **The Gap:** There is no comprehensive time and attendance system, no commission tracking engine tied to sales, and no payroll execution system to calculate gross pay, taxes, deductions, and net pay.

### Path to ERP:
1. **Time & Attendance:** Introduce `EmployeeShift` and `TimePunch` models for clock-in/clock-out tracking, distinct from POS cashier shifts.
2. **Commission Engine:** Add `CommissionRule` models (e.g., flat rate or percentage based on item/category) and link them to `Sale` records, accumulating into a `CommissionLedger`.
3. **Payroll Processing:** Build a `PayrollRun` entity that aggregates base salary (from `Salary`), hours worked (from `TimePunch`), and commissions, while applying configurable `TaxProfile` and deduction rules to generate payslips.

---

## 4. Multi-Warehouse Logistics

**Definition:** Managing and routing inventory across multiple physical locations based on complex demand forecasting.

### Current State:
- **What Exists:** `Location`, `Branch`, `StockTransfer`, `StockTransferItem`, `BranchItemStock`, `StockMovement`.
- **The Gap:** The system can manually move stock between branches (transfers), but it cannot intelligently forecast demand, recommend optimal transfer routes, or automatically trigger cross-docking operations based on historical sales velocity.

### Path to ERP:
1. **Advanced Fulfillment Logic:** Expand `StockTransfer` to support dynamic routing (e.g., Warehouse A to Branch B).
2. **Demand Forecasting Algorithm:** Implement analytics that calculate historical sales velocity (e.g., units sold per day over 30/60/90 days) and factor in lead time to generate `RestockRecommendation` records.
3. **Automated Replenishment:** Allow the system to automatically generate internal `StockTransfer` drafts to move excess inventory from a central warehouse to high-demand retail branches before stockouts occur.
