using System.Security.Claims;
using System.Text.Encodings.Web;
using ChMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChMS.Api.Tests;

/// <summary>
/// Hosts the API with an in-memory SQLite database and a fake sign-in, so tests don't need
/// SQL Server or Microsoft Entra ID. Tests pick the user with <see cref="CreateClientAs"/>.
/// </summary>
public sealed class ChmsApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public ChmsApiFactory()
    {
        _connection.Open();
    }

    public HttpClient CreateClientAs(string objectId, string name, params string[] roles)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, $"{objectId}|{name}|{string.Join(",", roles)}");
        return client;
    }

    public async Task<T> WithDbAsync<T>(Func<ChmsDbContext, Task<T>> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ChmsDbContext>();
        return await action(db);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.UseEnvironment("Testing");
        builder.UseSetting("AzureAd:TenantId", "00000000-0000-0000-0000-000000000000");
        builder.UseSetting("AzureAd:ClientId", "00000000-0000-0000-0000-000000000000");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ChmsDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ChmsDbContext>>();
            services.AddDbContext<ChmsDbContext>((provider, options) =>
            {
                options.UseSqlite(_connection);
                options.AddInterceptors(provider.GetRequiredService<ChMS.Infrastructure.Audit.AuditInterceptor>());
            });

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            services.PostConfigure<AuthenticationOptions>(o =>
            {
                o.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                o.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<ChmsDbContext>().Database.EnsureCreated();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }

    private sealed class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "Test";
        public const string UserHeader = "X-Test-User";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserHeader, out var header))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var parts = header.ToString().Split('|');
            var claims = new List<Claim>
            {
                new("http://schemas.microsoft.com/identity/claims/objectidentifier", parts[0]),
                new("name", parts[1]),
                new("preferred_username", $"{parts[0]}@example.org"),
            };
            claims.AddRange(parts[2]
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(claims, SchemeName);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
        }
    }
}
