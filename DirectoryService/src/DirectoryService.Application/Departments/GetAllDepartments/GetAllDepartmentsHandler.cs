using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Contracts.Departments;
using DirectoryService.Shared.CustomErrors;
using Microsoft.AspNetCore.Connections;

namespace DirectoryService.Application.Departments.GetAllDepartments;

public class GetAllDepartmentsHandler : IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentListItemDto>>
{
    private readonly INpgSqlConnectionFactory _connectionFactory;

    public GetAllDepartmentsHandler(INpgSqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<PagedResult<DepartmentListItemDto>, Errors>> Handle(GetDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
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

        var pageCountQuery = """
                             SELECT COUNT(*) FROM departments
                             WHERE @Search is NULL or name ILIKE @Search
                             """;

        var departmentListItemQuery = $"""
                                       SELECT id, name, path, created_at as CreatedAt
                                       FROM departments
                                       WHERE @Search is NULL or name ILIKE @Search
                                       ORDER BY {sortBy} {sortDir}
                                       LIMIT @PageSize
                                       OFFSET @Offset
                                       """;

        using var dbConn = await _connectionFactory.CreateConnectionAsync();

        var parameters = new
        {
            Search = query.Search is null
                ? null
                : $"%{query.Search}%",
            query.Page,
            query.PageSize,
            Offset = query.PageSize * (query.Page - 1)
        };

        var departmentListItemsArray =
            (await dbConn.QueryAsync<DepartmentListItemDto>(departmentListItemQuery, parameters)).ToArray();

        var totalCount = await dbConn.QuerySingleAsync<int>(pageCountQuery, new
        {
            Search = query.Search is null
                ? null
                : $"%{query.Search}%",
        });

        var PageCount = (int)Math.Ceiling(totalCount / (double)query.PageSize);
        return new PagedResult<DepartmentListItemDto>(departmentListItemsArray, query.Page, query.PageSize, PageCount,
            totalCount);
    }
}