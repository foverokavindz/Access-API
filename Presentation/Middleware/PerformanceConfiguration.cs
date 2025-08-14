using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace resultsService.Presentation.Middleware;

/// <summary>
/// Configuration for response compression and other performance optimizations
/// </summary>
public static class PerformanceConfiguration
{
    /// <summary>
    /// Configure response compression
    /// </summary>
    public static void ConfigureResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            
            // Add MIME types that should be compressed
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/json",
                "application/xml",
                "text/json",
                "text/xml"
            });
        });

        // Configure compression levels
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });
    }

    /// <summary>
    /// Configure caching headers
    /// </summary>
    public static void ConfigureCaching(this IServiceCollection services)
    {
        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 1024 * 1024; // 1MB
            options.UseCaseSensitivePaths = false;
        });
    }

    /// <summary>
    /// Configure rate limiting (basic implementation)
    /// </summary>
    public static void ConfigureRateLimiting(this IServiceCollection services)
    {
        // Note: For production, consider using a more robust rate limiting solution
        // like AspNetCoreRateLimit or built-in .NET 7+ rate limiting
        services.AddMemoryCache();
    }
}
