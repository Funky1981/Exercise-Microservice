using Serilog.Context;

namespace Exercise.API.Middleware;

/// <summary>
/// Ensures every request has an X-Correlation-Id header.
/// If the caller supplies one it is reused; otherwise a new GUID is generated.
/// The value is pushed into Serilog's LogContext so every log line written during
/// the request automatically carries {CorrelationId} without any handler changes.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        // Echo it back in the response so callers can trace the request
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.TryAdd(HeaderName, correlationId);
            return Task.CompletedTask;
        });

        // Push into Serilog LogContext — automatically popped when the scope is disposed
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
