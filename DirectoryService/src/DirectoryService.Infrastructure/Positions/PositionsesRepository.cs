using System.Linq.Expressions;
using DirectoryService.Application.Positions;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Positions;

public class PositionsesRepository : IPositionsRepository
{
    private readonly DirectoryServiceDbContext _context;

    public PositionsesRepository(DirectoryServiceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Position position, CancellationToken cancellationToken = default)
    {
        await _context.Positions.AddAsync(position, cancellationToken);
    }

    public async Task<IReadOnlyList<Position>> GetByAsync(Expression<Func<Position, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Positions
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }
}