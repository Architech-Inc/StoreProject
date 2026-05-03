using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Suppliers;

public class SupplierDto
{
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Notes { get; set; }
}

public class CreateSupplierRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? RegistrationNumber { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class UpdateSupplierRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? RegistrationNumber { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
