using System.Linq.Expressions;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    public Task AddAsync(Location location, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Location>> GetByAsync(Expression<Func<Location, bool>> predicate, CancellationToken cancellationToken = default);
}