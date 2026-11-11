using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChMS.Infrastructure.Persistence;

/// <summary>Lets <c>dotnet ef</c> create migrations without starting the API or signing in.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ChmsDbContext>
{
    public ChmsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CHMS_DESIGN_CONNECTION")
            ?? "Server=localhost,1433;Database=Chms;User Id=sa;Password=placeholder;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<ChmsDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ChmsDbContext(options);
    }
}
