using System.Net;
using System.Text.Json;

namespace resultsService.Presentation.Middleware;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions
/// and returns a consistent error response
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case ArgumentException ex:
                response.Message = ex.Message;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Title = "Bad Request";
                break;

            case ArgumentNullException ex:
                response.Message = $"Required parameter is missing: {ex.ParamName}";
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Title = "Bad Request";
                break;

            case InvalidOperationException ex:
                response.Message = ex.Message;
                response.StatusCode = (int)HttpStatusCode.Conflict;
                response.Title = "Conflict";
                break;

            case KeyNotFoundException ex:
                response.Message = ex.Message;
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Title = "Not Found";
                break;

            case UnauthorizedAccessException ex:
                response.Message = ex.Message;
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Title = "Unauthorized";
                break;

            case NotImplementedException ex:
                response.Message = ex.Message;
                response.StatusCode = (int)HttpStatusCode.NotImplemented;
                response.Title = "Not Implemented";
                break;

            default:
                response.Message = "An error occurred while processing your request";
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Title = "Internal Server Error";
                break;
        }

        context.Response.StatusCode = response.StatusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Standardized error response model
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Error title/summary
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Unique identifier for tracking this error
    /// </summary>
    public string TraceId { get; set; } = Guid.NewGuid().ToString();
}
