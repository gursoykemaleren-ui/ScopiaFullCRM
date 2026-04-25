
namespace CrmWorkTrack.Application.Common.Pagination;

public class PagedResult<T>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public PagedResult()
    {
    }

    public PagedResult(int page, int pageSize, int totalCount, IReadOnlyList<T> items)
    {
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
        Items = items;
    }
}