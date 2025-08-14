using System.Diagnostics;
using System.Text;

namespace resultsService.Presentation.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Start timing the request
        var stopwatch = Stopwatch.StartNew();
        
        // Generate a correlation ID for tracking
        var correlationId = Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        // Log incoming request
        await LogRequestAsync(context, correlationId);

        // Capture the original response body stream
        var originalResponseBodyStream = context.Response.Body;

        try
        {
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // Execute the next middleware in the pipeline
            await _next(context);

            // Log outgoing response
            stopwatch.Stop();
            await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);

            // Copy the response back to the original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);
        }
        finally
        {
            context.Response.Body = originalResponseBodyStream;
        }
    }

    private async Task LogRequestAsync(HttpContext context, string correlationId)
    {
        var request = context.Request;

        // Build request info
        var requestInfo = new StringBuilder();
        requestInfo.AppendLine($"=== INCOMING REQUEST [{correlationId}] ===");
        requestInfo.AppendLine($"Method: {request.Method}");
        requestInfo.AppendLine($"Path: {request.Path}");
        requestInfo.AppendLine($"QueryString: {request.QueryString}");
        requestInfo.AppendLine($"ContentType: {request.ContentType}");
        requestInfo.AppendLine($"ContentLength: {request.ContentLength}");
        requestInfo.AppendLine($"UserAgent: {request.Headers.UserAgent}");
        requestInfo.AppendLine($"RemoteIP: {context.Connection.RemoteIpAddress}");

        // Log headers (exclude sensitive ones)
        if (request.Headers.Any())
        {
            requestInfo.AppendLine("Headers:");
            foreach (var header in request.Headers)
            {
                if (!IsSensitiveHeader(header.Key))
                {
                    requestInfo.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
                }
            }
        }

        // Log request body for non-GET requests (be careful with large payloads)
        if (request.Method != "GET" && request.ContentLength > 0 && request.ContentLength < 10000)
        {
            request.EnableBuffering();
            var body = await ReadStreamAsync(request.Body);
            request.Body.Position = 0;
            
            if (!string.IsNullOrEmpty(body))
            {
                requestInfo.AppendLine($"Body: {body}");
            }
        }

        _logger.LogInformation(requestInfo.ToString());
    }

    private async Task LogResponseAsync(HttpContext context, string correlationId, long elapsedMs)
    {
        var response = context.Response;

        var responseInfo = new StringBuilder();
        responseInfo.AppendLine($"=== OUTGOING RESPONSE [{correlationId}] ===");
        responseInfo.AppendLine($"StatusCode: {response.StatusCode}");
        responseInfo.AppendLine($"ContentType: {response.ContentType}");
        responseInfo.AppendLine($"ContentLength: {response.ContentLength}");
        responseInfo.AppendLine($"ElapsedTime: {elapsedMs}ms");

        // Log response headers
        if (response.Headers.Any())
        {
            responseInfo.AppendLine("Headers:");
            foreach (var header in response.Headers)
            {
                responseInfo.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        // Log response body for error responses or small payloads
        if (response.StatusCode >= 400 || (response.ContentLength > 0 && response.ContentLength < 10000))
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var body = await ReadStreamAsync(response.Body);
            response.Body.Seek(0, SeekOrigin.Begin);
            
            if (!string.IsNullOrEmpty(body))
            {
                responseInfo.AppendLine($"Body: {body}");
            }
        }

        var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        _logger.Log(logLevel, responseInfo.ToString());
    }

    private static async Task<string> ReadStreamAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[]
        {
            "Authorization",
            "Cookie",
            "Set-Cookie",
            "X-API-Key",
            "X-Auth-Token",
            "Authentication"
        };

        return sensitiveHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase);
    }
}
