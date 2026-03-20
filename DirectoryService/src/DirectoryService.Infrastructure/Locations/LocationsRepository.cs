using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Infrastructure.Locations;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public LocationsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        await _dbContext.Locations.AddAsync(location, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}