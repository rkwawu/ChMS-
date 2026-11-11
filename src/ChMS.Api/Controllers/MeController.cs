using System.Security.Claims;
using ChMS.Domain.Security;
using ChMS.Domain.Users;
using ChMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

namespace ChMS.Api.Controllers;

[ApiController]
[Route("api/me")]
public sealed class MeController(ChmsDbContext db, TimeProvider clock, IAuthorizationService authorization) : ControllerBase
{
    public sealed record MeResponse(Guid Id, string DisplayName, string? Email, IReadOnlyList<string> Roles, bool CanSeeGiving);

    /// <summary>Returns the signed-in staff member, creating their user record on first sign-in.</summary>
    [HttpGet]
    public async Task<ActionResult<MeResponse>> Get(CancellationToken cancellationToken)
    {
        var objectId = User.GetObjectId();
        if (string.IsNullOrEmpty(objectId))
        {
            return Forbid();
        }

        var displayName = User.FindFirstValue("name") ?? User.GetDisplayName() ?? "Unknown";
        var email = User.FindFirstValue("preferred_username") ?? User.FindFirstValue(ClaimTypes.Email);
        var now = clock.GetUtcNow();

        var user = await db.Users.SingleOrDefaultAsync(u => u.EntraObjectId == objectId, cancellationToken);
        if (user is null)
        {
            user = AppUser.Create(objectId, displayName, email, now);
            db.Users.Add(user);
        }
        else
        {
            user.RecordSignIn(displayName, email, now);
        }

        await db.SaveChangesAsync(cancellationToken);

        var roles = Roles.All.Where(User.IsInRole).ToList();
        var canSeeGiving = (await authorization.AuthorizeAsync(User, Policies.CanSeeGiving)).Succeeded;

        return new MeResponse(user.Id, user.DisplayName, user.Email, roles, canSeeGiving);
    }
}
