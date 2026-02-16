namespace BookingInfrastructure;

using Microsoft.EntityFrameworkCore;
using BookingDomain;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> op) : base(op)
    {
    }

    public DbSet<Users> Users { get; set; }
    public DbSet<Roles> Roles { get; set; }
    public DbSet<UserRoles> UserRoles { get; set; }
    public DbSet<OwnerProfiles> OwnerProfiles { get; set; }
    public DbSet<Properties> Properties { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Bookings> Bookings { get; set; }
    public DbSet<Reviews> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRoles>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<OwnerProfiles>()
            .HasOne(op => op.User)
            .WithOne(u => u.OwnerProfile)
            .HasForeignKey<OwnerProfiles>(op => op.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reviews>()
            .HasOne(r => r.Booking)
            .WithOne(b => b.Review)
            .HasForeignKey<Reviews>(r => r.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Properties>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.OwnedProperties)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Properties>()
            .HasOne(p => p.Address)
            .WithMany(a => a.Properties)
            .HasForeignKey(p => p.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bookings>()
            .HasOne(b => b.Property)
            .WithMany(p => p.Bookings)
            .HasForeignKey(b => b.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bookings>()
            .HasOne(b => b.Guest)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Reviews>()
            .HasOne(r => r.Guest)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.GuestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}