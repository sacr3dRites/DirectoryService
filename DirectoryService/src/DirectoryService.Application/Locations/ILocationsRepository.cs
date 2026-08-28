using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Application.Locations;

public interface ILocationsRepository
{
    public Task<UnitResult<Error>> AddAsync(Location location, CancellationToken cancellationToken = default);
    public Task<UnitResult<Error>> Delete(Location location, bool isActive);

    Task<Result<IReadOnlyList<Location>, Error>> GetByAsync(Expression<Func<Location, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Location>, Error>> GetByIncludingInactiveAsync(
        Expression<Func<Location, bool>> predicate,
        CancellationToken cancellationToken = default);
}
