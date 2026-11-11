using System.Net;
using System.Net.Http.Json;
using ChMS.Domain.Audit;
using ChMS.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace ChMS.Api.Tests;

public sealed class ApiTests : IClassFixture<ChmsApiFactory>
{
    private readonly ChmsApiFactory _factory;

    public ApiTests(ChmsApiFactory factory)
    {
        _factory = factory;
    }

    private sealed record Me(Guid Id, string DisplayName, string? Email, List<string> Roles, bool CanSeeGiving);

    [Fact]
    public async Task Health_check_needs_no_sign_in()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Api_rejects_anonymous_requests()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(new Uri("/api/me", UriKind.Relative));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task First_sign_in_creates_user_and_audit_entry()
    {
        using var client = _factory.CreateClientAs("oid-treasurer", "Grace Treasurer", Roles.Treasurer);

        var me = await client.GetFromJsonAsync<Me>(new Uri("/api/me", UriKind.Relative));

        Assert.NotNull(me);
        Assert.Equal("Grace Treasurer", me.DisplayName);
        Assert.Equal([Roles.Treasurer], me.Roles);
        Assert.True(me.CanSeeGiving);

        var created = await _factory.WithDbAsync(db => db.AuditLog
            .Where(a => a.RecordType == "AppUser" && a.RecordId == me.Id.ToString())
            .ToListAsync());
        var entry = Assert.Single(created);
        Assert.Equal(AuditAction.Created, entry.Action);
        Assert.Equal("oid-treasurer", entry.ActorId);
    }

    [Fact]
    public async Task Repeat_sign_in_does_not_add_audit_noise()
    {
        using var client = _factory.CreateClientAs("oid-repeat", "Repeat Person", Roles.Staff);

        var me = await client.GetFromJsonAsync<Me>(new Uri("/api/me", UriKind.Relative));
        await client.GetFromJsonAsync<Me>(new Uri("/api/me", UriKind.Relative));

        var count = await _factory.WithDbAsync(db => db.AuditLog.CountAsync(a => a.RecordId == me!.Id.ToString()));
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task Staff_cannot_see_giving()
    {
        using var client = _factory.CreateClientAs("oid-staff", "Sam Staff", Roles.Staff);

        var me = await client.GetFromJsonAsync<Me>(new Uri("/api/me", UriKind.Relative));

        Assert.False(me!.CanSeeGiving);
    }

    [Theory]
    [InlineData(Roles.Counter, HttpStatusCode.Forbidden)]
    [InlineData(Roles.Staff, HttpStatusCode.Forbidden)]
    [InlineData(Roles.Treasurer, HttpStatusCode.OK)]
    [InlineData(Roles.Administrator, HttpStatusCode.OK)]
    public async Task Audit_log_is_limited_to_treasurer_and_administrator(string role, HttpStatusCode expected)
    {
        using var client = _factory.CreateClientAs($"oid-{role}", role, role);

        var response = await client.GetAsync(new Uri("/api/audit-log", UriKind.Relative));

        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task Audit_entries_cannot_be_changed_or_deleted()
    {
        using var client = _factory.CreateClientAs("oid-tamper", "Tamper Test", Roles.Staff);
        await client.GetAsync(new Uri("/api/me", UriKind.Relative));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _factory.WithDbAsync(async db =>
        {
            var entry = await db.AuditLog.FirstAsync(a => a.ActorId == "oid-tamper");
            db.AuditLog.Remove(entry);
            return await db.SaveChangesAsync();
        }));
    }
}
