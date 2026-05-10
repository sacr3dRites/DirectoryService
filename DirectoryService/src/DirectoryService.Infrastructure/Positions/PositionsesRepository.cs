using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Positions;

public class PositionsesRepository : IPositionsRepository
{
    private readonly DirectoryServiceDbContext _context;

    public PositionsesRepository(DirectoryServiceDbContext context)
    {
        _context = context;
    }

    public async Task<UnitResult<Error>> AddAsync(Position position, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Positions.AddAsync(position, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return Error.Failure("position.add.failed", "Failed to add position");
        }

        return UnitResult.Success<Error>();
    }

    public async Task<Result<IReadOnlyList<Position>, Error>> GetByAsync(Expression<Func<Position, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Positions
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return Error.Failure("position.get.failed", "Failed to get positions");
        }
    }
}