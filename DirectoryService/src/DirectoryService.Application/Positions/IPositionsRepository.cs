using System.Linq.Expressions;
using DirectoryService.Domain.Positions;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    public Task AddAsync(Position position, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Position>> GetByAsync(Expression<Func<Position, bool>> predicate,
        CancellationToken cancellationToken = default);
}