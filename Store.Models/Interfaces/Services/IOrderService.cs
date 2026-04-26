using Store.Models.DTOs.Common;
using Store.Models.DTOs.Orders;
using Store.Models.Enums;

namespace Store.Models.Interfaces.Services;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default);
    Task<OrderDto> CreateAsync(CreateOrderRequest request, Guid? actingUserId, CancellationToken ct = default);
    Task<bool> ReceiveOrderAsync(Guid orderId, CancellationToken ct = default);
    Task<bool> CancelOrderAsync(Guid orderId, CancellationToken ct = default);
}

public interface IOtpService
{
    Task<string> GenerateAsync(Guid userId, OtpPurpose purpose, CancellationToken ct = default);
    Task<bool> ValidateAsync(Guid userId, string code, OtpPurpose purpose, CancellationToken ct = default);
}
