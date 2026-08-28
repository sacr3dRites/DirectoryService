using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    public Task<UnitResult<Error>> AddAsync(Position position, CancellationToken cancellationToken = default);
    Task<UnitResult<Error>> Delete(Position pos, bool isActive);

    Task<Result<IReadOnlyList<Position>, Error>> GetByAsync(Expression<Func<Position, bool>> predicate,
        CancellationToken cancellationToken = default);
}