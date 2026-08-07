using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.PaginationUtils;

public class PagedResult<T>(T[] items, int pageNumber, int pageSize, int pageCount)
{
    public T[] Items { get; init; } = items;

    public int PageNumber { get; init; } = pageNumber;

    public int PageSize { get; init; } = pageSize;

    public int PageCount { get; init; } = pageCount;
}