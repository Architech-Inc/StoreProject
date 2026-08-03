namespace Store.Models.DTOs.Common;

/// <summary>
/// Canonical upload result returned by the files API (inside ApiResponse.Data).
/// </summary>
public sealed class FileUploadResultDto
{
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string FullImageUrl { get; set; } = string.Empty;
}
