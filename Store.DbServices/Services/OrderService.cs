using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Orders;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    public OrderService(IUnitOfWork uow) => _uow = uow;

    public async Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _uow.Repository<ItemsOrder>().Query()
            .Include(o => o.Supplier)
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ItemsOrderId == orderId, ct);

        return order is null ? null : MapToDto(order);
    }

    public async Task<PagedResult<OrderDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<ItemsOrder>().Query()
            .Include(o => o.Supplier)
            .Include(o => o.Items)
            .AsNoTracking();

        var total = await query.CountAsync(ct);
        var orders = await query
            .OrderByDescending(o => o.DateCreated)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => MapToDto(o))
            .ToListAsync(ct);

        return new PagedResult<OrderDto>(orders, total, request.Page, request.PageSize);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        var orderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var order = new ItemsOrder
        {
            ItemsOrderId = Guid.NewGuid(),
            OrderNumber = orderNumber,
            SupplierId = request.SupplierId,
            CreatedByUserId = actingUserId,
            Status = OrderStatus.Pending,
            Notes = request.Notes?.Trim(),
            TotalAmount = 0
        };

        decimal total = 0;
        foreach (var line in request.Lines)
        {
            var item = await _uow.Repository<Item>().GetByIdAsync(line.ItemId, ct)
                ?? throw new InvalidOperationException($"Item {line.ItemId} not found.");

            var lineTotal = item.CostPrice.HasValue ? item.CostPrice.Value * line.QuantityOrdered : 0;
            total += lineTotal;

            var orderItem = new OrderItem
            {
                ItemsOrderId = order.ItemsOrderId,
                ItemId = line.ItemId,
                ItemName = item.Name,
                QuantityOrdered = line.QuantityOrdered,
                QuantityReceived = 0,
                UnitCost = item.CostPrice,
                LineTotal = lineTotal
            };

            await _uow.Repository<OrderItem>().AddAsync(orderItem, ct);
        }

        order.TotalAmount = total;
        await _uow.Repository<ItemsOrder>().AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        return (await GetByIdAsync(order.ItemsOrderId, ct))!;
    }

    public async Task<bool> ReceiveOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _uow.Repository<ItemsOrder>().Query()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.ItemsOrderId == orderId, ct);

        if (order is null) return false;

        return await _uow.ExecuteStrategyAsync(async () =>
        {
            await _uow.BeginTransactionAsync(ct);
            try
            {
                foreach (var line in order.Items)
                {
                    var item = await _uow.Repository<Item>().GetByIdAsync(line.ItemId, ct);
                    if (item is not null)
                    {
                        var toReceive = line.QuantityOrdered - line.QuantityReceived;
                        item.InStock += toReceive;
                        line.QuantityReceived = line.QuantityOrdered;
                        _uow.Repository<Item>().Update(item);
                        _uow.Repository<OrderItem>().Update(line);
                    }
                }

                order.Status = OrderStatus.Received;
                _uow.Repository<ItemsOrder>().Update(order);

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

    public async Task<bool> CancelOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _uow.Repository<ItemsOrder>().Query()
            .FirstOrDefaultAsync(o => o.ItemsOrderId == orderId, ct);

        if (order is null || order.Status == OrderStatus.Received) return false;

        order.Status = OrderStatus.Cancelled;
        _uow.Repository<ItemsOrder>().Update(order);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private static OrderDto MapToDto(ItemsOrder o) => new()
    {
        OrderId = o.ItemsOrderId,
        OrderNumber = o.OrderNumber,
        SupplierId = o.SupplierId,
        SupplierName = o.Supplier?.Name,
        Status = o.Status,
        TotalAmount = o.TotalAmount,
        Notes = o.Notes,
        DateCreated = o.DateCreated,
        Lines = o.Items.Select(i => new OrderLineDto
        {
            OrderItemId = i.OrderItemId,
            ItemId = i.ItemId,
            ItemName = i.ItemName,
            QuantityOrdered = i.QuantityOrdered,
            QuantityReceived = i.QuantityReceived,
            UnitCost = i.UnitCost,
            LineTotal = i.LineTotal
        }).ToList()
    };
}
