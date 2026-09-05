using Store.Models.Entities.Base;

namespace Store.Models.Entities.Finance;

public class JournalEntryLine : BaseEntity
{
    public Guid JournalEntryLineId { get; set; } = Guid.NewGuid();
    
    public Guid JournalEntryId { get; set; }
    
    public int AccountId { get; set; }
    
    public decimal DebitAmount { get; set; }
    
    public decimal CreditAmount { get; set; }
    
    public string? Description { get; set; }
    
    // Navigation properties
    public JournalEntry? JournalEntry { get; set; }
    public Account? Account { get; set; }
}
