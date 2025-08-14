namespace resultsService.Application.DTOs;

public class MarketplaceItemDto
{
    public int Id { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string? ItemImageUrl { get; set; }
    public string? ItemUrl { get; set; }
    public string ItemTitle { get; set; } = string.Empty;
    public int PlatformId { get; set; }
    public string? SearchTerm { get; set; }
    public string? Quantity { get; set; }
    public int? QuantityNumber { get; set; }
    public string? Price { get; set; }
    public decimal? PriceUsd { get; set; }
    public string? ProductId { get; set; }
    public string? SellerId { get; set; }
    public string? SellerName { get; set; }
    public string? SellerUrl { get; set; }
    public string? SellerLocation { get; set; }
    public DateTime DetectedDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
