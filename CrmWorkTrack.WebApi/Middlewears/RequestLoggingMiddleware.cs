using System.Diagnostics;
using System.Security.Claims;

namespace CrmWorkTrack.WebApi.Middlewares;

public sealed class RequestLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();
        var hasException = false;

        var method = context.Request.Method;
        var path = context.Request.Path.ToString();
        var queryString = context.Request.QueryString.HasValue
            ? context.Request.QueryString.Value ?? string.Empty
            : string.Empty;
        var traceId = context.TraceIdentifier;
        // swagger ve health endpointlerini loglama
        if (path.StartsWith("/swagger") || path.StartsWith("/health"))
        {
            await next(context);
            return;
        }
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            hasException = true;
            sw.Stop();

            var remoteIp = context.Connection.RemoteIpAddress?.ToString();

            var userId =
                context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.User?.FindFirst("sub")?.Value
                ?? "anonymous";

            _logger.LogError(
                ex,
                "HTTP {Method} {Path}{QueryString} failed with exception in {ElapsedMs} ms. UserId: {UserId}. IP: {RemoteIp}. TraceId: {TraceId}",
                method,
                path,
                queryString,
                sw.ElapsedMilliseconds,
                userId,
                remoteIp,
                traceId);

            throw;
        }
        finally
        {
            sw.Stop();

            if (!hasException)
            {
                var remoteIp = context.Connection.RemoteIpAddress?.ToString();

                var userId =
                    context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User?.FindFirst("sub")?.Value
                    ?? "anonymous";
                var elapsedMs = sw.ElapsedMilliseconds;
                var statusCode = context.Response.StatusCode;

                if (statusCode >= 500)
                {
                    _logger.LogError(
                        "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMs} ms. UserId: {UserId}. IP: {RemoteIp}. TraceId: {TraceId}",
                        method,
                        path,
                        queryString,
                        statusCode,
                        elapsedMs,
                        userId,
                        remoteIp,
                        traceId);
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning(
                        "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMs} ms. UserId: {UserId}. IP: {RemoteIp}. TraceId: {TraceId}",
                        method,
                        path,
                        queryString,
                        statusCode,
                        elapsedMs,
                        userId,
                        remoteIp,
                        traceId);
                }
                else
                {
                    _logger.LogInformation(
                        "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {ElapsedMs} ms. UserId: {UserId}. IP: {RemoteIp}. TraceId: {TraceId}",
                        method,
                        path,
                        queryString,
                        statusCode,
                        elapsedMs,
                        userId,
                        remoteIp,
                        traceId);
                }
            }
        }
    }
}