using Microsoft.EntityFrameworkCore;
using TempMonitor.Models;

namespace TempMonitor.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TemperatureReading> TemperatureReadings { get; set; }
    public DbSet<WeatherReading> WeatherReadings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TemperatureReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Temperature).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Humidity).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Timestamp).HasColumnType("timestamp with time zone");
            entity.HasIndex(e => e.Timestamp);

            // Configure 1-to-1 relationship with WeatherReading
            entity.HasOne(e => e.WeatherReading)
                .WithOne(w => w.TemperatureReading)
                .HasForeignKey<WeatherReading>(w => w.TemperatureReadingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WeatherReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Temperature).HasColumnType("decimal(5,2)");
            entity.Property(e => e.FeelsLike).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Humidity).HasColumnType("decimal(5,2)");
            entity.Property(e => e.WindSpeed).HasColumnType("decimal(5,2)");
            entity.Property(e => e.WeatherMain).HasMaxLength(100);
            entity.Property(e => e.WeatherDescription).HasMaxLength(200);
            entity.Property(e => e.Sunrise).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Sunset).HasColumnType("timestamp with time zone");
            entity.Property(e => e.Timestamp).HasColumnType("timestamp with time zone");

            // Create unique index on foreign key to enforce 1-to-1
            entity.HasIndex(e => e.TemperatureReadingId).IsUnique();
            entity.HasIndex(e => e.Timestamp);
        });
    }
}