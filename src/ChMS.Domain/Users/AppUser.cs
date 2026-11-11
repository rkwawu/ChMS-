using ChMS.Domain.Audit;

namespace ChMS.Domain.Users;

/// <summary>
/// A staff member who has signed in with Microsoft 365. Roles are not stored here; they come from
/// the Entra token on each request.
/// </summary>
public sealed class AppUser : IAudited
{
    private AppUser()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>The user's object ID in Microsoft Entra ID (the "oid" claim).</summary>
    public string EntraObjectId { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    [AuditIgnore]
    public DateTimeOffset LastSignInAt { get; private set; }

    public static AppUser Create(string entraObjectId, string displayName, string? email, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entraObjectId);

        return new AppUser
        {
            Id = Guid.CreateVersion7(),
            EntraObjectId = entraObjectId,
            DisplayName = displayName,
            Email = email,
            LastSignInAt = now,
        };
    }

    public void RecordSignIn(string displayName, string? email, DateTimeOffset now)
    {
        DisplayName = displayName;
        Email = email;
        LastSignInAt = now;
    }
}
