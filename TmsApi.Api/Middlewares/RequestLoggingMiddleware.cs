using System.Diagnostics;
namespace TmsApi.Api.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generate correlation ID
        var correlationId =
            Guid.NewGuid().ToString("N")[..8];

        // Add response header
        context.Response.Headers["X-Correlation-Id"]
            = correlationId;

        // Start timing request
        var stopwatch = Stopwatch.StartNew();

        // Entry log
        _logger.LogInformation(
            "Request started: {Method} {Path} | CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        // Continue pipeline
        await _next(context);

        // Stop timer
        stopwatch.Stop();

        // Exit log
        _logger.LogInformation(
            "Request completed: {StatusCode} in {ElapsedMs}ms | CorrelationId: {CorrelationId}",
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds,
            correlationId);
    }
}