using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Shared;
using DirectoryService.Shared.EndpointResults;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Result<Guid, Errors>, CreateLocationCommand>
{
    private readonly ILocationsRepository _repository;

    public CreateLocationHandler(ILocationsRepository repository)
    {
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
            return name.Error;

        if (address.IsFailure)
            return address.Error;

        if (timezone.IsFailure)
            return timezone.Error;


        var location = Location.Create(
            name.Value,
            address.Value,
            timezone.Value
        );

        // сохранение Location в БД

        await _repository.AddAsync(location, cancellationToken);

        // Логгирование об успешном или неуспешном сохранении

        return location.Id;
    }
}