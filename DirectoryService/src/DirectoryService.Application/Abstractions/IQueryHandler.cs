using CSharpFunctionalExtensions;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Application.Abstractions;

public interface IQueryHandler<T>
{
    Task<Result<T, Errors>> Handle(CancellationToken cancellationToken);
}

public interface IQueryHandler<in TQuery, TResult>
{
    Task<Result<TResult, Errors>> Handle(
        TQuery query,
        CancellationToken cancellationToken);
}