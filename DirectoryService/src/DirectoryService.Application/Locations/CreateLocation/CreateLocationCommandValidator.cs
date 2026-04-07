using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.CreateLocationDto.Name)
            .MustBeValueObject(CorrectLocationName.Create);

        RuleFor(x => x.CreateLocationDto.Address)
            .MustBeValueObject(LocationAddress.Create);

        RuleFor(x => x.CreateLocationDto.Timezone)
            .MustBeValueObject(Timezone.Create);
    }
}