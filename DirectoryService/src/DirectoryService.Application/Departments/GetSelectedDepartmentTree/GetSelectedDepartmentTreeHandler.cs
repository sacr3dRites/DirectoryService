using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Contracts.Departments;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Departments.GetSelectedDepartmentTree;

public class
    GetSelectedDepartmentTreeHandler : IQueryHandler<GetSelectedDepartmentTreeQuery, PagedResult<DepartmentTree>>
{
    private readonly IReadDbContext _context;

    public GetSelectedDepartmentTreeHandler(IReadDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<DepartmentTree>, Errors>> Handle(GetSelectedDepartmentTreeQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page < 1 || query.PageSize is < 1 or > 100)
            return GeneralErrors.ValueIsInvalid("параметры пагинации").ToErrors();

        var term = query.Search?.Trim();

        if (string.IsNullOrEmpty(term) || term.Length < 2)
            return GeneralErrors.ValueIsInvalid(term).ToErrors();

        var normalizedTerm = term.ToLower();


        var trees = await _context.DepartmentsRead
            .Where(dep => dep.Name.Value.ToLower().Contains(normalizedTerm))
            .OrderBy(dep => dep.Name.Value)
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

        var totalCount = trees.Length;

        var pageCount = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        return new PagedResult<DepartmentTree>(trees, query.Page, query.PageSize, pageCount, totalCount);
    }
}