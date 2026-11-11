using ChMS.Domain.Audit;
using ChMS.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace ChMS.Infrastructure.Persistence;

public sealed class ChmsDbContext(DbContextOptions<ChmsDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        // Every decimal in the database is exact money-safe precision. Never float or double.
        configurationBuilder.Properties<decimal>().HavePrecision(19, 4);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<AppUser>(user =>
        {
            user.ToTable("Users");
            user.HasKey(u => u.Id);
            user.Property(u => u.Id).ValueGeneratedNever();
            user.Property(u => u.EntraObjectId).HasMaxLength(64).IsRequired();
            user.HasIndex(u => u.EntraObjectId).IsUnique();
            user.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
            user.Property(u => u.Email).HasMaxLength(320);
        });

        modelBuilder.Entity<AuditLogEntry>(log =>
        {
            log.ToTable("AuditLog");
            log.HasKey(a => a.Id);
            log.Property(a => a.ActorId).HasMaxLength(64);
            log.Property(a => a.ActorName).HasMaxLength(200);
            log.Property(a => a.RecordType).HasMaxLength(100);
            log.Property(a => a.RecordId).HasMaxLength(100);
            log.Property(a => a.Action).HasConversion<string>().HasMaxLength(20);
            log.Property(a => a.Reason).HasMaxLength(1000);
            log.HasIndex(a => new { a.RecordType, a.RecordId });
            log.HasIndex(a => a.OccurredAt);
        });
    }
}
