using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Shared;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Application.Locations.GetLocations;

public class GetLocationsHandler : IQueryHandler<GetLocationsQuery, PagedResult<LocationListItemDto>>
{
    private readonly INpgSqlConnectionFactory _connectionFactory;

    public GetLocationsHandler(INpgSqlConnectionFactory npgSqlConnectionFactory)
    {
        _connectionFactory = npgSqlConnectionFactory;
    }

    public async Task<Result<PagedResult<LocationListItemDto>, Errors>> Handle(GetLocationsQuery query,
        CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();

        parameters.Add(
            "Search",
            string.IsNullOrWhiteSpace(query.Search)
                ? null
                : $"%{query.Search.Trim()}%",
            DbType.String
        );

        if (query.PageSize > 0)
        {
            parameters.Add("PageSize", query.PageSize);
        }

        if (query.Page > 0)
        {
            parameters.Add("Offset", query.PageSize * (query.Page - 1));
        }

        var sortBy = query.SortBy switch
        {
            SortBy.CreatedAt => "created_at",
            SortBy.Name => "name",
            SortBy.UpdatedAt => "updated_at"
        };

        var sortDirection = query.SortDirection switch
        {
            SortDirection.Asc => "ASC",
            SortDirection.Desc => "DESC"
        };

        parameters.Add("minDepartmentCount", query.minDepartmentCount);

        var sqlLocationQuery = $"""
                                WITH filtered_locations AS (
                                SELECT l.id, l.name, l.location_address, l.created_at, l.updated_at, COUNT(dl.department_location_id)::int AS department_count
                                FROM locations l
                                LEFT JOIN department_locations AS dl ON dl.location_id = l.id
                                WHERE @Search is NULL or name ILIKE @Search
                                GROUP BY
                                    l.id,
                                    l.name,
                                    l.location_address,
                                    l.created_at,
                                    l.updated_at
                                HAVING @MinDepartmentCount IS NULL
                                    OR COUNT(dl.department_location_id) >= @MinDepartmentCount
                                ),
                                totalCount AS (
                                SELECT COUNT(*)::int AS total_count
                                FROM filtered_locations
                                ),
                                paged AS (
                                SELECT f.*,
                                ROW_NUMBER() OVER (
                                ORDER BY {sortBy} {sortDirection}, f.id) AS page_order
                                FROM filtered_locations f
                                ORDER BY page_order
                                LIMIT @PageSize
                                OFFSET @Offset
                                )
                                SELECT
                                p.id AS "Id",
                                    p.name AS "Name",
                                    p.location_address AS "Address",
                                    p.created_at AS "CreatedAt",
                                    p.department_count AS "DepartmentCount",
                                    t.total_count AS "TotalCount"
                                FROM paged p, totalCount t
                                ORDER BY p.page_order NULLS LAST;
                                """;
        using var dbConn = await _connectionFactory.CreateConnectionAsync();
        var locationArr = (await dbConn.QueryAsync<LocationListItemDto>(sqlLocationQuery, parameters)).ToArray();


        if (locationArr.Length > 0)
        {
            var pageCount = (int)Math.Ceiling(locationArr.First().TotalCount / (double)query.PageSize);
            return new PagedResult<LocationListItemDto>(locationArr, query.Page, query.PageSize, pageCount,
                locationArr.First().TotalCount);
        }

        return new PagedResult<LocationListItemDto>(Array.Empty<LocationListItemDto>(), query.Page, query.PageSize, 0,
            0);
    }
}