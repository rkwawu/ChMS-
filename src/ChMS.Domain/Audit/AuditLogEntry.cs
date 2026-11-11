namespace ChMS.Domain.Audit;

/// <summary>Marks an entity whose every create, change and delete is written to the audit log.</summary>
#pragma warning disable CA1040 // Marker interface is intentional.
public interface IAudited;
#pragma warning restore CA1040

/// <summary>Excludes a property from audit comparisons, e.g. a last-seen timestamp.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class AuditIgnoreAttribute : Attribute;

public enum AuditAction
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
}

/// <summary>
/// One append-only audit record. Rows are never updated or deleted; the persistence layer rejects both.
/// </summary>
public sealed class AuditLogEntry
{
    public long Id { get; private set; }

    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>Entra object ID of the signed-in user, or "system" for background work.</summary>
    public required string ActorId { get; init; }

    public string? ActorName { get; init; }

    public required string RecordType { get; init; }

    public required string RecordId { get; init; }

    public AuditAction Action { get; init; }

    /// <summary>JSON of the audited property values before the change; null for creates.</summary>
    public string? Before { get; init; }

    /// <summary>JSON of the audited property values after the change; null for deletes.</summary>
    public string? After { get; init; }

    public string? Reason { get; init; }
}
