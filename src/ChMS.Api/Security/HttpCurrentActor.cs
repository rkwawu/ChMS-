using System.Security.Claims;
using ChMS.Infrastructure.Audit;
using Microsoft.Identity.Web;

namespace ChMS.Api.Security;

/// <summary>Reads the signed-in Entra user from the current request for the audit log.</summary>
public sealed class HttpCurrentActor(IHttpContextAccessor accessor) : ICurrentActor
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public string Id => User?.GetObjectId() ?? "system";

    public string? Name => User is null ? "System" : User.FindFirstValue("name") ?? User.GetDisplayName();
}
