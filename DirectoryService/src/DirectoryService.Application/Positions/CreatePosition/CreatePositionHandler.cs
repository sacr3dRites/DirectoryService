using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared.CustomErrors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionHandler : ICommandHandler<Result<Guid, Errors>, CreatePositionCommand>
{
    private readonly IValidator<CreatePositionCommand> _validator;
    private readonly IPositionsRepository _positionsRepository;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(
        IValidator<CreatePositionCommand> validator,
        IPositionsRepository positionsRepository,
        ILogger<CreatePositionHandler> logger)
    {
        _validator = validator;
        _positionsRepository = positionsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            _logger.LogError(errors.First().Message);
            return errors;
        }

        var name = CorrectPositionName.Create(command.CreatePositionDto.Name).Value;
        var description = command.CreatePositionDto.Description;

        var positionResult = Position.Create(name, description);

        if (positionResult.IsFailure)
        {
            _logger.LogError(positionResult.Error.FirstOrDefault().Message);
            return positionResult.Error;
        }

        var position = positionResult.Value;

        try
        {
            _positionsRepository.AddAsync(position, cancellationToken);
        }
        catch (Exception e)
        {
            return GeneralErrors.DatabaseOperationFailure().ToErrors();
        }

        _logger.LogInformation($"Position {name} created");

        return position.Id;
    }
}