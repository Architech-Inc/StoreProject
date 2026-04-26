using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Language : BaseEntity
{
    public int LanguageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
