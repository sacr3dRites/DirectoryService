using DirectoryService.Application.Validation;
using DirectoryService.Domain.Locations.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.CreateLocationRequest.Name)
            .NotEmpty()
            .MustBeValueObject(CorrectLocationName.Create);

        RuleFor(x => x.CreateLocationRequest.Address)
            .NotEmpty()
            .MustBeValueObject(LocationAddress.Create);

        RuleFor(x => x.CreateLocationRequest.Timezone)
            .NotEmpty()
            .MustBeValueObject(Timezone.Create);
    }
}