using Microsoft.EntityFrameworkCore;
using resultsService.Domain.Entities;
using resultsService.Domain.Interfaces;
using resultsService.Infrastructure.Data;

namespace resultsService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for MarketplaceItem entity using Entity Framework Core
/// </summary>
public class MarketplaceItemRepository : IMarketplaceItemRepository
{
    private readonly ApplicationDbContext _context;

    public MarketplaceItemRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MarketplaceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.MarketplaceItems
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<MarketplaceItem?> GetByItemIdAsync(string itemId, CancellationToken cancellationToken = default)
    {
        return await _context.MarketplaceItems
            .FirstOrDefaultAsync(m => m.ItemId == itemId, cancellationToken);
    }

    public async Task<IEnumerable<MarketplaceItem>> GetAllAsync(
        int? platformId = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MarketplaceItems.AsQueryable();

        // Apply filters
        if (platformId.HasValue)
        {
            query = query.Where(m => m.PlatformId == platformId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(m => m.SearchTerm != null && m.SearchTerm.Contains(searchTerm));
        }

        // Apply pagination
        var skip = (pageNumber - 1) * pageSize;
        
        return await query
            .OrderByDescending(m => m.DetectedDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(
        int? platformId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MarketplaceItems.AsQueryable();

        // Apply same filters as GetAllAsync
        if (platformId.HasValue)
        {
            query = query.Where(m => m.PlatformId == platformId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(m => m.SearchTerm != null && m.SearchTerm.Contains(searchTerm));
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<MarketplaceItem> AddAsync(MarketplaceItem item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _context.MarketplaceItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<MarketplaceItem> UpdateAsync(MarketplaceItem item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        _context.MarketplaceItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _context.MarketplaceItems
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (item == null)
            return false;

        _context.MarketplaceItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IEnumerable<MarketplaceItem>> GetBySellerIdAsync(
        string sellerId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.MarketplaceItems
            .Where(m => m.SellerId == sellerId)
            .OrderByDescending(m => m.DetectedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MarketplaceItem>> GetByPriceRangeAsync(
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MarketplaceItems.AsQueryable();

        if (minPrice.HasValue)
        {
            query = query.Where(m => m.PriceUsd >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(m => m.PriceUsd <= maxPrice.Value);
        }

        return await query
            .OrderByDescending(m => m.DetectedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MarketplaceItem>> GetRecentlyDetectedAsync(
        int hoursAgo = 24,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddHours(-hoursAgo);

        return await _context.MarketplaceItems
            .Where(m => m.DetectedDate >= cutoffDate)
            .OrderByDescending(m => m.DetectedDate)
            .ToListAsync(cancellationToken);
    }
}
