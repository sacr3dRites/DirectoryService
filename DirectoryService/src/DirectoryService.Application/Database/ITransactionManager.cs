using CSharpFunctionalExtensions;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Application.Database;

public interface ITransactionManager
{
    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);

    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken);
}