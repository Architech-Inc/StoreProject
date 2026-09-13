using Store.Models.Interfaces;

namespace Store.Models.Entities.Base;

/// <summary>
/// Base class for all persistent entities. Provides timestamps, audit fields, and a
/// soft-delete contract. <see cref="StoreDbContext"/> applies a global query filter
/// to every entity that derives from this class so callers never see deleted rows.
/// </summary>
public abstract class BaseEntity : ISoftDeletable
{
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of the soft-delete, or <c>null</c> if not deleted.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>User who initiated the soft-delete, or <c>null</c>.</summary>
    public Guid? DeletedById { get; set; }

    /// <summary>True after a soft-delete; default false.</summary>
    public bool IsDeleted { get; set; }
}
