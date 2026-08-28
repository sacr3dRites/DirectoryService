using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Shared.CustomErrors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.DeleteLocation;

public class DeleteLocationHandler : ICommandHandler<Result<Guid, Errors>, DeleteLocationCommand>
{
    private readonly ILocationsRepository _locationsRepository;
    private readonly ILogger<DeleteLocationHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public DeleteLocationHandler(
        ITransactionManager transactionManager,
        ILogger<DeleteLocationHandler> logger,
        ILocationsRepository locationsRepository)
    {
        _transactionManager = transactionManager;
        _logger = logger;
        _locationsRepository = locationsRepository;
    }

    public async Task<Result<Guid, Errors>> Handle(DeleteLocationCommand command, CancellationToken cancellationToken)
    {
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);

        if (transactionScopeResult.IsFailure)
        {
            _logger.LogError(transactionScopeResult.Error.Message);
            return transactionScopeResult.Error.ToErrors();
        }

        using var transactionScope = transactionScopeResult.Value;

        var id = command.Id;

        var locResult = await _locationsRepository.GetByAsync(loc => loc.Id == id);

        if (locResult.IsFailure)
        {
            _logger.LogError(locResult.Error.Message);
            return locResult.Error.ToErrors();
        }

        if (!locResult.Value.Any())
        {
            _logger.LogError("No locations found");
            return Error.NotFound("location.not.found", "No locations found").ToErrors();
        }

        var loc = locResult.Value.First();

        var result = await _locationsRepository.Delete(loc, false);

        if (result.IsFailure)
        {
            _logger.LogError(result.Error.Message);
            return result.Error.ToErrors();
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
        {
            return commitResult.Error.ToErrors();
        }

        return loc.Id;
    }
}