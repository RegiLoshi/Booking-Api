using LoginLogInfrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoginLogInfrastructure;

public class LoginLogDbContext(DbContextOptions<LoginLogDbContext> options) : DbContext(options)
{
    public DbSet<LoginLogEntry> LoginLogs => Set<LoginLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LoginLogEntry>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.EventId).IsUnique();
            entity.Property(x => x.EventType).HasMaxLength(100);
            entity.Property(x => x.SourceApp).HasMaxLength(100);
            entity.Property(x => x.Email).HasMaxLength(320);
            entity.Property(x => x.PayloadJson);
        });
    }
}
