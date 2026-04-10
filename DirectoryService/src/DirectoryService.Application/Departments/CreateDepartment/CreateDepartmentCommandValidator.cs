using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Domain.Departments.ValueObjects;
using FluentValidation;

namespace DirectoryService.Application.Departments.CreateDepartment;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    private readonly IDepartmentsRepository _departmentRepo;
    private readonly ILocationsRepository _locationRepo;

    public CreateDepartmentCommandValidator(
        IDepartmentsRepository departmentsRepository,
        ILocationsRepository locationsRepository)
    {
        _departmentRepo = departmentsRepository;
        _locationRepo = locationsRepository;
    }

    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.CreateDepartmentDto.Name)
            .NotEmpty()
            .MustBeValueObject(CorrectDepartmentName.Create);

        RuleFor(x => x.CreateDepartmentDto.Identifier)
            .NotEmpty()
            .MustBeValueObject(DepartmentIdentifier.Create);

        RuleFor(x => x.CreateDepartmentDto.ParentId)
            .MustAsync(async (parentId, cancellationToken) =>
            {
                if (parentId == null)
                    return true;

                var result = await _departmentRepo.FindByIdAsync(parentId.Value, cancellationToken);
                return result is not null;
            })
            .WithMessage("Parent Id not found");

        RuleFor(x => x.CreateDepartmentDto.LocationIds)
            .MustAsync(async (locationIds, cancellationToken) =>
            {
                if (locationIds == null || !locationIds.Any())
                    return false;

                if (locationIds.Count() != locationIds.Distinct().Count())
                    return false;

                var existingLocations = await _locationRepo.GetExistingAsync(locationIds, cancellationToken);

                var existingIds = existingLocations
                    .Select(x => x.Id)
                    .ToHashSet();
                return locationIds.All(x => existingIds.Contains(x));
            })
            .WithMessage("One or several locations were not found");
    }
}