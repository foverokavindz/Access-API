using Microsoft.EntityFrameworkCore;
using resultsService.Domain.Entities;

namespace resultsService.Infrastructure.Data;

/// <summary>
/// Application database context for Entity Framework Core
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet for MarketplaceItem entities
    /// </summary>
    public DbSet<MarketplaceItem> MarketplaceItems { get; set; }

    /// <summary>
    /// Configure entity mappings and relationships
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure MarketplaceItem entity
        modelBuilder.Entity<MarketplaceItem>(entity =>
        {
            // Table name
            entity.ToTable("marketplace_items");

            // Primary key
            entity.HasKey(e => e.Id);

            // Configure properties
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.ItemId)
                .HasColumnName("item_id")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.ItemImageUrl)
                .HasColumnName("item_image_url")
                .HasMaxLength(1000);

            entity.Property(e => e.ItemUrl)
                .HasColumnName("item_url")
                .HasMaxLength(1000);

            entity.Property(e => e.ItemTitle)
                .HasColumnName("item_title")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.PlatformId)
                .HasColumnName("platform_id")
                .IsRequired();

            entity.Property(e => e.SearchTerm)
                .HasColumnName("search_term")
                .HasMaxLength(255);

            entity.Property(e => e.Quantity)
                .HasColumnName("quantity")
                .HasMaxLength(100);

            entity.Property(e => e.QuantityNumber)
                .HasColumnName("quantity_number");

            entity.Property(e => e.Price)
                .HasColumnName("price")
                .HasMaxLength(100);

            entity.Property(e => e.PriceUsd)
                .HasColumnName("price_usd")
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.ProductId)
                .HasColumnName("product_id")
                .HasMaxLength(255);

            entity.Property(e => e.SellerId)
                .HasColumnName("seller_id")
                .HasMaxLength(255);

            entity.Property(e => e.SellerName)
                .HasColumnName("seller_name")
                .HasMaxLength(255);

            entity.Property(e => e.SellerUrl)
                .HasColumnName("seller_url")
                .HasMaxLength(1000);

            entity.Property(e => e.SellerLocation)
                .HasColumnName("seller_location")
                .HasMaxLength(255);

            entity.Property(e => e.DetectedDate)
                .HasColumnName("detected_date")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            // Indexes for better performance
            entity.HasIndex(e => e.ItemId)
                .HasDatabaseName("IX_marketplace_items_item_id");

            entity.HasIndex(e => e.PlatformId)
                .HasDatabaseName("IX_marketplace_items_platform_id");

            entity.HasIndex(e => e.SellerId)
                .HasDatabaseName("IX_marketplace_items_seller_id");

            entity.HasIndex(e => e.DetectedDate)
                .HasDatabaseName("IX_marketplace_items_detected_date");
        });
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update timestamps
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Override SaveChanges to automatically update timestamps
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Automatically update timestamps for entities
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.GetType()
                        .GetProperty("CreatedAt")?
                        .SetValue(entry.Entity, DateTime.UtcNow);
                    entry.Entity.GetType()
                        .GetProperty("UpdatedAt")?
                        .SetValue(entry.Entity, DateTime.UtcNow);
                    break;

                case EntityState.Modified:
                    entry.Entity.GetType()
                        .GetProperty("UpdatedAt")?
                        .SetValue(entry.Entity, DateTime.UtcNow);
                    break;
            }
        }
    }
}
