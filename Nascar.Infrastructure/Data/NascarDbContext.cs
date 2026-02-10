using Microsoft.EntityFrameworkCore;
using Nascar.Domain.Entities;

namespace Nascar.Infrastructure.Data;

public class NascarDbContext : DbContext
{
    public NascarDbContext(DbContextOptions<NascarDbContext> options) : base(options) { }

    public DbSet<RaceEvent> RaceEvents => Set<RaceEvent>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<RaceSnapshot> RaceSnapshots => Set<RaceSnapshot>();
    public DbSet<DriverSnapshot> DriverSnapshots => Set<DriverSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RaceSnapshot>()
            .HasMany(r => r.DriverSnapshots)
            .WithOne(d => d.RaceSnapshot)
            .HasForeignKey(d => d.RaceSnapshotId);
    }
}