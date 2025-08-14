using resultsService.Application.DTOs;
using resultsService.Domain.Entities;
using resultsService.Domain.Interfaces;

namespace resultsService.Application.Services;

/// <summary>
/// Application service for marketplace item operations
/// </summary>
public class MarketplaceItemService
{
    private readonly IMarketplaceItemRepository _repository;

    public MarketplaceItemService(IMarketplaceItemRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Gets a marketplace item by ID
    /// </summary>
    public async Task<MarketplaceItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    /// <summary>
    /// Gets a marketplace item by external item ID
    /// </summary>
    public async Task<MarketplaceItemDto?> GetByItemIdAsync(string itemId, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByItemIdAsync(itemId, cancellationToken);
        return item == null ? null : MapToDto(item);
    }

    /// <summary>
    /// Gets all marketplace items with pagination and filtering
    /// </summary>
    public async Task<PagedResultDto<MarketplaceItemDto>> GetAllAsync(
        int? platformId = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        var items = await _repository.GetAllAsync(platformId, searchTerm, pageNumber, pageSize, cancellationToken);
        var totalCount = await _repository.GetCountAsync(platformId, searchTerm, cancellationToken);

        return new PagedResultDto<MarketplaceItemDto>
        {
            Items = items.Select(MapToDto),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Creates a new marketplace item
    /// </summary>
    public async Task<MarketplaceItemDto> CreateAsync(CreateMarketplaceItemDto createDto, CancellationToken cancellationToken = default)
    {
        // Business validation
        if (string.IsNullOrWhiteSpace(createDto.ItemId))
            throw new ArgumentException("ItemId is required", nameof(createDto.ItemId));

        if (string.IsNullOrWhiteSpace(createDto.ItemTitle))
            throw new ArgumentException("ItemTitle is required", nameof(createDto.ItemTitle));

        // Check if item already exists
        var existingItem = await _repository.GetByItemIdAsync(createDto.ItemId, cancellationToken);
        if (existingItem != null)
            throw new InvalidOperationException($"Item with ItemId '{createDto.ItemId}' already exists");

        // Create domain entity
        var item = new MarketplaceItem(
            createDto.ItemId,
            createDto.ItemTitle,
            createDto.PlatformId,
            createDto.DetectedDate);

        // Update optional properties
        if (!string.IsNullOrWhiteSpace(createDto.ItemImageUrl) || !string.IsNullOrWhiteSpace(createDto.ItemUrl))
        {
            item.UpdateMediaContent(createDto.ItemImageUrl, createDto.ItemUrl);
        }

        if (!string.IsNullOrWhiteSpace(createDto.Price) || createDto.PriceUsd.HasValue)
        {
            item.UpdatePricing(createDto.Price, createDto.PriceUsd);
        }

        if (!string.IsNullOrWhiteSpace(createDto.Quantity) || createDto.QuantityNumber.HasValue)
        {
            item.UpdateQuantity(createDto.Quantity, createDto.QuantityNumber);
        }

        if (!string.IsNullOrWhiteSpace(createDto.SellerId) || !string.IsNullOrWhiteSpace(createDto.SellerName))
        {
            item.UpdateSellerInfo(createDto.SellerId, createDto.SellerName, createDto.SellerUrl, createDto.SellerLocation);
        }

        // Save to repository
        var savedItem = await _repository.AddAsync(item, cancellationToken);
        return MapToDto(savedItem);
    }

    /// <summary>
    /// Updates an existing marketplace item
    /// </summary>
    public async Task<MarketplaceItemDto?> UpdateAsync(int id, UpdateMarketplaceItemDto updateDto, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return null;

        // Apply updates using domain methods
        item.UpdateMediaContent(updateDto.ItemImageUrl, updateDto.ItemUrl);
        item.UpdatePricing(updateDto.Price, updateDto.PriceUsd);
        item.UpdateQuantity(updateDto.Quantity, updateDto.QuantityNumber);
        item.UpdateSellerInfo(updateDto.SellerId, updateDto.SellerName, updateDto.SellerUrl, updateDto.SellerLocation);

        var updatedItem = await _repository.UpdateAsync(item, cancellationToken);
        return MapToDto(updatedItem);
    }

    /// <summary>
    /// Deletes a marketplace item
    /// </summary>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    /// <summary>
    /// Gets marketplace items by seller ID
    /// </summary>
    public async Task<IEnumerable<MarketplaceItemDto>> GetBySellerIdAsync(string sellerId, CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetBySellerIdAsync(sellerId, cancellationToken);
        return items.Select(MapToDto);
    }

    /// <summary>
    /// Gets marketplace items within a price range
    /// </summary>
    public async Task<IEnumerable<MarketplaceItemDto>> GetByPriceRangeAsync(decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetByPriceRangeAsync(minPrice, maxPrice, cancellationToken);
        return items.Select(MapToDto);
    }

    /// <summary>
    /// Gets recently detected marketplace items
    /// </summary>
    public async Task<IEnumerable<MarketplaceItemDto>> GetRecentlyDetectedAsync(int hoursAgo = 24, CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetRecentlyDetectedAsync(hoursAgo, cancellationToken);
        return items.Select(MapToDto);
    }

    /// <summary>
    /// Maps domain entity to DTO
    /// </summary>
    private static MarketplaceItemDto MapToDto(MarketplaceItem item)
    {
        return new MarketplaceItemDto
        {
            Id = item.Id,
            ItemId = item.ItemId,
            ItemImageUrl = item.ItemImageUrl,
            ItemUrl = item.ItemUrl,
            ItemTitle = item.ItemTitle,
            PlatformId = item.PlatformId,
            SearchTerm = item.SearchTerm,
            Quantity = item.Quantity,
            QuantityNumber = item.QuantityNumber,
            Price = item.Price,
            PriceUsd = item.PriceUsd,
            ProductId = item.ProductId,
            SellerId = item.SellerId,
            SellerName = item.SellerName,
            SellerUrl = item.SellerUrl,
            SellerLocation = item.SellerLocation,
            DetectedDate = item.DetectedDate,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
