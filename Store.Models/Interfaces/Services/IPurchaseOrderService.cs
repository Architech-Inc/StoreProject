using Store.Models.DTOs.Procurement;
using Store.Models.Enums;

namespace Store.Models.Interfaces.Services;

public interface IPurchaseOrderService
{
    Task<List<PurchaseOrderDto>> GetAllAsync(PurchaseOrderStatus? status = null, Guid? supplierId = null);

    Task<PurchaseOrderDto?> GetByIdAsync(int id);

    /// <summary>Creates a PO in Draft status.</summary>
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequest request, Guid requestedByUserId);

    /// <summary>Submits a Draft PO for approval.</summary>
    Task<PurchaseOrderDto?> SubmitAsync(int id, Guid userId);

    /// <summary>Manager approves a Submitted PO.</summary>
    Task<PurchaseOrderDto?> ApproveAsync(int id, Guid approvedByUserId);

    /// <summary>
    /// Receives goods: updates ReceivedQuantity per line, updates Item.InStock,
    /// writes StockMovement (Receive), and advances PO status to PartiallyReceived or Received.
    /// </summary>
    Task<PurchaseOrderDto?> ReceiveAsync(int id, ReceivePurchaseOrderRequest request, Guid receivedByUserId);

    /// <summary>Cancels a Draft or Submitted PO.</summary>
    Task<PurchaseOrderDto?> CancelAsync(int id, Guid userId);
}
