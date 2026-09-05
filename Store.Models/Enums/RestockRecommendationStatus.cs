namespace Store.Models.Enums;

public enum RestockRecommendationStatus
{
    Pending = 0,
    ConvertedToTransfer = 1,
    ConvertedToPurchaseOrder = 2,
    Dismissed = 3
}
