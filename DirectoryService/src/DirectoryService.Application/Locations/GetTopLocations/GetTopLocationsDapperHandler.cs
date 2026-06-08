using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Application.Locations.GetTopLocations;

public class GetTopLocationsDapperHandler : IQueryHandler<LocationsTopDto[]>
{
    private readonly INpgSqlConnectionFactory _connectionFactory;

    public GetTopLocationsDapperHandler(INpgSqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<LocationsTopDto[], Errors>> Handle(CancellationToken cancellationToken)
    {
        const string query = """
                             SELECT
                             id AS "Id", 
                             name AS "LocationName", 
                             location_address AS "Address",
                             COUNT(department_locations.department_location_id)::int as DepartmentCount
                             FROM locations
                             LEFT JOIN department_locations ON locations.id = department_locations.location_id
                             GROUP BY id, name, location_address
                             HAVING COUNT(department_locations.department_location_id) >= 5
                             ORDER BY DepartmentCount desc
                             """;
        var dbConn = await _connectionFactory.CreateConnectionAsync();

        LocationsTopDto[] locationsTopDtos = (await dbConn.QueryAsync<LocationsTopDto>(query)).ToArray();

        return locationsTopDtos;
    }
}