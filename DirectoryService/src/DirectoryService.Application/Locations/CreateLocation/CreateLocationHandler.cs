using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared.CustomErrors;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Result<Guid, Errors>, CreateLocationCommand>
{
    private readonly ILocationsRepository _repository;
    private readonly ILogger<CreateLocationHandler> _logger;

    public CreateLocationHandler(ILogger<CreateLocationHandler> logger, ILocationsRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        // проверка валидности

        // создание Location

        var name = CorrectLocationName.Create(request.CreateLocationDto.Name);

        var address = LocationAddress.Create(request.CreateLocationDto.Address);

        var timezone = Timezone.Create(request.CreateLocationDto.Timezone);

        if (name.IsFailure)
        {
            _logger.LogError($"Name is invalid");
            return name.Error;
        }

        if (address.IsFailure)
        {
            _logger.LogError($"Address is invalid");
            return address.Error;
        }

        if (timezone.IsFailure)
        {
            _logger.LogError($"Timezone is invalid");
            return timezone.Error;
        }


        var location = Location.Create(
            name.Value,
            address.Value,
            timezone.Value
        );

        // сохранение Location в БД

        await _repository.AddAsync(location, cancellationToken);

        // Логгирование об успешном или неуспешном сохранении

        _logger.LogInformation($"Created location with id {location.Id}");

        return location.Id;
    }
}