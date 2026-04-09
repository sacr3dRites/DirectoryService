using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    public Task AddAsync(Location location, CancellationToken cancellationToken = default);

    public Task<Location?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    public Task<List<Location>> GetExistingAsync(Guid[] ids, CancellationToken cancellationToken = default);
}