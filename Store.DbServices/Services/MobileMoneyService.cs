using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.Models.DTOs.Payments;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class MobileMoneyService : IMobileMoneyService
{
    private readonly IUnitOfWork _uow;
    private readonly StoreDbContext _db;

    public MobileMoneyService(IUnitOfWork uow, StoreDbContext db)
    {
        _uow = uow;
        _db = db;
    }

    public async Task<MobileMoneyTransactionDto> InitiateAsync(InitiateMobileMoneyRequest request, CancellationToken ct = default)
    {
        var tx = new MobileMoneyTransaction
        {
            MobileMoneyTransactionId = Guid.NewGuid(),
            InvoiceId = request.InvoiceId,
            Provider = request.Provider,
            PhoneNumber = request.PhoneNumber.Trim(),
            Amount = request.Amount,
            Status = MobileMoneyStatus.Pending
        };

        await _uow.Repository<MobileMoneyTransaction>().AddAsync(tx, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToDto(tx);
    }

    public async Task<MobileMoneyTransactionDto?> HandleMtnMomoCallbackAsync(MtnMomoCallbackRequest callback, CancellationToken ct = default)
    {
        var providerTxId = callback.FinancialTransactionId;
        var externalId = callback.ExternalId;

        // Resolve by providerTransactionId (idempotency) or ExternalId (our invoice guid)
        MobileMoneyTransaction? tx = null;

        if (!string.IsNullOrEmpty(providerTxId))
            tx = await _db.MobileMoneyTransactions.FirstOrDefaultAsync(t => t.ProviderTransactionId == providerTxId, ct);

        if (tx is null && Guid.TryParse(externalId, out var invoiceId))
            tx = await _db.MobileMoneyTransactions
                .Where(t => t.InvoiceId == invoiceId && t.Provider == MobileMoneyProvider.MtnMomo && t.Status == MobileMoneyStatus.Pending)
                .OrderByDescending(t => t.DateCreated)
                .FirstOrDefaultAsync(ct);

        if (tx is null) return null;

        // Idempotency: already processed
        if (tx.Status != MobileMoneyStatus.Pending) return MapToDto(tx);

        var isSuccess = string.Equals(callback.Status, "SUCCESSFUL", StringComparison.OrdinalIgnoreCase);
        tx.Status = isSuccess ? MobileMoneyStatus.Completed : MobileMoneyStatus.Failed;
        tx.ProviderTransactionId = providerTxId ?? tx.ProviderTransactionId;
        tx.CompletedAtUtc = DateTime.UtcNow;
        tx.CallbackPayload = System.Text.Json.JsonSerializer.Serialize(callback);
        tx.LastModified = DateTime.UtcNow;

        _uow.Repository<MobileMoneyTransaction>().Update(tx);
        await _uow.SaveChangesAsync(ct);

        return MapToDto(tx);
    }

    public async Task<MobileMoneyTransactionDto?> HandleOrangeMoneyCallbackAsync(OrangeMoneyCallbackRequest callback, CancellationToken ct = default)
    {
        var providerTxId = callback.TransactionId;

        MobileMoneyTransaction? tx = null;

        if (!string.IsNullOrEmpty(providerTxId))
            tx = await _db.MobileMoneyTransactions.FirstOrDefaultAsync(t => t.ProviderTransactionId == providerTxId, ct);

        if (tx is null && Guid.TryParse(callback.InvoiceId, out var invoiceId))
            tx = await _db.MobileMoneyTransactions
                .Where(t => t.InvoiceId == invoiceId && t.Provider == MobileMoneyProvider.OrangeMoney && t.Status == MobileMoneyStatus.Pending)
                .OrderByDescending(t => t.DateCreated)
                .FirstOrDefaultAsync(ct);

        if (tx is null) return null;

        if (tx.Status != MobileMoneyStatus.Pending) return MapToDto(tx);

        var isSuccess = string.Equals(callback.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase);
        tx.Status = isSuccess ? MobileMoneyStatus.Completed : MobileMoneyStatus.Failed;
        tx.ProviderTransactionId = providerTxId ?? tx.ProviderTransactionId;
        tx.CompletedAtUtc = DateTime.UtcNow;
        tx.CallbackPayload = System.Text.Json.JsonSerializer.Serialize(callback);
        tx.LastModified = DateTime.UtcNow;

        _uow.Repository<MobileMoneyTransaction>().Update(tx);
        await _uow.SaveChangesAsync(ct);

        return MapToDto(tx);
    }

    public async Task<SettlementReportDto> GetSettlementReportAsync(DateTime fromDateUtc, DateTime toDateUtc, CancellationToken ct = default)
    {
        var toDateEnd = toDateUtc.Date.AddDays(1).AddTicks(-1);

        // Aggregate InvoiceTenders by PaymentType within the date range
        var tenders = await _db.InvoiceTenders
            .Include(t => t.Invoice)
            .Where(t => t.Invoice.DateCreated >= fromDateUtc.Date && t.Invoice.DateCreated <= toDateEnd && t.Invoice.IsPaid)
            .GroupBy(t => t.PaymentType)
            .Select(g => new ChannelSettlementDto
            {
                PaymentType = g.Key,
                Channel = g.Key.ToString(),
                TotalAmount = g.Sum(t => t.Amount),
                InvoiceCount = g.Select(t => t.InvoiceId).Distinct().Count()
            })
            .ToListAsync(ct);

        // Also count invoices that still use the legacy single-tender model (no InvoiceTender rows)
        var invoicesWithTenders = await _db.InvoiceTenders
            .Where(t => t.Invoice.DateCreated >= fromDateUtc.Date && t.Invoice.DateCreated <= toDateEnd && t.Invoice.IsPaid)
            .Select(t => t.InvoiceId)
            .Distinct()
            .CountAsync(ct);

        var allInvoices = await _db.Invoices
            .Where(i => i.DateCreated >= fromDateUtc.Date && i.DateCreated <= toDateEnd && i.IsPaid)
            .ToListAsync(ct);

        // For invoices with no tender lines, group by their PaymentType directly
        var invoicesWithNoTenders = allInvoices
            .Where(i => !_db.InvoiceTenders.Any(t => t.InvoiceId == i.InvoiceId))
            .ToList();

        foreach (var group in invoicesWithNoTenders.GroupBy(i => i.PaymentType))
        {
            var existing = tenders.FirstOrDefault(t => t.PaymentType == group.Key);
            if (existing is not null)
            {
                existing.TotalAmount += group.Sum(i => i.TotalAmount);
                existing.InvoiceCount += group.Count();
            }
            else
            {
                tenders.Add(new ChannelSettlementDto
                {
                    PaymentType = group.Key,
                    Channel = group.Key.ToString(),
                    TotalAmount = group.Sum(i => i.TotalAmount),
                    InvoiceCount = group.Count()
                });
            }
        }

        // Pending mobile money transactions
        var pendingMoMo = await _db.MobileMoneyTransactions
            .Where(t => t.Status == MobileMoneyStatus.Pending && t.DateCreated >= fromDateUtc.Date && t.DateCreated <= toDateEnd)
            .OrderByDescending(t => t.DateCreated)
            .Select(t => MapToDto(t))
            .ToListAsync(ct);

        return new SettlementReportDto
        {
            FromDate = fromDateUtc.Date,
            ToDate = toDateUtc.Date,
            TotalInvoices = allInvoices.Count,
            TotalSales = allInvoices.Sum(i => i.TotalAmount),
            ByChannel = tenders.OrderByDescending(t => t.TotalAmount).ToList(),
            PendingMobileMoneyTransactions = pendingMoMo
        };
    }

    public async Task<IReadOnlyList<MobileMoneyTransactionDto>> GetTransactionsAsync(int page, int pageSize, MobileMoneyStatus? status = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _db.MobileMoneyTransactions.AsNoTracking().AsQueryable();

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        var rows = await query
            .OrderByDescending(t => t.DateCreated)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => MapToDto(t))
            .ToListAsync(ct);

        return rows;
    }

    private static MobileMoneyTransactionDto MapToDto(MobileMoneyTransaction t) => new()
    {
        MobileMoneyTransactionId = t.MobileMoneyTransactionId,
        InvoiceId = t.InvoiceId,
        Provider = t.Provider == MobileMoneyProvider.MtnMomo ? "MTN MoMo" : "Orange Money",
        PhoneNumber = t.PhoneNumber,
        Amount = t.Amount,
        Status = t.Status.ToString(),
        ProviderTransactionId = t.ProviderTransactionId,
        CompletedAtUtc = t.CompletedAtUtc,
        DateCreated = t.DateCreated
    };
}
