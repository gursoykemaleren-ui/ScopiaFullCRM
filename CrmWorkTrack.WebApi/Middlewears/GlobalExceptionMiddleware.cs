using CrmWorkTrack.WebApi.Common.Exceptions;
using System.Text.Json;
using CrmWorkTrack.WebApi.Common.Models;
using CrmWorkTrack.WebApi.Common.Constants;

namespace CrmWorkTrack.WebApi.Middlewares;

public sealed class GlobalExceptionMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (BusinessException ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                StatusCodes.Status400BadRequest,
                ex.ErrorCode,
                ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                StatusCodes.Status404NotFound,
                ErrorCodes.Resource.NotFound,
                "Resource not found.");
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                StatusCodes.Status403Forbidden,
                ErrorCodes.Auth.Forbidden,
                "Forbidden.");
        }
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                StatusCodes.Status400BadRequest,
                ErrorCodes.General.RequestInvalid,
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                StatusCodes.Status400BadRequest,
                ErrorCodes.General.RequestInvalidOperation,
                ex.Message);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                StatusCodes.Status500InternalServerError,
                ErrorCodes.General.ServerError,
                "Unexpected server error.");
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception ex,
        int statusCode,
        string code,
        string message)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                ex,
                "Cannot write error response because response has already started. TraceId: {TraceId}",
                context.TraceIdentifier);

            return;
        }

        _logger.LogError(
            ex,
            "Unhandled exception caught by GlobalExceptionMiddleware. StatusCode: {StatusCode}, TraceId: {TraceId}",
            statusCode,
            context.TraceIdentifier);

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = ApiResponse<object>.Fail(new ApiErrorDto
        {
            Code = code,
            Message = message,
            TraceId = context.TraceIdentifier
        });

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}