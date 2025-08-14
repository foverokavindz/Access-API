using resultsService.Domain.Enums;

namespace resultsService.Domain.Entities;

/// <summary>
/// Represents a marketplace item scraped from various platforms
/// </summary>
public class MarketplaceItem : BaseEntity
{
    /// <summary>
    /// External item identifier from the source platform
    /// </summary>
    public string ItemId { get; private set; } = string.Empty;

    /// <summary>
    /// URL to the item's image
    /// </summary>
    public string? ItemImageUrl { get; private set; }

    /// <summary>
    /// Direct URL to the item on the source platform
    /// </summary>
    public string? ItemUrl { get; private set; }

    /// <summary>
    /// Title/name of the marketplace item
    /// </summary>
    public string ItemTitle { get; private set; } = string.Empty;

    /// <summary>
    /// Platform where this item was found
    /// </summary>
    public int PlatformId { get; private set; }

    /// <summary>
    /// Search term used to find this item
    /// </summary>
    public string? SearchTerm { get; private set; }

    /// <summary>
    /// Quantity as text (e.g., "5 pieces", "1 box")
    /// </summary>
    public string? Quantity { get; private set; }

    /// <summary>
    /// Numeric quantity value
    /// </summary>
    public int? QuantityNumber { get; private set; }

    /// <summary>
    /// Price as displayed on the platform (with currency)
    /// </summary>
    public string? Price { get; private set; }

    /// <summary>
    /// Price converted to USD for comparison
    /// </summary>
    public decimal? PriceUsd { get; private set; }

    /// <summary>
    /// Product identifier from the source platform
    /// </summary>
    public string? ProductId { get; private set; }

    /// <summary>
    /// Seller's unique identifier
    /// </summary>
    public string? SellerId { get; private set; }

    /// <summary>
    /// Display name of the seller
    /// </summary>
    public string? SellerName { get; private set; }

    /// <summary>
    /// Direct URL to seller's profile
    /// </summary>
    public string? SellerUrl { get; private set; }

    /// <summary>
    /// Geographic location of the seller
    /// </summary>
    public string? SellerLocation { get; private set; }

    /// <summary>
    /// Date and time when this item was detected/scraped
    /// </summary>
    public DateTime DetectedDate { get; private set; }

    // Private parameterless constructor for EF Core
    private MarketplaceItem() { }

    /// <summary>
    /// Creates a new marketplace item
    /// </summary>
    public MarketplaceItem(
        string itemId,
        string itemTitle,
        int platformId,
        DateTime? detectedDate = null)
    {
        ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
        ItemTitle = itemTitle ?? throw new ArgumentNullException(nameof(itemTitle));
        PlatformId = platformId;
        DetectedDate = detectedDate ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the item's pricing information
    /// </summary>
    public void UpdatePricing(string? price, decimal? priceUsd)
    {
        Price = price;
        PriceUsd = priceUsd;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the seller information
    /// </summary>
    public void UpdateSellerInfo(string? sellerId, string? sellerName, string? sellerUrl, string? sellerLocation)
    {
        SellerId = sellerId;
        SellerName = sellerName;
        SellerUrl = sellerUrl;
        SellerLocation = sellerLocation;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the item's media content
    /// </summary>
    public void UpdateMediaContent(string? itemImageUrl, string? itemUrl)
    {
        ItemImageUrl = itemImageUrl;
        ItemUrl = itemUrl;
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates quantity information
    /// </summary>
    public void UpdateQuantity(string? quantity, int? quantityNumber)
    {
        Quantity = quantity;
        QuantityNumber = quantityNumber;
        UpdateTimestamp();
    }
}
