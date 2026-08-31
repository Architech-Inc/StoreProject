using Microsoft.EntityFrameworkCore;

using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.DTOs.Invoices;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Repositories;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _uow;
    private readonly IDiscountService _discountService;

    public InvoiceService(IUnitOfWork uow, IDiscountService discountService)
    {
        _uow = uow;
        _discountService = discountService;
    }

    public async Task<InvoiceDto?> GetByIdAsync(Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer).ThenInclude(c => c!.Phones).ThenInclude(cp => cp.Phone)
            .Include(i => i.Customer).ThenInclude(c => c!.Emails).ThenInclude(ce => ce.Email)
            .Include(i => i.Customer).ThenInclude(c => c!.LoyaltyAccount)
            .Include(i => i.User).ThenInclude(u => u!.Employee)
            .Include(i => i.Branch)
            .Include(i => i.Sales).ThenInclude(s => s.Item).ThenInclude(it => it.Unit)
            .Include(i => i.Tenders)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct);

        return invoice is null ? null : MapToDto(invoice);
    }

    public async Task<PublicReceiptDto?> GetPublicReceiptAsync(Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Include(i => i.User).ThenInclude(u => u!.Employee)
            .Include(i => i.Branch)
            .Include(i => i.Sales).ThenInclude(s => s.Item)
            .Include(i => i.Tenders)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct);

        if (invoice == null) return null;

        var cashierName = invoice.User?.Employee?.FirstName ?? invoice.User?.Username ?? "Staff Cashier";
        var customerName = invoice.Customer?.FullName ?? "Walk-in Customer";
        var subtotal = invoice.Sales.Sum(s => s.Quantity * s.UnitPrice);
        var discount = invoice.Sales.Sum(s => (s.DiscountAmount ?? 0) * s.Quantity);

        var rawSignature = $"{invoice.InvoiceId}:{invoice.TotalAmount}:{invoice.DateCreated:O}";
        var hashBytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawSignature));
        var verificationSig = Convert.ToHexString(hashBytes)[..16];

        return new PublicReceiptDto
        {
            InvoiceId = invoice.InvoiceId,
            BranchName = invoice.Branch?.Name ?? "Main Store - Akwa Douala",
            CashierName = cashierName,
            CustomerName = customerName,
            SubtotalAmount = subtotal,
            DiscountAmount = discount,
            TaxAmount = 0m,
            TotalAmount = invoice.TotalAmount,
            AmountTendered = invoice.AmountTendered,
            ChangeGiven = invoice.ChangeGiven,
            PaymentMethod = invoice.PaymentType.ToString(),
            Status = invoice.IsPaid ? "Completed" : "Pending",
            DateCreated = invoice.DateCreated,
            VerificationSignature = verificationSig,
            Lines = invoice.Sales.Select(s => new PublicReceiptLineDto
            {
                ItemName = s.Item?.Name ?? "Item",
                Quantity = s.Quantity,
                UnitPrice = s.UnitPrice,
                DiscountAmount = s.DiscountAmount ?? 0,
                LineTotal = s.LineTotal
            }).ToList()
        };
    }

    public async Task<PagedResult<InvoiceDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var invReq = request as InvoicePagedRequest ?? new InvoicePagedRequest
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending
        };

        var query = BuildFilteredQuery(invReq);

        var total = await query.CountAsync(ct);

        // Sorting
        query = (invReq.SortBy?.ToLowerInvariant()) switch
        {
            "date_asc" => query.OrderBy(i => i.DateCreated),
            "total_desc" => query.OrderByDescending(i => i.TotalAmount),
            "total_asc" => query.OrderBy(i => i.TotalAmount),
            "balance_desc" => query.OrderByDescending(i => i.TotalAmount - i.AmountTendered),
            _ => query.OrderByDescending(i => i.DateCreated)
        };

        var items = await query
            .Skip((invReq.Page - 1) * invReq.PageSize)
            .Take(invReq.PageSize)
            .ToListAsync(ct);

        return new PagedResult<InvoiceDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = invReq.Page,
            PageSize = invReq.PageSize
        };
    }

    public async Task<InvoiceSummaryMetricsDto> GetSummaryMetricsAsync(InvoicePagedRequest request, CancellationToken ct = default)
    {
        var query = BuildFilteredQuery(request);

        var invoices = await query.Select(i => new
        {
            i.TotalAmount,
            i.AmountTendered,
            i.IsPaid,
            RefundedAmount = i.Sales.Where(s => s.Quantity < 0).Sum(s => -s.LineTotal)
        }).ToListAsync(ct);

        var totalCount = invoices.Count;
        var grossSales = invoices.Sum(i => i.TotalAmount);
        var collected = invoices.Sum(i => i.AmountTendered);
        var outstanding = invoices.Where(i => !i.IsPaid).Sum(i => Math.Max(0, i.TotalAmount - i.AmountTendered));
        var refunded = invoices.Sum(i => i.RefundedAmount);
        var voided = invoices.Where(i => !i.IsPaid && i.AmountTendered == 0 && i.TotalAmount > 0).Sum(i => i.TotalAmount);

        var paidCount = invoices.Count(i => i.IsPaid);
        var unpaidCount = invoices.Count(i => !i.IsPaid && (i.TotalAmount - i.AmountTendered) > 0);
        var refundedCount = invoices.Count(i => i.RefundedAmount > 0);
        var voidedCount = invoices.Count(i => !i.IsPaid && i.AmountTendered == 0 && i.TotalAmount > 0);

        var aov = totalCount > 0 ? Math.Round(grossSales / totalCount, 2) : 0;

        return new InvoiceSummaryMetricsDto
        {
            GrossSales = grossSales,
            CollectedRevenue = collected,
            OutstandingReceivables = outstanding,
            RefundedVolume = refunded,
            VoidedVolume = voided,
            TotalInvoicesCount = totalCount,
            PaidCount = paidCount,
            UnpaidCount = unpaidCount,
            RefundedCount = refundedCount,
            VoidedCount = voidedCount,
            AverageOrderValue = aov
        };
    }

    private IQueryable<Invoice> BuildFilteredQuery(InvoicePagedRequest request)
    {
        var query = _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer).ThenInclude(c => c!.Phones).ThenInclude(cp => cp.Phone)
            .Include(i => i.Customer).ThenInclude(c => c!.Emails).ThenInclude(ce => ce.Email)
            .Include(i => i.User).ThenInclude(u => u!.Employee)
            .Include(i => i.Branch)
            .Include(i => i.Sales)
            .Include(i => i.Tenders)
            .AsNoTracking()
            .AsSplitQuery();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            if (Guid.TryParse(term, out var guid))
            {
                query = query.Where(i => i.InvoiceId == guid);
            }
            else
            {
                query = query.Where(i =>
                    i.InvoiceId.ToString().Contains(term) ||
                    (i.Customer != null && (
                        (i.Customer.FirstName + " " + i.Customer.LastName).Contains(term) ||
                        (i.Customer.MiddleName != null && i.Customer.MiddleName.Contains(term)) ||
                        i.Customer.Phones.Any(p => p.Phone.Number.Contains(term)) ||
                        i.Customer.Emails.Any(e => e.Email.Address.Contains(term))
                    )) ||
                    (i.User != null && (
                        (i.User.Employee != null && (i.User.Employee.FirstName + " " + i.User.Employee.LastName).Contains(term)) ||
                        i.User.Username.Contains(term)
                    )) ||
                    (i.Notes != null && i.Notes.Contains(term)) ||
                    i.Tenders.Any(t => t.Reference != null && t.Reference.Contains(term))
                );
            }
        }

        if (request.FromDate.HasValue)
        {
            var from = request.FromDate.Value.Date;
            query = query.Where(i => i.DateCreated >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = request.ToDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(i => i.DateCreated <= to);
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(i => i.BranchId == request.BranchId.Value);
        }

        if (request.PaymentType.HasValue)
        {
            query = query.Where(i => i.PaymentType == request.PaymentType.Value ||
                                     i.Tenders.Any(t => t.PaymentType == request.PaymentType.Value));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status.ToLowerInvariant() != "all")
        {
            var st = request.Status.ToLowerInvariant();
            if (st == "paid")
            {
                query = query.Where(i => i.IsPaid);
            }
            else if (st == "unpaid")
            {
                query = query.Where(i => !i.IsPaid && (i.TotalAmount - i.AmountTendered) > 0);
            }
            else if (st == "refunded")
            {
                query = query.Where(i => i.Sales.Any(s => s.Quantity < 0));
            }
            else if (st == "voided")
            {
                query = query.Where(i => !i.IsPaid && i.AmountTendered == 0 && i.TotalAmount > 0);
            }
        }

        if (request.MinAmount.HasValue)
        {
            query = query.Where(i => i.TotalAmount >= request.MinAmount.Value);
        }

        if (request.MaxAmount.HasValue)
        {
            query = query.Where(i => i.TotalAmount <= request.MaxAmount.Value);
        }

        return query;
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        int? branchId = null;
        if (actingUserId.HasValue)
        {
            var userRole = await _uow.Repository<UserBranchRole>().Query()
                .FirstOrDefaultAsync(ubr => ubr.UserId == actingUserId.Value, ct);
            branchId = userRole?.BranchId;
        }

        return await _uow.ExecuteStrategyAsync(async () =>
        {
            await _uow.BeginTransactionAsync(ct);
            try
            {
                var invoice = new Invoice
                {
                    InvoiceId = Guid.NewGuid(),
                    UserId = actingUserId,
                    CustomerId = request.CustomerId,
                    BranchId = branchId,
                    PaymentType = request.PaymentType,
                    AmountTendered = request.AmountTendered,
                    Notes = request.Notes,
                    DateCreated = DateTime.UtcNow
                };

                decimal total = 0m;
                var sales = new List<Sale>();

                foreach (var line in request.Lines)
                {
                    var item = await _uow.Repository<Item>().Query()
                        .Include(i => i.Unit)
                        .FirstOrDefaultAsync(i => i.ItemId == line.ItemId, ct)
                        ?? throw new KeyNotFoundException($"Item {line.ItemId} not found.");

                    if (item.InStock < line.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for item '{item.Name}'. Available: {item.InStock}, requested: {line.Quantity}.");

                    var unitPrice = line.OverrideUnitPrice ?? item.UnitPrice;
                    var lineTotal = unitPrice * line.Quantity;

                    var sale = new Sale
                    {
                        SaleId = Guid.NewGuid(),
                        InvoiceId = invoice.InvoiceId,
                        ItemId = item.ItemId,
                        UserId = actingUserId,
                        ItemName = item.Name,
                        UnitAbbreviation = item.Unit?.Abbreviation,
                        UnitPrice = unitPrice,
                        Quantity = line.Quantity,
                        LineTotal = lineTotal
                    };

                    item.InStock -= line.Quantity;
                    _uow.Repository<Item>().Update(item);

                    sales.Add(sale);
                    total += lineTotal;
                }

                invoice.TotalAmount = total;
                invoice.Sales = sales;

                // Add tender record
                var tender = new InvoiceTender
                {
                    InvoiceId = invoice.InvoiceId,
                    PaymentType = request.PaymentType,
                    Amount = request.AmountTendered,
                    DateCreated = DateTime.UtcNow
                };
                invoice.Tenders.Add(tender);

                if (invoice.AmountTendered >= invoice.TotalAmount)
                {
                    invoice.IsPaid = true;
                    invoice.ChangeGiven = invoice.AmountTendered - invoice.TotalAmount;
                }

                await _uow.Repository<Invoice>().AddAsync(invoice, ct);

                // If coupon code provided, record usage
                if (!string.IsNullOrWhiteSpace(request.CouponCode))
                {
                    var discount = await _discountService.ValidateCouponAsync(request.CouponCode.Trim());
                    if (discount != null)
                    {
                        decimal discountAmount = 0;
                        if (discount.FixedAmount.HasValue && discount.FixedAmount.Value > 0)
                        {
                            discountAmount = discount.FixedAmount.Value;
                        }
                        else if (discount.Percentage > 0)
                        {
                            discountAmount = Math.Round(invoice.TotalAmount * (discount.Percentage / 100m), 2);
                        }

                        if (discountAmount > 0)
                        {
                            invoice.TotalAmount = Math.Max(0, invoice.TotalAmount - discountAmount);
                            if (invoice.AmountTendered >= invoice.TotalAmount)
                            {
                                invoice.IsPaid = true;
                                invoice.ChangeGiven = invoice.AmountTendered - invoice.TotalAmount;
                            }
                            await _discountService.IncrementUsageAsync(discount.DiscountId);
                        }
                    }
                }

                await _uow.SaveChangesAsync(ct);
                await _uow.CommitTransactionAsync(ct);

                return await GetByIdAsync(invoice.InvoiceId, ct)
                    ?? throw new InvalidOperationException("Failed to retrieve created invoice.");
            }
            catch
            {
                await _uow.RollbackTransactionAsync(ct);
                throw;
            }
        });
    }

    public Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid? actingUserId, CancellationToken ct = default)
    {
        return VoidInvoiceAsync(invoiceId, actingUserId, null, ct);
    }

    public async Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid? actingUserId, string? reason, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Sales)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct);

        if (invoice is null || !invoice.IsPaid) return false;

        if (actingUserId.HasValue && invoice.BranchId.HasValue)
        {
            var hasAccess = await _uow.Repository<UserBranchRole>().Query()
                .AnyAsync(ubr => ubr.UserId == actingUserId.Value && ubr.BranchId == invoice.BranchId.Value, ct);
            if (!hasAccess) throw new UnauthorizedAccessException("You do not have access to void invoices at this branch.");
        }

        return await _uow.ExecuteStrategyAsync(async () =>
        {
            await _uow.BeginTransactionAsync(ct);
            try
            {
                foreach (var sale in invoice.Sales.Where(s => s.Quantity > 0))
                {
                    var item = await _uow.Repository<Item>().GetByIdAsync(sale.ItemId, ct);
                    if (item is not null)
                    {
                        item.InStock += sale.Quantity;
                        _uow.Repository<Item>().Update(item);
                    }
                }

                invoice.IsPaid = false;
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    invoice.Notes = string.IsNullOrWhiteSpace(invoice.Notes)
                        ? $"[VOIDED: {reason.Trim()}]"
                        : $"{invoice.Notes} [VOIDED: {reason.Trim()}]";
                }

                _uow.Repository<Invoice>().Update(invoice);

                await _uow.SaveChangesAsync(ct);
                await _uow.CommitTransactionAsync(ct);
                return true;
            }
            catch
            {
                await _uow.RollbackTransactionAsync(ct);
                throw;
            }
        });
    }

    public async Task<InvoiceDto?> RefundInvoiceAsync(Guid invoiceId, RefundInvoiceRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Sales)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct);

        if (invoice is null || !invoice.IsPaid) return null;

        if (actingUserId.HasValue && invoice.BranchId.HasValue)
        {
            var hasAccess = await _uow.Repository<UserBranchRole>().Query()
                .AnyAsync(ubr => ubr.UserId == actingUserId.Value && ubr.BranchId == invoice.BranchId.Value, ct);
            if (!hasAccess) throw new UnauthorizedAccessException("You do not have access to refund invoices at this branch.");
        }

        return await _uow.ExecuteStrategyAsync(async () =>
        {
            await _uow.BeginTransactionAsync(ct);
            try
            {
                foreach (var refundLine in request.Lines)
                {
                    var sale = invoice.Sales.FirstOrDefault(s => s.ItemId == refundLine.ItemId && s.Quantity > 0);
                    if (sale is null || sale.Quantity < refundLine.Quantity)
                        throw new InvalidOperationException($"Cannot refund quantity {refundLine.Quantity} for item {refundLine.ItemId}.");

                    if (request.RestockItems)
                    {
                        var item = await _uow.Repository<Item>().GetByIdAsync(refundLine.ItemId, ct);
                        if (item is not null)
                        {
                            item.InStock += refundLine.Quantity;
                            _uow.Repository<Item>().Update(item);
                        }
                    }

                    // Add a negative sale line to reflect the refund
                    var negativeSale = new Sale
                    {
                        SaleId = Guid.NewGuid(),
                        InvoiceId = invoice.InvoiceId,
                        ItemId = sale.ItemId,
                        UserId = actingUserId,
                        ItemName = $"{sale.ItemName} (Return - {request.ReasonCode})",
                        UnitAbbreviation = sale.UnitAbbreviation,
                        UnitPrice = sale.UnitPrice,
                        DiscountAmount = sale.DiscountAmount,
                        Quantity = -refundLine.Quantity,
                        LineTotal = -(Math.Round((sale.UnitPrice - (sale.DiscountAmount ?? 0)) * refundLine.Quantity, 2))
                    };

                    await _uow.Repository<Sale>().AddAsync(negativeSale, ct);
                    invoice.TotalAmount += negativeSale.LineTotal;
                }

                if (!string.IsNullOrWhiteSpace(request.Notes))
                {
                    invoice.Notes = string.IsNullOrWhiteSpace(invoice.Notes)
                        ? $"[RETURN NOTE: {request.Notes.Trim()}]"
                        : $"{invoice.Notes} [RETURN NOTE: {request.Notes.Trim()}]";
                }

                _uow.Repository<Invoice>().Update(invoice);
                await _uow.SaveChangesAsync(ct);
                await _uow.CommitTransactionAsync(ct);

                return await GetByIdAsync(invoiceId, ct);
            }
            catch
            {
                await _uow.RollbackTransactionAsync(ct);
                throw;
            }
        });
    }

    public async Task<InvoiceTenderDto> AddTenderAsync(Guid invoiceId, AddTenderRequest request, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Tenders)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct)
            ?? throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

        if (invoice.IsPaid)
            throw new InvalidOperationException("Invoice is already fully paid.");

        var tender = new InvoiceTender
        {
            InvoiceId = invoiceId,
            PaymentType = request.PaymentType,
            Amount = request.Amount,
            Reference = request.Reference?.Trim(),
            DateCreated = DateTime.UtcNow
        };

        await _uow.Repository<InvoiceTender>().AddAsync(tender, ct);

        invoice.AmountTendered += request.Amount;
        if (invoice.AmountTendered >= invoice.TotalAmount)
        {
            invoice.IsPaid = true;
            invoice.ChangeGiven = Math.Max(0, invoice.AmountTendered - invoice.TotalAmount);
        }
        _uow.Repository<Invoice>().Update(invoice);

        await _uow.SaveChangesAsync(ct);

        return new InvoiceTenderDto
        {
            InvoiceTenderId = tender.InvoiceTenderId,
            PaymentType = tender.PaymentType.ToString(),
            Amount = tender.Amount,
            Reference = tender.Reference,
            DateCreated = tender.DateCreated
        };
    }

    private static InvoiceDto MapToDto(Invoice i)
    {
        var tendersList = i.Tenders?.Select(t => new InvoiceTenderDto
        {
            InvoiceTenderId = t.InvoiceTenderId,
            PaymentType = t.PaymentType.ToString(),
            Amount = t.Amount,
            Reference = t.Reference,
            DateCreated = t.DateCreated
        }).ToList() ?? new List<InvoiceTenderDto>();

        var tenderSummary = tendersList.Count > 1
            ? $"Split ({string.Join(", ", tendersList.Select(t => t.PaymentType).Distinct())})"
            : (tendersList.Count == 1 ? tendersList[0].PaymentType : i.PaymentType.ToString());

        var salesList = i.Sales?.Select(s => new SaleLineDto
        {
            SaleId = s.SaleId,
            ItemId = s.ItemId,
            ItemName = s.ItemName,
            UnitAbbreviation = s.UnitAbbreviation,
            UnitPrice = s.UnitPrice,
            DiscountAmount = s.DiscountAmount,
            Quantity = s.Quantity,
            LineTotal = s.LineTotal
        }).ToList() ?? new List<SaleLineDto>();

        var refundedAmount = salesList.Where(s => s.Quantity < 0).Sum(s => -s.LineTotal);

        var primaryPhone = i.Customer?.Phones.Select(p => p.Phone.Number).FirstOrDefault();
        var primaryEmail = i.Customer?.Emails.Select(e => e.Email.Address).FirstOrDefault();
        var processedByName = i.User?.Employee != null
            ? $"{i.User.Employee.FirstName} {i.User.Employee.LastName}".Trim()
            : i.User?.Username;

        return new InvoiceDto
        {
            InvoiceId = i.InvoiceId,
            CustomerId = i.CustomerId,
            CustomerName = i.Customer is null ? null : $"{i.Customer.FirstName} {i.Customer.LastName}".Trim(),
            CustomerPhone = primaryPhone,
            CustomerEmail = primaryEmail,
            CustomerSegment = i.Customer?.Segment.ToString(),
            UserId = i.UserId,
            ProcessedBy = processedByName,
            BranchId = i.BranchId,
            BranchName = i.Branch?.Name,
            TotalAmount = i.TotalAmount,
            AmountTendered = i.AmountTendered,
            ChangeGiven = i.ChangeGiven,
            OutstandingBalance = i.IsPaid ? 0 : Math.Max(0, i.TotalAmount - i.AmountTendered),
            PaymentType = i.PaymentType,
            TenderSummary = tenderSummary,
            IsPaid = i.IsPaid,
            IsRefunded = refundedAmount > 0,
            RefundedAmount = refundedAmount,
            LinesCount = salesList.Count(s => s.Quantity > 0),
            Notes = i.Notes,
            DateCreated = i.DateCreated,
            Lines = salesList,
            Tenders = tendersList
        };
    }
}
