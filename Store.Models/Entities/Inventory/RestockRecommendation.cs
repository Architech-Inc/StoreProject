using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities.Inventory;

public class RestockRecommendation : BaseEntity
{
    [Key]
    public Guid RecommendationId { get; set; } = Guid.NewGuid();
    
    public int BranchId { get; set; }
    public Guid ItemId { get; set; }
    
    public int RecommendedQuantity { get; set; }
    
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
    
    public RestockRecommendationStatus Status { get; set; } = RestockRecommendationStatus.Pending;
    
    // For tracing which action was taken
    public int? GeneratedStockTransferId { get; set; }
    public int? GeneratedPurchaseOrderId { get; set; }
    
    // Navigation
    public Branch Branch { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public StockTransfer? GeneratedStockTransfer { get; set; }
    public PurchaseOrder? GeneratedPurchaseOrder { get; set; }
}
