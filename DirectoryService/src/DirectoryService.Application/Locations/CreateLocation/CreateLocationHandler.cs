using System.Text.Json;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared.CustomErrors;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Result<Guid, Errors>, CreateLocationCommand>
{
    private readonly ILocationsRepository _repository;
    private readonly ILogger<CreateLocationHandler> _logger;
    private readonly IValidator<CreateLocationCommand> _validator;

    public CreateLocationHandler(ILogger<CreateLocationHandler> logger,
        ILocationsRepository repository,
        IValidator<CreateLocationCommand> validator)
    {
        _logger = logger;
        _validator = validator;
        _repository = repository;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();
            _logger.LogError(errors.First().Message);
            return errors;
        }

        var name = CorrectLocationName.Create(command.CreateLocationDto.Name);

        var address = LocationAddress.Create(command.CreateLocationDto.Address);

        var timezone = Timezone.Create(command.CreateLocationDto.Timezone);

        var location = Location.Create(
            name.Value,
            address.Value,
            timezone.Value
        );

        try
        {
            await _repository.AddAsync(location, cancellationToken);
        }
        catch (Exception e)
        {
            return GeneralErrors.DatabaseOperationFailure().ToErrors();
        }

        _logger.LogInformation($"Created location with id {location.Id}");

        return location.Id;
    }
}