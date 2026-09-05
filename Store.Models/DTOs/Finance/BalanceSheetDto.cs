namespace Store.Models.DTOs.Finance;

public class BalanceSheetDto
{
    public DateTime AsOfDate { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal TotalEquity { get; set; }
    
    public List<AccountBalanceDto> AssetAccounts { get; set; } = new();
    public List<AccountBalanceDto> LiabilityAccounts { get; set; } = new();
    public List<AccountBalanceDto> EquityAccounts { get; set; } = new();
}
