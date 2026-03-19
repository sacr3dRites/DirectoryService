using DirectoryService.Application.Abstractions;
using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;
using DirectoryService.Domain.SharedValueObjects;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
{
    private readonly ILocationsRepository _repository;

    public CreateLocationHandler(ILocationsRepository repository)
    {
        _repository = repository;
    }

    public Task<Guid> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        // проверка валидности

        // создание Location

        var name = CorrectName.Create(
            request.CreateLocationDto.Name,
            DirectoryServiceConstants.LOCATION_NAME_MAX_LENGTH);

        var address = LocationAddress.Create(request.CreateLocationDto.Address);

        var timezone = Timezone.Create(request.CreateLocationDto.Timezone);

        if (timezone.IsFailure)
        {
            throw new Exception(timezone.Error);
        }

        if (name.IsFailure)
        {
            throw new Exception(name.Error);
        }

        if (address.IsFailure)
        {
            throw new Exception(address.Error);
        }


        var location = Location.Create(
            name.Value,
            address.Value,
            timezone.Value
        );

        // сохранение Location в БД

        _repository.AddAsync(location, cancellationToken);

        // Логгирование об успешном или неуспешном сохранении

        return Task.FromResult(location.Id);
    }
}