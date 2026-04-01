using FuelPriceTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FuelPriceTracker.Data;

public sealed class FuelPriceDbContext(DbContextOptions<FuelPriceDbContext> options) : DbContext(options)
{
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<PriceSnapshot> PriceSnapshots => Set<PriceSnapshot>();
    public DbSet<PriceChange> PriceChanges => Set<PriceChange>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasMaxLength(64);
            entity.Property(x => x.Name).HasMaxLength(256);
            entity.Property(x => x.Brand).HasMaxLength(128);
            entity.Property(x => x.Street).HasMaxLength(256);
            entity.Property(x => x.Place).HasMaxLength(128);
        });

        modelBuilder.Entity<PriceSnapshot>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.StationId, x.TimestampUtc });
            entity.Property(x => x.Diesel).HasColumnType("decimal(5,3)");
            entity.Property(x => x.E5).HasColumnType("decimal(5,3)");
            entity.Property(x => x.E10).HasColumnType("decimal(5,3)");

            entity.HasOne(x => x.Station)
                .WithMany(x => x.PriceSnapshots)
                .HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PriceChange>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.StationId, x.TimestampUtc });
            entity.Property(x => x.PreviousDiesel).HasColumnType("decimal(5,3)");
            entity.Property(x => x.NewDiesel).HasColumnType("decimal(5,3)");
            entity.Property(x => x.PreviousE5).HasColumnType("decimal(5,3)");
            entity.Property(x => x.NewE5).HasColumnType("decimal(5,3)");
            entity.Property(x => x.PreviousE10).HasColumnType("decimal(5,3)");
            entity.Property(x => x.NewE10).HasColumnType("decimal(5,3)");

            entity.HasOne<Station>()
                .WithMany()
                .HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
