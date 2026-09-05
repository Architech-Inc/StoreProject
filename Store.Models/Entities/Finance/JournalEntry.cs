using Store.Models.Entities.Base;

namespace Store.Models.Entities.Finance;

public class JournalEntry : BaseEntity
{
    public Guid JournalEntryId { get; set; } = Guid.NewGuid();
    
    public DateTime Date { get; set; }
    
    /// <summary>
    /// E.g. Invoice Number or PO Number.
    /// </summary>
    public string? ReferenceId { get; set; }
    
    public ReferenceType ReferenceType { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public Guid? CreatedByUserId { get; set; }
    
    public bool IsPosted { get; set; }
    
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}
