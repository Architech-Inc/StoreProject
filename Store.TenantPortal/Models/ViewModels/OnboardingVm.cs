using System.ComponentModel.DataAnnotations;

namespace Store.TenantPortal.Models.ViewModels;

public class OnboardingVm
{
    // Step 1: Admin Credentials
    [Required(ErrorMessage = "Admin username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    [Display(Name = "Admin Username")]
    public string AdminUsername { get; set; } = "admin";

    [Required(ErrorMessage = "Admin password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Admin Password")]
    public string AdminPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the password.")]
    [Compare(nameof(AdminPassword), ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmAdminPassword { get; set; } = string.Empty;

    // Step 2: Store Identity
    [Required(ErrorMessage = "Store name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Store name must be between 2 and 100 characters.")]
    [Display(Name = "Store Name")]
    public string StoreName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Store slug is required.")]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Slug must be between 3 and 50 characters.")]
    [Display(Name = "Store Slug")]
    public string StoreSlug { get; set; } = string.Empty;

    public string Currency { get; set; } = "XAF";

    public int PlanTier { get; set; } = 1; // 0=Starter, 1=Professional, 2=Enterprise

    // Step 3: Domain Selection
    public string DomainChoice { get; set; } = "Platform"; // "Platform" or "Custom"

    [Display(Name = "Custom Domain")]
    public string? CustomDomain { get; set; }
}
