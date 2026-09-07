using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Shared;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Departments.GetDepartmentTrees;

public class
    GetAllRootDepartmentTreesHandler : IQueryHandler<GetAllRootDepartmentTreesQuery, PagedResult<DepartmentTree>>
{
    private readonly IReadDbContext _context;

    public GetAllRootDepartmentTreesHandler(IReadDbContext readDbContext)
    {
        _context = readDbContext;
    }

    public async Task<Result<PagedResult<DepartmentTree>, Errors>> Handle(GetAllRootDepartmentTreesQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page < 1 || query.PageSize is < 1 or > 100)
            return GeneralErrors.ValueIsInvalid("параметры пагинации").ToErrors();

        var rootDeps = _context.DepartmentsRead
            .Where(dep => EF.Property<Guid?>(dep, "parent_id") == null);

        if (!Enum.IsDefined(query.SortBy))
            return GeneralErrors.ValueIsInvalid(nameof(query.SortBy)).ToErrors();

        if (!Enum.IsDefined(query.SortDirection))
            return GeneralErrors.ValueIsInvalid(nameof(query.SortDirection)).ToErrors();

        var sortDescending = query.SortDirection switch
        {
            SortDirection.Asc => false,
            SortDirection.Desc => true,
            _ => throw new ArgumentOutOfRangeException(nameof(query.SortDirection))
        };

        var orderedRootDeps = query.SortBy switch
        {
            SortBy.Name => sortDescending
                ? rootDeps.OrderByDescending(dep => dep.Name.Value)
                : rootDeps.OrderBy(dep => dep.Name.Value),
            SortBy.CreatedAt => sortDescending
                ? rootDeps.OrderByDescending(dep => dep.CreatedAt)
                : rootDeps.OrderBy(dep => dep.CreatedAt),
            SortBy.UpdatedAt => sortDescending
                ? rootDeps.OrderByDescending(dep => dep.UpdatedAt)
                : rootDeps.OrderBy(dep => dep.UpdatedAt),
            _ => throw new ArgumentOutOfRangeException(nameof(query.SortBy))
        };

        var totalCount = await rootDeps.CountAsync(cancellationToken);

        var trees = await orderedRootDeps
            .ThenBy(dep => dep.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(dep => new DepartmentTree(
                dep.Id,
                dep.Name.Value,
                dep.Identifier.Value,
                dep.Path.Value,
                dep.Depth,
                dep.Children.Any(child => child.IsActive),
                dep.Children.Count(child => child.IsActive)
            ))
            .ToArrayAsync(cancellationToken);

        var pageCount = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PagedResult<DepartmentTree>(trees, query.Page, query.PageSize, pageCount, totalCount);
    }
}