using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Shared.CustomErrors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.DeletePosition;

public class DeletePositionHandler : ICommandHandler<Result<Guid, Errors>, DeletePositionCommand>
{
    private readonly IPositionsRepository _positionRepository;
    private readonly ILogger<DeletePositionHandler> _logger;
    private readonly ITransactionManager _transactionManager;

    public DeletePositionHandler(
        ITransactionManager transactionManager,
        IPositionsRepository positionsRepository,
        ILogger<DeletePositionHandler> logger)
    {
        _transactionManager = transactionManager;
        _logger = logger;
        _positionRepository = positionsRepository;
    }

    public async Task<Result<Guid, Errors>> Handle(DeletePositionCommand command, CancellationToken cancellationToken)
    {
        var id = command.Id;

        var posResult = await _positionRepository.GetByIncludingInactiveAsync(
            pos => pos.Id == id,
            cancellationToken);

        if (posResult.IsFailure)
        {
            _logger.LogError(posResult.Error.Message);
            return posResult.Error.ToErrors();
        }

        if (!posResult.Value.Any())
        {
            _logger.LogError("No positions found");
            return Error.NotFound("position.not.found", "No positions found").ToErrors();
        }

        var pos = posResult.Value.First();

        if (!pos.IsActive)
        {
            return pos.Id;
        }

        var result = await _positionRepository.Delete(pos, false);

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

        return pos.Id;
    }
}
