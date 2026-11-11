using ChMS.Infrastructure.Audit;
using ChMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChMS.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers the database and audit logging. The caller must register <see cref="ICurrentActor"/>.</summary>
    public static IServiceCollection AddChmsInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDatabase)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditInterceptor>();
        services.AddDbContext<ChmsDbContext>((provider, options) =>
        {
            configureDatabase(options);
            options.AddInterceptors(provider.GetRequiredService<AuditInterceptor>());
        });

        return services;
    }
}
