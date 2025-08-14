namespace resultsService.Domain.Enums;

/// <summary>
/// Represents different platform segments for marketplace scanning
/// </summary>
public enum SegmentType
{
    /// <summary>
    /// Marketplace platforms (e.g., eBay, Amazon, Etsy)
    /// </summary>
    Marketplace = 1,

    /// <summary>
    /// Social media platforms (e.g., Facebook Marketplace, Instagram Shopping)
    /// </summary>
    SocialMedia = 2,

    /// <summary>
    /// General websites and e-commerce sites
    /// </summary>
    Websites = 3,

    /// <summary>
    /// Domain-related marketplaces
    /// </summary>
    Domain = 4,

    /// <summary>
    /// NFT marketplaces (e.g., OpenSea, Rarible)
    /// </summary>
    NFT = 5,

    /// <summary>
    /// Mobile app stores and app marketplaces
    /// </summary>
    Apps = 6
}

/// <summary>
/// Extension methods for SegmentType enum
/// </summary>
public static class SegmentTypeExtensions
{
    /// <summary>
    /// Gets a human-readable description of the segment type
    /// </summary>
    public static string GetDescription(this SegmentType segmentType)
    {
        return segmentType switch
        {
            SegmentType.Marketplace => "Online Marketplaces",
            SegmentType.SocialMedia => "Social Media Platforms",
            SegmentType.Websites => "E-commerce Websites",
            SegmentType.Domain => "Domain Marketplaces",
            SegmentType.NFT => "NFT Marketplaces",
            SegmentType.Apps => "App Stores",
            _ => "Unknown Segment"
        };
    }

    /// <summary>
    /// Checks if the segment supports real-time monitoring
    /// </summary>
    public static bool SupportsRealTimeMonitoring(this SegmentType segmentType)
    {
        return segmentType switch
        {
            SegmentType.Marketplace => true,
            SegmentType.SocialMedia => false, // Usually requires special permissions
            SegmentType.Websites => true,
            SegmentType.Domain => true,
            SegmentType.NFT => true,
            SegmentType.Apps => false, // App stores have different APIs
            _ => false
        };
    }
}
