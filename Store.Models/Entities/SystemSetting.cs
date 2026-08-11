using System.ComponentModel.DataAnnotations;

namespace Store.Models.Entities;

public class SystemSetting
{
    [Key]
    [MaxLength(128)]
    public string SettingKey { get; set; } = string.Empty;

    public string? SettingValue { get; set; }

    [MaxLength(256)]
    public string? Description { get; set; }

    public DateTime LastModified { get; set; }
}
