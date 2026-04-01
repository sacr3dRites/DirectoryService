using CSharpFunctionalExtensions;
using DirectoryService.Shared.CustomErrors;

namespace DirectoryService.Application.Abstractions;

public interface ICommandHandler<TResponse, in TCommand>
    where TCommand : ICommand
{
    Task<Result<Guid, Errors>> Handle(TCommand request, CancellationToken cancellationToken);
}