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

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations.FindAsync(id, cancellationToken);
    }

    public async Task<List<Location>> GetExistingAsync(Guid[] ids, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Locations
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}