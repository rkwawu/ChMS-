namespace ChMS.Infrastructure.Audit;

/// <summary>Who is making the current change, for the audit log.</summary>
public interface ICurrentActor
{
    /// <summary>Entra object ID, or "system" when no user is signed in.</summary>
    string Id { get; }

    string? Name { get; }
}

public sealed class SystemActor : ICurrentActor
{
    public string Id => "system";

    public string? Name => "System";
}
