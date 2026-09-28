using Store.Models.Interfaces;

namespace Store.Models.Entities.Base;

/// <summary>
/// Base class for all persistent entities. Provides timestamps, audit fields, and a
/// soft-delete contract. The query filter is registered per-entity in
/// <see cref="StoreDbContext.OnModelCreating"/> on a curated list of business
/// entities (Item, Supplier, Employee, Customer) plus their dependent child
/// entities (added automatically via reflection so EF Core 10622 warnings go away).
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
