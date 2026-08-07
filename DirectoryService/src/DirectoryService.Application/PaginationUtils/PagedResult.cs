using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.PaginationUtils;

public class PagedResult<T>(T[] items, int pageNumber, int pageSize, int pageCount, int totalCount)
{
    public T[] Items { get; init; } = items;

    public int PageNumber { get; init; } = pageNumber;

    public int PageSize { get; init; } = pageSize;

    public int PageCount { get; init; } = pageCount;

    public int TotalCount { get; init; } = totalCount;
}