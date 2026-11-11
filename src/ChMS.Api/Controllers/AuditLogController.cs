using ChMS.Domain.Audit;
using ChMS.Domain.Security;
using ChMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChMS.Api.Controllers;

[ApiController]
[Route("api/audit-log")]
[Authorize(Policy = Policies.CanReadAuditLog)]
public sealed class AuditLogController(ChmsDbContext db) : ControllerBase
{
    public sealed record AuditLogItem(
        long Id,
        DateTimeOffset OccurredAt,
        string ActorId,
        string? ActorName,
        string RecordType,
        string RecordId,
        AuditAction Action,
        string? Before,
        string? After,
        string? Reason);

    /// <summary>Most recent audit entries first, optionally filtered to one record.</summary>
    [HttpGet]
    public async Task<IReadOnlyList<AuditLogItem>> Get(
        [FromQuery] string? recordType,
        [FromQuery] string? recordId,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 500);

        var query = db.AuditLog.AsNoTracking();
        if (!string.IsNullOrEmpty(recordType))
        {
            query = query.Where(a => a.RecordType == recordType);
        }

        if (!string.IsNullOrEmpty(recordId))
        {
            query = query.Where(a => a.RecordId == recordId);
        }

        return await query
            .OrderByDescending(a => a.Id)
            .Take(take)
            .Select(a => new AuditLogItem(
                a.Id, a.OccurredAt, a.ActorId, a.ActorName, a.RecordType, a.RecordId, a.Action, a.Before, a.After, a.Reason))
            .ToListAsync(cancellationToken);
    }
}
