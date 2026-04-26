using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Document : BaseEntity
{
    public int DocumentId { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? UploadedByUserId { get; set; }
}
