using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations;
using DirectoryService.Infrastructure.Database;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application.Locations.GetLocation;

public class LocationQueryByIdHandler : IQueryByIdHandler<LocationDto>
{
    private readonly IReadDbContext _context;

    public LocationQueryByIdHandler(IReadDbContext context)
    {
        _context = context;
    }

    public async Task<Result<LocationDto, Errors>> Handle(Guid id, CancellationToken cancellationToken)
    {
        var location = await _context.LocationsRead
            .FirstOrDefaultAsync(location => location.Id == id, cancellationToken: cancellationToken);

        if (location == null)
        {
            return GeneralErrors.NotFound().ToErrors();
        }

        return new LocationDto(
            location.Id,
            location.Name.Value,
            location.LocationAddress.Value,
            location.Timezone.Name,
            location.IsActive,
            location.CreatedAt,
            location.UpdatedAt
        );
    }
}