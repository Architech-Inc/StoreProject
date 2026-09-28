namespace Store.Models.Interfaces;

/// <summary>
/// Implemented by entities that should be soft-deleted (logical delete) instead
/// of physically removed. <see cref="StoreDbContext"/> applies a global query
/// filter to every entity that has a required navigation to a soft-deletable
/// parent, so callers never see deleted rows (or orphans of deleted parents).
/// Callers that need to see deleted rows can use <c>IgnoreQueryFilters()</c>.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>True after a soft-delete.</summary>
    bool IsDeleted { get; set; }

    /// <summary>UTC timestamp of the soft-delete.</summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>User who initiated the soft-delete.</summary>
    Guid? DeletedById { get; set; }
}
