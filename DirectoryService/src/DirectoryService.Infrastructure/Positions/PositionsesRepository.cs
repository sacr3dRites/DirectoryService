using DirectoryService.Application.Positions;
using DirectoryService.Domain.Positions;

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
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<Position?> GetByName(string name)
    {
        var position = _context.Positions.FirstOrDefault(x => x.Name.Equals(name));

        return Task.FromResult(position);
    }
}