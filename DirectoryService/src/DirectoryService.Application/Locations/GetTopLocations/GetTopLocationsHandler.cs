using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.GetTopLocations;

public class GetTopLocationsHandler : IQueryHandler<LocationsTopDto[]>
{
    private readonly IReadDbContext _context;

    public GetTopLocationsHandler(IReadDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LocationsTopDto[], Errors>> Handle(CancellationToken cancellationToken)
    {
        var locations = await (
            from l in _context.LocationsRead
            join dl in _context.DepartmentLocationsRead
                on l.Id equals dl.LocationId into dlGroup
            let departmentCount = dlGroup.Count()
            where departmentCount >= 5
            orderby departmentCount descending
            select new LocationsTopDto(
                l.Id,
                l.Name.Value,
                l.LocationAddress.Value,
                departmentCount
            )
        ).ToArrayAsync(cancellationToken: cancellationToken);
        return locations;
    }
}