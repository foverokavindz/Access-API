using resultsService.Domain.Enums;

namespace resultsService.Domain.Entities;


public class MarketplaceItem : BaseEntity
{
    public string ItemId { get; private set; } = string.Empty;

    public string? ItemImageUrl { get; private set; }

    public string? ItemUrl { get; private set; }

    public string ItemTitle { get; private set; } = string.Empty;

    public int PlatformId { get; private set; }

    public string? SearchTerm { get; private set; }

    public string? Quantity { get; private set; }

    public int? QuantityNumber { get; private set; }

    public string? Price { get; private set; }

    public decimal? PriceUsd { get; private set; }

    public string? ProductId { get; private set; }

    public string? SellerId { get; private set; }

    public string? SellerName { get; private set; }

    public string? SellerUrl { get; private set; }

    public string? SellerLocation { get; private set; }

    public DateTime DetectedDate { get; private set; }

    // Private parameterless constructor for EF Core
    private MarketplaceItem() { }

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

    public void UpdatePricing(string? price, decimal? priceUsd)
    {
        Price = price;
        PriceUsd = priceUsd;
        UpdateTimestamp();
    }

    public void UpdateSellerInfo(string? sellerId, string? sellerName, string? sellerUrl, string? sellerLocation)
    {
        SellerId = sellerId;
        SellerName = sellerName;
        SellerUrl = sellerUrl;
        SellerLocation = sellerLocation;
        UpdateTimestamp();
    }

    public void UpdateMediaContent(string? itemImageUrl, string? itemUrl)
    {
        ItemImageUrl = itemImageUrl;
        ItemUrl = itemUrl;
        UpdateTimestamp();
    }

    public void UpdateQuantity(string? quantity, int? quantityNumber)
    {
        Quantity = quantity;
        QuantityNumber = quantityNumber;
        UpdateTimestamp();
    }
}
