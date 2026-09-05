using Store.Models.Entities.Base;

namespace Store.Models.Entities.HR;

public class TaxBracket : BaseEntity
{
    public int TaxBracketId { get; set; }
    
    // Limits
    public decimal MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    
    // Tax rules
    public decimal TaxPercentage { get; set; }
    public decimal FixedTaxAmount { get; set; }
    
    public bool IsActive { get; set; } = true;
}
