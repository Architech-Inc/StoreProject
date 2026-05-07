using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Contacts;
using Store.Models.Enums;

namespace Store.Models.DTOs.Procurement;

public class SupplierDto
{
    public Guid SupplierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Notes { get; set; }
    public string? ImagePath { get; set; }
    public DateTime DateCreated { get; set; }
    public List<SupplierEmailDto> Emails { get; set; } = new();
    public List<SupplierPhoneDto> Phones { get; set; } = new();
    public List<SupplierLocationDto> Locations { get; set; } = new();
}

public class SupplierEmailDto
{
    public int SupplierEmailId { get; set; }
    public Guid SupplierId { get; set; }
    public string Email { get; set; } = string.Empty;
    public EmailType EmailType { get; set; }
    public bool IsPrimary { get; set; }
}

public class SupplierPhoneDto
{
    public int SupplierPhoneId { get; set; }
    public Guid SupplierId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public PhoneType PhoneType { get; set; }
    public bool IsPrimary { get; set; }
}

public class SupplierLocationDto
{
    public int SupplierLocationId { get; set; }
    public Guid SupplierId { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class CreateSupplierRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public string? ImagePath { get; set; }

    public List<CreateSupplierEmailRequest> Emails { get; set; } = new();
    public List<CreateSupplierPhoneRequest> Phones { get; set; } = new();
    public List<CreateSupplierLocationRequest> Locations { get; set; } = new();
}

public class CreateSupplierEmailRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    public EmailType EmailType { get; set; } = EmailType.Work;
    public bool IsPrimary { get; set; }
}

public class CreateSupplierPhoneRequest
{
    [Required, MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public PhoneType PhoneType { get; set; } = PhoneType.Work;
    public bool IsPrimary { get; set; }
}

public class CreateSupplierLocationRequest
{
    [Required, MaxLength(255)]
    public string AddressLine1 { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? AddressLine2 { get; set; }

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [Required, MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}

public class UpdateSupplierRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public string? ImagePath { get; set; }

    public List<CreateSupplierEmailRequest>? Emails { get; set; }
    public List<CreateSupplierPhoneRequest>? Phones { get; set; }
    public List<CreateSupplierLocationRequest>? Locations { get; set; }
}
