using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Shared.CustomErrors;
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
        var id = command.Id;

        var locResult = await _locationsRepository.GetByIncludingInactiveAsync(
            loc => loc.Id == id,
            cancellationToken);

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

        if (!loc.IsActive)
        {
            return loc.Id;
        }

        var result = await _locationsRepository.Delete(loc, false);

        if (result.IsFailure)
        {
            _logger.LogError(result.Error.Message);
            return result.Error.ToErrors();
        }

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (saveChangesResult.IsFailure)
        {
            _logger.LogError(saveChangesResult.Error.Message);
            return saveChangesResult.Error.ToErrors();
        }

        return loc.Id;
    }
}
