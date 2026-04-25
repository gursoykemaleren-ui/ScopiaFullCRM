namespace CrmWorkTrack.WebApi.Common.Models;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiErrorDto? Error { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(ApiErrorDto error) => new() { Success = false, Error = error };
}

public sealed class ApiErrorDto
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;
    public IDictionary<string, string[]>? Details { get; init; }
    public string? TraceId { get; init; }
}

