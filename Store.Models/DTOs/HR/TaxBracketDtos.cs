using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.HR;

public class TaxBracketDto
{
    public int TaxBracketId { get; set; }
    public decimal MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal FixedTaxAmount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateTaxBracketRequest
{
    [Range(0, 999999999)]
    public decimal MinAmount { get; set; }

    [Range(0, 999999999)]
    public decimal? MaxAmount { get; set; }

    [Range(0, 100)]
    public decimal TaxPercentage { get; set; }

    [Range(0, 999999999)]
    public decimal FixedTaxAmount { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateTaxBracketRequest : CreateTaxBracketRequest
{
}
