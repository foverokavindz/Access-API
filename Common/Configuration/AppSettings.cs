namespace resultsService.Common.Configuration;

/// <summary>
/// Configuration model for API settings
/// </summary>
public class ApiSettings
{
    public const string SectionName = "ApiSettings";

    /// <summary>
    /// Default page size for paginated results
    /// </summary>
    public int DefaultPageSize { get; set; } = 50;

    /// <summary>
    /// Maximum allowed page size
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// API title for documentation
    /// </summary>
    public string ApiTitle { get; set; } = "Results Service API";

    /// <summary>
    /// API version
    /// </summary>
    public string ApiVersion { get; set; } = "v1";

    /// <summary>
    /// API description for documentation
    /// </summary>
    public string ApiDescription { get; set; } = "API for managing marketplace scraping results";
}

/// <summary>
/// Configuration model for CORS settings
/// </summary>
public class CorsSettings
{
    public const string SectionName = "Cors";

    /// <summary>
    /// Allowed origins for CORS
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether to allow credentials
    /// </summary>
    public bool AllowCredentials { get; set; } = true;

    /// <summary>
    /// Allowed HTTP methods
    /// </summary>
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Allowed headers
    /// </summary>
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Configuration model for rate limiting settings
/// </summary>
public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Whether rate limiting is enabled
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Number of requests allowed per window
    /// </summary>
    public int PermitLimit { get; set; } = 100;

    /// <summary>
    /// Time window in seconds
    /// </summary>
    public int WindowInSeconds { get; set; } = 60;

    /// <summary>
    /// Queue limit for rate limiting
    /// </summary>
    public int QueueLimit { get; set; } = 10;
}

/// <summary>
/// Configuration model for caching settings
/// </summary>
public class CachingSettings
{
    public const string SectionName = "Caching";

    /// <summary>
    /// Default cache expiration in minutes
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Health check cache expiration in minutes
    /// </summary>
    public int HealthCheckCacheMinutes { get; set; } = 1;
}

/// <summary>
/// Configuration model for health check settings
/// </summary>
public class HealthCheckSettings
{
    public const string SectionName = "HealthChecks";

    /// <summary>
    /// Whether health check UI is enabled
    /// </summary>
    public bool EnableUI { get; set; } = true;

    /// <summary>
    /// Path for health check UI
    /// </summary>
    public string UIPath { get; set; } = "/health-ui";

    /// <summary>
    /// Health check interval in seconds
    /// </summary>
    public int CheckInterval { get; set; } = 30;
}
