using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace resultsService.Presentation.Middleware;

/// <summary>
/// Configuration for health checks
/// </summary>
public static class HealthCheckConfiguration
{
    /// <summary>
    /// Configure health checks for the application
    /// </summary>
    public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running"))
            .AddNpgSql(
                configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
                name: "database",
                tags: new[] { "database", "postgresql" })
            .AddCheck<ApiHealthCheck>("api_health");

        // Add health check UI (optional - for development)
        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(30);
            options.MaximumHistoryEntriesPerEndpoint(50);
            options.AddHealthCheckEndpoint("API Health", "/health");
        }).AddInMemoryStorage();
    }

    /// <summary>
    /// Configure health check endpoints
    /// </summary>
    public static void UseHealthCheckEndpoints(this IApplicationBuilder app)
    {
        // Detailed health check endpoint
        app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    totalDuration = report.TotalDuration.TotalMilliseconds,
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        duration = entry.Value.Duration.TotalMilliseconds,
                        description = entry.Value.Description,
                        data = entry.Value.Data
                    })
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
            }
        });

        // Simple ready endpoint
        app.UseHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        // Simple live endpoint
        app.UseHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false
        });

        // Health check UI (for development)
        app.UseHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-ui-api";
        });
    }
}

/// <summary>
/// Custom health check for API-specific health monitoring
/// </summary>
public class ApiHealthCheck : IHealthCheck
{
    private readonly ILogger<ApiHealthCheck> _logger;

    public ApiHealthCheck(ILogger<ApiHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Add your custom health check logic here
            // For example: check external service connectivity, disk space, memory usage, etc.

            var memoryUsed = GC.GetTotalMemory(false);
            var data = new Dictionary<string, object>
            {
                ["memory_bytes"] = memoryUsed,
                ["timestamp"] = DateTime.UtcNow,
                ["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };

            // Simple memory check (you can make this more sophisticated)
            if (memoryUsed > 500_000_000) // 500MB threshold
            {
                return Task.FromResult(HealthCheckResult.Degraded("High memory usage detected", data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("API is healthy", data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Health check failed", ex));
        }
    }
}
