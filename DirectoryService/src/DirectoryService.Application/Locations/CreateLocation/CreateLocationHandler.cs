using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Domain;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationHandler : ICommandHandler<Result<Guid>, CreateLocationCommand>
{
    private readonly ILocationsRepository _repository;

    public CreateLocationHandler(ILocationsRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        // проверка валидности

        // создание Location

        var name = CorrectLocationName.Create(request.CreateLocationDto.Name);

        var address = LocationAddress.Create(request.CreateLocationDto.Address);

        var timezone = Timezone.Create(request.CreateLocationDto.Timezone);

        var result = Result.Combine(name, address, timezone);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);


        var location = Location.Create(
            name.Value,
            address.Value,
            timezone.Value
        );

        // сохранение Location в БД

        await _repository.AddAsync(location, cancellationToken);

        // Логгирование об успешном или неуспешном сохранении

        return Result.Success(location.Id);
    }
}