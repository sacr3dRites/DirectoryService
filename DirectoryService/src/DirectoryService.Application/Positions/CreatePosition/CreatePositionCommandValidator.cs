using DirectoryService.Application.Departments;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Positions;
using FluentValidation;

namespace DirectoryService.Application.Positions.CreatePosition;

public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
{
    public CreatePositionCommandValidator()
    {
        RuleFor(x => x.CreatePositionRequest.Name)
            .NotEmpty()
            .MustBeValueObject(CorrectPositionName.Create);
        ;

        RuleFor(x => x.CreatePositionRequest.Description)
            .NotEmpty()
            .MaximumLength(1000)
            .WithMessage("Description cannot be empty or longer than 1000 characters");


        RuleFor(X => X.CreatePositionRequest.DepartmentIds)
            .NotNull()
            .WithMessage("The list of departments contains non-existent, inactive or duplicate elements");
    }
}