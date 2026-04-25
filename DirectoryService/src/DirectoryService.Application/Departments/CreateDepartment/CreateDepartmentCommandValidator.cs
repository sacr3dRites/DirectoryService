using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.CreateDepartmentRequest.Name)
            .NotEmpty()
            .MustBeValueObject(CorrectDepartmentName.Create);

        RuleFor(x => x.CreateDepartmentRequest.Identifier)
            .NotEmpty()
            .MustBeValueObject(DepartmentIdentifier.Create);

        RuleFor(x => x.CreateDepartmentRequest.ParentId);

        RuleFor(x => x.CreateDepartmentRequest.LocationIds)
            .NotNull()
            .WithMessage("One or several locations were not found");
    }
}