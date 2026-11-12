// Namespace for the data access layer of the Ecommerce API
namespace Ecommerce.Api.Data;

// Import Entity Framework Core for DbContext and DbSet functionality
using Microsoft.EntityFrameworkCore;
// Import the domain models used by this context
using Ecommerce.Api.Models;

/// <summary>
/// Application database context that manages entity sets and database configuration.
/// Uses InMemory provider for demonstration purposes.
/// </summary>
public class AppDbContext : DbContext
{
    // Constructor that accepts DbContextOptions so the host can configure the provider (InMemory, SQL Server, etc.)
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        // Base constructor call wires up the options provided by dependency injection
    }

    // DbSet representing the Provinces table/collection; enables LINQ queries and change tracking
    public DbSet<Province> Provinces { get; set; } = null!;

    // DbSet representing the Cities table/collection
    public DbSet<City> Cities { get; set; } = null!;

    // DbSet representing the Barangays table/collection
    public DbSet<Barangay> Barangays { get; set; } = null!;

    // Override to configure entity relationships, keys, and constraints using Fluent API
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call base implementation first so default conventions are applied
        base.OnModelCreating(modelBuilder);

        // Configure composite primary key for City (ProvinceId + CityId)
        modelBuilder.Entity<City>()
            .HasKey(c => new { c.ProvinceId, c.CityId });

        // Configure composite primary key for Barangay (ProvinceId + CityId + BarangayId)
        modelBuilder.Entity<Barangay>()
            .HasKey(b => new { b.ProvinceId, b.CityId, b.BarangayId });

        // Configure one-to-many relationship: Province has many Cities
        modelBuilder.Entity<City>()
            .HasOne(c => c.Province)
            .WithMany(p => p.Cities)
            .HasForeignKey(c => c.ProvinceId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configure one-to-many relationship: City has many Barangays
        modelBuilder.Entity<Barangay>()
            .HasOne(b => b.City)
            .WithMany(c => c.Barangays)
            .HasForeignKey(b => new { b.ProvinceId, b.CityId })
            .OnDelete(DeleteBehavior.NoAction);

        // Configure relationship from Barangay to Province
        modelBuilder.Entity<Barangay>()
            .HasOne(b => b.Province)
            .WithMany()
            .HasForeignKey(b => b.ProvinceId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
