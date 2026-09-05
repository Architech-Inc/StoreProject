using Store.Models.Entities.Base;

namespace Store.Models.Entities.Finance;

public class Account : BaseEntity
{
    public int AccountId { get; set; }
    
    /// <summary>
    /// E.g., "1000", "4000"
    /// </summary>
    public string AccountCode { get; set; } = string.Empty;
    
    /// <summary>
    /// E.g., "Cash", "Sales Revenue"
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    public AccountType AccountType { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public string? Description { get; set; }
    
    // Navigation properties
    public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}
