using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Customers;

public class CustomerDto
{
    public Guid CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {(MiddleName is not null ? MiddleName + " " : "")}{LastName}".Trim();
    public Gender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? Notes { get; set; }
    public string? ImagePath { get; set; }
    public CustomerSegment Segment { get; set; } = CustomerSegment.Standard;
    public DateTime DateCreated { get; set; }
}

public class CreateCustomerRequest
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? MiddleName { get; set; }

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    public Gender Gender { get; set; } = Gender.NotSpecified;
    public DateTime? DateOfBirth { get; set; }

    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public CustomerSegment Segment { get; set; } = CustomerSegment.Standard;
}

public class UpdateCustomerRequest
{
    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? MiddleName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    public Gender? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public CustomerSegment? Segment { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
