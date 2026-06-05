using CSharpFunctionalExtensions;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Application.Abstractions;

public interface IQueryHandler<T>
{
    Task<Result<T, Errors>> Handle(Guid id, CancellationToken cancellationToken);
}