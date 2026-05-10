using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Locations;

public class LocationsRepository : ILocationsRepository
{
    private readonly DirectoryServiceDbContext _dbContext;

    public LocationsRepository(DirectoryServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UnitResult<Error>> AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.Locations.AddAsync(location, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return Error.Failure("location.add.failed", "Failed to add location");
        }

        return UnitResult.Success<Error>();
    }

    public async Task<Result<IReadOnlyList<Location>, Error>> GetByAsync(
        Expression<Func<Location, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.Locations
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return Error.Failure("location.get.failed", "Failed to get locations");
        }
    }
}