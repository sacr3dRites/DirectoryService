using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Shared;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Departments.GetAllDepartmentChildren;

public class
    GetAllDepartmentChildrenHandler : IQueryHandler<GetAllDepartmentChildrenQuery, PagedResult<DepartmentTree>>
{
    private readonly IReadDbContext _context;
    private readonly ILogger<GetAllDepartmentChildrenHandler> _logger;
    private readonly INpgSqlConnectionFactory _connectionFactory;

    public GetAllDepartmentChildrenHandler(
        INpgSqlConnectionFactory connectionFactory,
        IReadDbContext context,
        ILogger<GetAllDepartmentChildrenHandler> logger)
    {
        _connectionFactory = connectionFactory;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<DepartmentTree>, Errors>> Handle(GetAllDepartmentChildrenQuery query,
        CancellationToken cancellationToken)
    {
        var dep = await _context.DepartmentsRead.FirstOrDefaultAsync(x => x.Id == query.DepartmentId,
            cancellationToken);

        if (dep == null)
        {
            _logger.LogError($"Department with id {query.DepartmentId} not found");
            return GeneralErrors.NotFound(query.DepartmentId).ToErrors();
        }

        if (query.Page < 1 || query.PageSize is < 1 or > 100)
            return GeneralErrors.ValueIsInvalid("параметры пагинации").ToErrors();
        
        if (!Enum.IsDefined(query.SortBy))
            return GeneralErrors.ValueIsInvalid(nameof(query.SortBy)).ToErrors();

        if (!Enum.IsDefined(query.SortDirection))
            return GeneralErrors.ValueIsInvalid(nameof(query.SortDirection)).ToErrors();

        var sortBy = query.SortBy switch
        {
            SortBy.Name => "name",
            SortBy.CreatedAt => "created_at",
            SortBy.UpdatedAt => "updated_at",
            _ => throw new ArgumentOutOfRangeException(nameof(query.SortBy))
        };

        var sortDir = query.SortDirection switch
        {
            SortDirection.Asc => "ASC",
            SortDirection.Desc => "DESC",
            _ => throw new ArgumentOutOfRangeException(nameof(query.SortDirection))
        };

        var departmentChildrenListItemQuery = $"""
                                               WITH parent_dep AS(
                                               SELECT id, path
                                               FROM departments
                                               WHERE is_active = TRUE AND id = @DepartmentId
                                               )
                                               SELECT 
                                               d.id,
                                               d.name,
                                               d.identifier,
                                               d.path,
                                               d.depth::int AS Depth, 
                                               stats.children_count > 0 as HasChildren,
                                               stats.children_count AS ChildrenCount  
                                               FROM departments d
                                               JOIN parent_dep dp ON dp.id = d.parent_id
                                               CROSS JOIN LATERAL (
                                               SELECT COUNT(*)::int AS children_count
                                               FROM departments child
                                               WHERE child.parent_id = d.id
                                               AND child.is_active = TRUE
                                               ) stats
                                               WHERE d.is_active = TRUE AND d.path <@ dp.path AND dp.id <> d.id
                                               ORDER BY {sortBy} {sortDir}, d.id
                                               LIMIT @PageSize
                                               OFFSET @Offset
                                               """;

        using var dbConn = await _connectionFactory.CreateConnectionAsync();

        var parameters = new
        {
            DepartmentId = query.DepartmentId,
            query.Page,
            query.PageSize,
            Offset = query.PageSize * (query.Page - 1)
        };

        var command = new CommandDefinition(
            departmentChildrenListItemQuery, parameters, cancellationToken: cancellationToken
        );

        var departmentTreesArray =
            (await dbConn.QueryAsync<DepartmentTree>(command)).ToArray();

        var totalCountQuery = """
                              SELECT COUNT(*)
                              FROM departments child
                              JOIN departments parent ON child.path <@ parent.path
                              AND nlevel(child.path) = nlevel(parent.path)+1
                              WHERE parent.id = @DepartmentId
                              AND child.id <> parent.id
                              AND parent.is_active = TRUE
                              AND child.is_active = TRUE
                              """;

        var countCommand = new CommandDefinition(
            totalCountQuery, new { DepartmentId = query.DepartmentId }, cancellationToken: cancellationToken
        );

        var totalCount = await dbConn.QuerySingleAsync<int>(countCommand);

        var PageCount = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        return new PagedResult<DepartmentTree>(departmentTreesArray, query.Page, query.PageSize, PageCount,
            totalCount);
    }
}