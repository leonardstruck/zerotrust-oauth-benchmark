using Microsoft.EntityFrameworkCore;

using ZeroTrustOAuth.Inventory.Data.Configurations;
using ZeroTrustOAuth.Inventory.Domain;

namespace ZeroTrustOAuth.Inventory.Data;

/// <summary>
/// Database context for the Inventory service.
/// </summary>
public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the Products DbSet.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
