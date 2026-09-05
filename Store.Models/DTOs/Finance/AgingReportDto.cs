namespace Store.Models.DTOs.Finance;

public class AgingReportDto
{
    public DateTime AsOfDate { get; set; } = DateTime.UtcNow;
    public string ReportType { get; set; } = string.Empty; // "AR" or "AP"
    
    public decimal TotalCurrent { get; set; }
    public decimal Total1To30Days { get; set; }
    public decimal Total31To60Days { get; set; }
    public decimal Total61To90Days { get; set; }
    public decimal TotalOver90Days { get; set; }
    public decimal TotalAmount => TotalCurrent + Total1To30Days + Total31To60Days + Total61To90Days + TotalOver90Days;

    public List<AgingItemDto> Items { get; set; } = new List<AgingItemDto>();
}

public class AgingItemDto
{
    public Guid EntityId { get; set; }
    public int? IntEntityId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty; // Customer or Supplier name
    
    public DateTime? DueDate { get; set; }
    public int DaysOverdue { get; set; }
    
    public decimal Amount { get; set; }
    
    // Aging buckets
    public decimal Current { get; set; }
    public decimal Days1To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Over90Days { get; set; }
}
