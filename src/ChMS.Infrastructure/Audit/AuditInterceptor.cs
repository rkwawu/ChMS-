using System.Reflection;
using System.Text.Json;
using ChMS.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ChMS.Infrastructure.Audit;

/// <summary>
/// Writes an <see cref="AuditLogEntry"/> for every create, change and delete of an <see cref="IAudited"/>
/// entity, in the same transaction as the change. Rejects any attempt to change or delete audit rows.
/// </summary>
public sealed class AuditInterceptor(ICurrentActor actor, TimeProvider clock) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        AddAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData);
        AddAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditEntries(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker.Entries<AuditLogEntry>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                throw new InvalidOperationException("Audit log entries can't be changed or deleted.");
            }
        }

        var now = clock.GetUtcNow();
        var newEntries = new List<AuditLogEntry>();

        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.Entity is IAudited))
        {
            var auditEntry = entry.State switch
            {
                EntityState.Added => Build(entry, AuditAction.Created, before: null, after: Snapshot(entry, current: true), now),
                EntityState.Deleted => Build(entry, AuditAction.Deleted, before: Snapshot(entry, current: false), after: null, now),
                EntityState.Modified when HasAuditedChanges(entry) =>
                    Build(entry, AuditAction.Updated, Snapshot(entry, current: false), Snapshot(entry, current: true), now),
                _ => null,
            };

            if (auditEntry is not null)
            {
                newEntries.Add(auditEntry);
            }
        }

        context.Set<AuditLogEntry>().AddRange(newEntries);
    }

    private AuditLogEntry Build(EntityEntry entry, AuditAction action, string? before, string? after, DateTimeOffset now)
    {
        var key = entry.Metadata.FindPrimaryKey()
            ?? throw new InvalidOperationException($"{entry.Metadata.Name} has no primary key.");
        var recordId = string.Join(",", key.Properties.Select(p => entry.Property(p.Name).CurrentValue));

        return new AuditLogEntry
        {
            OccurredAt = now,
            ActorId = actor.Id,
            ActorName = actor.Name,
            RecordType = entry.Metadata.ClrType.Name,
            RecordId = recordId,
            Action = action,
            Before = before,
            After = after,
        };
    }

    private static bool HasAuditedChanges(EntityEntry entry) =>
        entry.Properties.Any(p => p.IsModified && !IsIgnored(p));

    private static string Snapshot(EntityEntry entry, bool current)
    {
        var values = entry.Properties
            .Where(p => !IsIgnored(p))
            .ToDictionary(p => p.Metadata.Name, p => current ? p.CurrentValue : p.OriginalValue);
        return JsonSerializer.Serialize(values, JsonOptions);
    }

    private static bool IsIgnored(PropertyEntry property) =>
        property.Metadata.PropertyInfo?.GetCustomAttribute<AuditIgnoreAttribute>() is not null;
}
