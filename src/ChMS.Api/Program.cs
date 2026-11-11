using ChMS.Api.Security;
using ChMS.Infrastructure;
using ChMS.Infrastructure.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Staff sign in with Microsoft 365. The API validates Entra ID access tokens and reads app roles from them.
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");
builder.Services.AddChmsAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentActor, HttpCurrentActor>();
builder.Services.AddChmsInfrastructure(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Chms")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

await app.RunAsync();

/// <summary>Exposed so integration tests can host the API.</summary>
#pragma warning disable CA1515 // Must be public for WebApplicationFactory.
public partial class Program;
#pragma warning restore CA1515
