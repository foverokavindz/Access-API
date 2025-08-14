namespace resultsService.Application.DTOs;

public class UpdateMarketplaceItemDto
{
    public string? ItemImageUrl { get; set; }

    public string? ItemUrl { get; set; }

    public string? Quantity { get; set; }

    public int? QuantityNumber { get; set; }

    public string? Price { get; set; }

    public decimal? PriceUsd { get; set; }

    public string? SellerId { get; set; }

    public string? SellerName { get; set; }

    public string? SellerUrl { get; set; }

    public string? SellerLocation { get; set; }
}
