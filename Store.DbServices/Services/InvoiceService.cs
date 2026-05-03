using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
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

    public InvoiceService(IUnitOfWork uow) => _uow = uow;

    public async Task<InvoiceDto?> GetByIdAsync(Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Include(i => i.User)
            .Include(i => i.Sales)
            .Include(i => i.Tenders)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct);

        return invoice is null ? null : MapToDto(invoice);
    }

    public async Task<PagedResult<InvoiceDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Include(i => i.Sales)
            .AsNoTracking();

        var total = await query.CountAsync(ct);
        var invoices = await query
            .OrderByDescending(i => i.DateCreated)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => MapToDto(i))
            .ToListAsync(ct);

        return new PagedResult<InvoiceDto>(invoices, total, request.Page, request.PageSize);
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var invoice = new Invoice
            {
                InvoiceId = Guid.NewGuid(),
                UserId = actingUserId,
                CustomerId = request.CustomerId,
                PaymentType = request.PaymentType,
                AmountTendered = request.AmountTendered,
                Notes = request.Notes?.Trim(),
                IsPaid = true,
                TotalAmount = 0,
                ChangeGiven = 0
            };

            decimal total = 0;

            foreach (var line in request.Lines)
            {
                var item = await _uow.Repository<Item>().Query()
                    .Include(i => i.Unit)
                    .Include(i => i.Discount)
                    .FirstOrDefaultAsync(i => i.ItemId == line.ItemId, ct)
                    ?? throw new InvalidOperationException($"Item {line.ItemId} not found.");

                if (item.InStock < line.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for '{item.Name}'. Available: {item.InStock}.");

                var discountAmount = item.Discount?.IsActive == true
                    ? Math.Round(item.UnitPrice * (item.Discount.Percentage / 100m), 4)
                    : 0m;

                var effectivePrice = item.UnitPrice - discountAmount;
                var lineTotal = Math.Round(effectivePrice * line.Quantity, 2);
                total += lineTotal;

                var sale = new Sale
                {
                    SaleId = Guid.NewGuid(),
                    InvoiceId = invoice.InvoiceId,
                    ItemId = item.ItemId,
                    UserId = actingUserId,
                    ItemName = item.Name,
                    UnitAbbreviation = item.Unit?.Abbreviation,
                    UnitPrice = item.UnitPrice,
                    DiscountAmount = discountAmount,
                    Quantity = line.Quantity,
                    LineTotal = lineTotal
                };

                await _uow.Repository<Sale>().AddAsync(sale, ct);

                // Deduct stock
                item.InStock -= line.Quantity;
                _uow.Repository<Item>().Update(item);
            }

            invoice.TotalAmount = total;
            invoice.ChangeGiven = Math.Max(0, request.AmountTendered - total);

            await _uow.Repository<Invoice>().AddAsync(invoice, ct);
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return (await GetByIdAsync(invoice.InvoiceId, ct))!;
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    public async Task<bool> VoidInvoiceAsync(Guid invoiceId, Guid? actingUserId, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Sales)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId, ct);

        if (invoice is null) return false;
        if (!invoice.IsPaid) return false;  // already voided / not paid

        await _uow.BeginTransactionAsync(ct);
        try
        {
            // Restore stock
            foreach (var sale in invoice.Sales)
            {
                var item = await _uow.Repository<Item>().GetByIdAsync(sale.ItemId, ct);
                if (item is not null)
                {
                    item.InStock += sale.Quantity;
                    _uow.Repository<Item>().Update(item);
                }
            }

            invoice.IsPaid = false;
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
            Reference = request.Reference?.Trim()
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

    private static InvoiceDto MapToDto(Invoice i) => new()
    {
        InvoiceId = i.InvoiceId,
        CustomerId = i.CustomerId,
        CustomerName = i.Customer is null ? null : $"{i.Customer.FirstName} {i.Customer.LastName}".Trim(),
        UserId = i.UserId,
        BranchId = i.BranchId,
        TotalAmount = i.TotalAmount,
        AmountTendered = i.AmountTendered,
        ChangeGiven = i.ChangeGiven,
        OutstandingBalance = i.IsPaid ? 0 : Math.Max(0, i.TotalAmount - i.AmountTendered),
        PaymentType = i.PaymentType,
        IsPaid = i.IsPaid,
        Notes = i.Notes,
        DateCreated = i.DateCreated,
        Lines = i.Sales.Select(s => new SaleLineDto
        {
            SaleId = s.SaleId,
            ItemId = s.ItemId,
            ItemName = s.ItemName,
            UnitAbbreviation = s.UnitAbbreviation,
            UnitPrice = s.UnitPrice,
            DiscountAmount = s.DiscountAmount,
            Quantity = s.Quantity,
            LineTotal = s.LineTotal
        }).ToList(),
        Tenders = i.Tenders.Select(t => new InvoiceTenderDto
        {
            InvoiceTenderId = t.InvoiceTenderId,
            PaymentType = t.PaymentType.ToString(),
            Amount = t.Amount,
            Reference = t.Reference,
            DateCreated = t.DateCreated
        }).ToList()
    };
}
