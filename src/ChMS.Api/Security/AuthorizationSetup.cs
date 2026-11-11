using ChMS.Domain.Security;
using Microsoft.AspNetCore.Authorization;

namespace ChMS.Api.Security;

public static class AuthorizationSetup
{
    public static IServiceCollection AddChmsAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            // Every endpoint requires a signed-in user unless it opts out with [AllowAnonymous].
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(Policies.CanSeeGiving, p => p.RequireRole(Roles.Treasurer, Roles.FinanceAssistant))
            .AddPolicy(Policies.CanReadAuditLog, p => p.RequireRole(Roles.Treasurer, Roles.Administrator))
            .AddPolicy(Policies.CanAdminister, p => p.RequireRole(Roles.Administrator));

        return services;
    }
}
