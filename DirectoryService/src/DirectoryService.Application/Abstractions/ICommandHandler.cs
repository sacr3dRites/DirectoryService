using CSharpFunctionalExtensions;

namespace DirectoryService.Application.Abstractions;

public interface ICommandHandler<TResponse, in TCommand>
    where TCommand : ICommand
{
    Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken);
}