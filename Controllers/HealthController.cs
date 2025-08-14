using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using resultsService.Infrastructure.Data;

namespace resultsService.Controllers;

/// <summary>
/// Controller for health check operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check requested");

        try
        {
            var response = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Service = "Results Service API"
            };

            _logger.LogInformation("Health check passed");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Database connectivity health check
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Database health status</returns>
    [HttpGet("database")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DatabaseAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Database health check requested");

        try
        {
            // Try to connect to the database
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                _logger.LogWarning("Database connection failed");
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Component = "Database",
                    Timestamp = DateTime.UtcNow,
                    Error = "Cannot connect to database"
                });
            }

            // Get marketplace items count for additional verification
            var itemCount = await _context.MarketplaceItems.CountAsync(cancellationToken);

            var response = new
            {
                Status = "Healthy",
                Component = "Database",
                Timestamp = DateTime.UtcNow,
                ConnectionStatus = "Connected",
                MarketplaceItemsCount = itemCount
            };

            _logger.LogInformation("Database health check passed. Items count: {ItemCount}", itemCount);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Component = "Database",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Detailed health check with all components
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed health status</returns>
    [HttpGet("detailed")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DetailedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Detailed health check requested");

        var healthChecks = new Dictionary<string, object>();
        var overallStatus = "Healthy";

        // API Health
        try
        {
            healthChecks["API"] = new
            {
                Status = "Healthy",
                Version = "1.0.0",
                Uptime = DateTime.UtcNow.Subtract(System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime()),
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            healthChecks["API"] = new
            {
                Status = "Unhealthy",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            overallStatus = "Unhealthy";
        }

        // Database Health
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            var itemCount = canConnect ? await _context.MarketplaceItems.CountAsync(cancellationToken) : 0;

            healthChecks["Database"] = new
            {
                Status = canConnect ? "Healthy" : "Unhealthy",
                ConnectionStatus = canConnect ? "Connected" : "Disconnected",
                MarketplaceItemsCount = itemCount,
                Timestamp = DateTime.UtcNow
            };

            if (!canConnect)
                overallStatus = "Unhealthy";
        }
        catch (Exception ex)
        {
            healthChecks["Database"] = new
            {
                Status = "Unhealthy",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            overallStatus = "Unhealthy";
        }

        // Memory Health
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;

            healthChecks["Memory"] = new
            {
                Status = "Healthy",
                WorkingSetMB = workingSet / (1024 * 1024),
                PrivateMemoryMB = privateMemory / (1024 * 1024),
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            healthChecks["Memory"] = new
            {
                Status = "Unhealthy",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            };
            overallStatus = "Unhealthy";
        }

        var response = new
        {
            Status = overallStatus,
            Timestamp = DateTime.UtcNow,
            Checks = healthChecks
        };

        var statusCode = overallStatus == "Healthy" ? 200 : 503;
        
        _logger.LogInformation("Detailed health check completed. Overall status: {Status}", overallStatus);
        
        return StatusCode(statusCode, response);
    }
}
