using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Contracts.Departments;
using DirectoryService.Infrastructure.Database;
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
            .Where(dep => dep.Parent == null);

        var totalCount = await rootDeps.CountAsync(cancellationToken);

        var trees = await rootDeps
            .OrderBy(dep => query.SortBy)
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