using DirectoryService.Domain.Positions;

namespace DirectoryService.Application.Positions;

public interface IPositionsRepository
{
    public Task AddAsync(Position position, CancellationToken cancellationToken = default);

    public Task<Position?> GetByName(string name);
}