namespace ChMS.Domain.Security;

/// <summary>
/// Staff roles. These are defined as app roles on the API's Microsoft Entra app registration and
/// assigned to people in the Entra admin center, so every grant is recorded in Entra's own audit log.
/// The values must match the app role "Value" fields exactly.
/// </summary>
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Treasurer = "Treasurer";
    public const string FinanceAssistant = "FinanceAssistant";
    public const string Counter = "Counter";
    public const string Staff = "Staff";

    public static readonly IReadOnlyList<string> All =
        [Administrator, Treasurer, FinanceAssistant, Counter, Staff];
}

/// <summary>Authorization policy names used by the API.</summary>
public static class Policies
{
    /// <summary>May see individual giving amounts.</summary>
    public const string CanSeeGiving = nameof(CanSeeGiving);

    /// <summary>May read the audit log.</summary>
    public const string CanReadAuditLog = nameof(CanReadAuditLog);

    /// <summary>May manage users and settings.</summary>
    public const string CanAdminister = nameof(CanAdminister);
}
