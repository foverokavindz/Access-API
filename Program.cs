using Microsoft.EntityFrameworkCore;
using FluentValidation;
using resultsService.Domain.Interfaces;
using resultsService.Infrastructure.Data;
using resultsService.Infrastructure.Repositories;
using resultsService.Application.Services;
using resultsService.Application.Validators;
using resultsService.Presentation.Middleware;
using resultsService.Common.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Bind configuration sections to strongly typed objects
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection(CorsSettings.SectionName));
builder.Services.Configure<RateLimitingSettings>(builder.Configuration.GetSection(RateLimitingSettings.SectionName));
builder.Services.Configure<CachingSettings>(builder.Configuration.GetSection(CachingSettings.SectionName));
builder.Services.Configure<HealthCheckSettings>(builder.Configuration.GetSection(HealthCheckSettings.SectionName));

// Add services to the container.

// Configure Entity Framework with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IMarketplaceItemRepository, MarketplaceItemRepository>();

// Register application services
builder.Services.AddScoped<MarketplaceItemService>();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateMarketplaceItemValidator>();

// Configure middleware services
builder.Services.ConfigureCors(builder.Configuration);
builder.Services.ConfigureResponseCompression();
builder.Services.ConfigureCaching();
builder.Services.ConfigureRateLimiting();
builder.Services.ConfigureHealthChecks(builder.Configuration);

// Configure controllers and API documentation
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Customize model validation error responses
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage);

            var response = new
            {
                title = "Validation Error",
                status = 400,
                errors = errors,
                traceId = System.Diagnostics.Activity.Current?.Id ?? Guid.NewGuid().ToString()
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure Swagger for better API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Results Service API",
        Version = "v1",
        Description = "API for managing marketplace scraping results",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Results Service Team",
            Email = "support@resultsservice.com"
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Health checks (should be early in pipeline)
app.UseHealthCheckEndpoints();

// Global exception handling (should be first)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Request logging (early in pipeline for complete request/response logging)
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<RequestLoggingMiddleware>();
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// CORS (must be before routing)
app.UseConfiguredCors(app.Environment);

// Performance optimizations
app.UseResponseCompression();
app.UseResponseCaching();

// Development tools
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Results Service API V1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableValidator();
        c.EnableDeepLinking();
    });
}

// Standard middleware
app.UseHttpsRedirection();
app.UseAuthentication(); // Add this when you implement authentication
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add a simple root endpoint
app.MapGet("/", () => new
{
    service = "Results Service API",
    version = "1.0.0",
    status = "Running",
    timestamp = DateTime.UtcNow,
    endpoints = new
    {
        health = "/health",
        healthUI = "/health-ui",
        swagger = "/swagger",
        api = "/api"
    }
}).WithTags("Root").WithSummary("API Information");

app.Run();
