using System.Linq.Expressions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

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
    }

    public async Task<List<Location>> GetByAsync(
        Expression<Func<Location, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }
}