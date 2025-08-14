namespace resultsService.Presentation.Middleware;

/// <summary>
/// CORS configuration for the application
/// </summary>
public static class CorsConfiguration
{
    public const string DefaultPolicy = "DefaultCorsPolicy";
    public const string AllowAllPolicy = "AllowAllCorsPolicy";

    /// <summary>
    /// Configure CORS policies
    /// </summary>
    public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            // Default policy - more restrictive for production
            options.AddPolicy(DefaultPolicy, policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                    ?? new[] { "http://localhost:3000", "http://localhost:8080" };

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
            });

            // Allow all policy - for development only
            options.AddPolicy(AllowAllPolicy, policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }

    /// <summary>
    /// Use appropriate CORS policy based on environment
    /// </summary>
    public static void UseConfiguredCors(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseCors(AllowAllPolicy);
        }
        else
        {
            app.UseCors(DefaultPolicy);
        }
    }
}
