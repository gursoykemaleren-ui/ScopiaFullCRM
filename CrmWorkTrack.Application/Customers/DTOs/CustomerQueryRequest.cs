public record CustomerQueryRequest(
    int Page = 1,
    int PageSize = 10,
    bool? IsActive = true,
    string? Q = null,
    string? SortBy = "createdAt",
    string? SortDir = "desc"
);
