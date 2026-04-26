using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Currency : BaseEntity
{
    public int CurrencyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
}
