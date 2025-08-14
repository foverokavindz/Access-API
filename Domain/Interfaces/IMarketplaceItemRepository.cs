using resultsService.Domain.Entities;

namespace resultsService.Domain.Interfaces;

/// <summary>
/// Repository contract for MarketplaceItem entity
/// This interface defines what operations can be performed on MarketplaceItem data
/// The actual implementation will be in the Infrastructure layer
/// </summary>
public interface IMarketplaceItemRepository
{
    /// <summary>
    /// Gets a marketplace item by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The marketplace item if found, null otherwise</returns>
    Task<MarketplaceItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a marketplace item by its external item ID
    /// </summary>
    /// <param name="itemId">The external item identifier</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The marketplace item if found, null otherwise</returns>
    Task<MarketplaceItem?> GetByItemIdAsync(string itemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all marketplace items with optional filtering and pagination
    /// </summary>
    /// <param name="platformId">Optional platform filter</param>
    /// <param name="searchTerm">Optional search term filter</param>
    /// <param name="pageNumber">Page number for pagination (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>List of marketplace items</returns>
    Task<IEnumerable<MarketplaceItem>> GetAllAsync(
        int? platformId = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of marketplace items with optional filtering
    /// </summary>
    /// <param name="platformId">Optional platform filter</param>
    /// <param name="searchTerm">Optional search term filter</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Total count of items</returns>
    Task<int> GetCountAsync(
        int? platformId = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new marketplace item
    /// </summary>
    /// <param name="item">The marketplace item to add</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The added marketplace item with generated ID</returns>
    Task<MarketplaceItem> AddAsync(MarketplaceItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing marketplace item
    /// </summary>
    /// <param name="item">The marketplace item to update</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>The updated marketplace item</returns>
    Task<MarketplaceItem> UpdateAsync(MarketplaceItem item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a marketplace item by its ID
    /// </summary>
    /// <param name="id">The unique identifier of the item to delete</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>True if the item was deleted, false if not found</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets marketplace items by seller ID
    /// </summary>
    /// <param name="sellerId">The seller identifier</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>List of marketplace items from the seller</returns>
    Task<IEnumerable<MarketplaceItem>> GetBySellerIdAsync(string sellerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets marketplace items within a price range
    /// </summary>
    /// <param name="minPrice">Minimum price in USD</param>
    /// <param name="maxPrice">Maximum price in USD</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>List of marketplace items within the price range</returns>
    Task<IEnumerable<MarketplaceItem>> GetByPriceRangeAsync(
        decimal? minPrice,
        decimal? maxPrice,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recently detected marketplace items
    /// </summary>
    /// <param name="hoursAgo">Number of hours ago to search from</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>List of recently detected marketplace items</returns>
    Task<IEnumerable<MarketplaceItem>> GetRecentlyDetectedAsync(
        int hoursAgo = 24,
        CancellationToken cancellationToken = default);
}
