using System.Text.Json;
using CrmWorkTrack.WebApi.Common.Models;

namespace CrmWorkTrack.WebApi.Middlewares;

public sealed class StatusCodeMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await next(context);

        if (context.Response.HasStarted)
            return;

        var statusCode = context.Response.StatusCode;

        if (statusCode < 400)
            return;

       
        if (context.Response.ContentLength.HasValue && context.Response.ContentLength.Value > 0)
            return;

        if (!string.IsNullOrWhiteSpace(context.Response.ContentType) &&
            !context.Response.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) &&
            !context.Response.ContentType.Contains("text/plain", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        context.Response.ContentType = "application/json";

        var (code, message) = statusCode switch
        {
            StatusCodes.Status401Unauthorized => ("auth.unauthorized", "Unauthorized"),
            StatusCodes.Status403Forbidden => ("auth.forbidden", "Forbidden"),
            StatusCodes.Status404NotFound => ("resource.not_found", "Not found"),
            _ => ("request.failed", "Request failed")
        };

        var payload = ApiResponse<object>.Fail(new ApiErrorDto
        {
            Code = code,
            Message = message,
            TraceId = context.TraceIdentifier
        });

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}